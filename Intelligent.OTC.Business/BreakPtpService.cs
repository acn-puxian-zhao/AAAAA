using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Repositories;
using System.Data.Entity;
using Intelligent.OTC.Business;
using Intelligent.OTC.Domain;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.Utils;

namespace Intelligent.OTC.Business
{
    public class BreakPtpService
    {
        public OTCRepository CommonRep { get; set; }
        public XcceleratorRepository XRep { get; set; }

        string defaultCust = "Default999";

        public int getCurrentPeriod()
        {
            PeroidService service = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");
            PeriodControl period = new PeriodControl();
            period = service.getcurrentPeroid();
            int CurrentPeriod = 0;
            if (period != null) { CurrentPeriod = period.Id; }
            return CurrentPeriod;
        }

        public IEnumerable<CustomerCommon> GetBreakPTP()
        {
            string deal = AppContext.Current.User.Deal.ToString();
            string eid = AppContext.Current.User.EID.ToString();
            DateTime dt = AppContext.Current.User.Now;
            string invoOpen=Helper .EnumToCode<InvoiceStatus>(InvoiceStatus.Open);
            string invoPTP = Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.PTP);
            string invoBrPTP = Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Broken_PTP);
            var reinvo =
                 from invo in CommonRep.GetQueryable<InvoiceAging>()
                 where (invo.States == invoOpen && invo.PtpDate != null && invo.PtpDate < dt.Date)
                        || (invo.States == invoPTP && invo.PtpDate != null && invo.PtpDate < dt.Date)
                        || invo.States == invoBrPTP
                        && invo.Deal == deal
                 group invo by new { invo.CustomerNum, invo.Deal } into g
                 select new { CustomerNum = g.Key.CustomerNum, Deal = g.Key.Deal };

            var result = from cc in CommonRep.GetQueryable<CustomerCommon>()
                         where cc.Deal == deal && cc.Operator == eid
                         join invo in reinvo on cc.CustomerNum equals invo.CustomerNum
                         select cc;


            return result.AsEnumerable();

        }

        public void confirmBreakPTP(InvoiceLog invoLogInstance)
        {
            string invoBrPTP = Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Broken_PTP);
            var invos = (from invo in CommonRep.GetQueryable<InvoiceAging>()
                         where invo.States != invoBrPTP && invoLogInstance.invoiceIds.Contains(invo.Id)
                         select invo.Id);
            //aviod 2nd brokensent=>breakPTP
            invoLogInstance.invoiceIds = invos.ToArray();

            //1.insert invoiceLog
            insertInvoiceLog(invoLogInstance);

            //2.update invoiceAging states
            changeInvoiceStates(invoLogInstance);

            CommonRep.Commit();
        }

        public void changeStatus(int[] ids, string NewStatus, string mailId)
        {
            InvoiceLog invoLogInstance = new InvoiceLog();
            invoLogInstance.invoiceIds = ids;
            invoLogInstance.LogAction = "BREAK PTP";
            invoLogInstance.LogType = "7";
            invoLogInstance.NewStatus = Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Broken_PTP);
            invoLogInstance.NewTrack = NewStatus;
            invoLogInstance.ContactPerson = "";
            invoLogInstance.ProofId = mailId;
            invoLogInstance.Discription = "";

            //1.insert invoiceLog
            insertInvoiceLog(invoLogInstance);

            //2.update invoiceAging states
            changeInvoiceStatesByIds(ids, NewStatus);

            //3.update the T_alert ActionDate
            if (NewStatus == Helper.EnumToCode<TrackStatus>(TrackStatus.Wait_for_2nd_Time_Dispute_respond))
            {
                int currentPeriodId = getCurrentPeriod();
                List<string> customerLi = new List<string>();
                if (ids != null && ids.Count() > 0)
                {
                    var customer = CommonRep.FindBy<InvoiceAging>(ids[0]);
                    //get the legalEntity
                    var lstLegalEntity = (from invo in CommonRep.GetQueryable<InvoiceAging>()
                                          where ids.Contains(invo.Id)
                                          select invo.LegalEntity).Distinct();
                    foreach (var leg in lstLegalEntity)
                    {
                        CollectorAlert aler = CommonRep.GetQueryable<CollectorAlert>().Where(a => a.CustomerNum == customer.CustomerNum &&
                             a.LegalEntity == leg && a.AlertType == 4 && a.PeriodId == currentPeriodId).FirstOrDefault();
                        if (aler != null)
                        {
                            aler.ActionDate = GetHoldDate(customer.CustomerNum, leg, CurrentTime);
                        }
                    }
                }
            }
            CommonRep.Commit();
        }

        private DateTime GetHoldDate(string customerNum, string legalEntity, DateTime dt)
        {
            dt = dt.Date;

            PeroidService perService = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");

            //取得当前peroid
            PeriodControl per = perService.getcurrentPeroid();
            try
            {

                //取得所有CustomerPaymentCircle by CustomerNum
                List<CustomerPaymentCircle> custPayments = CommonRep.GetQueryable<CustomerPaymentCircle>()
                                                     .Where(o => o.Deal == CurrentDeal
                                                         && o.PaymentDay >= CurrentTime
                                                         && o.PaymentDay <= per.PeriodEnd
                                                         && o.CustomerNum == customerNum
                                                         && o.LegalEntity == legalEntity).ToList();

                //取得所有DunningReminderConfig by CustomerNum
                List<DunningReminderConfig> duns = CommonRep.GetQueryable<DunningReminderConfig>()
                                                    .Where(o => (o.Deal == CurrentDeal
                                                    && o.CustomerNum == customerNum) ||
                                                    o.VRClass != null || o.CustomerNum == defaultCust).ToList();

                //取得所有CustomerLevel by CustomerNum
                List<CustomerLevelView> custLevels = CommonRep.GetQueryable<CustomerLevelView>()
                            .Where(o => o.Deal == CurrentDeal
                            && o.CustomerNum == customerNum).ToList();

                //取得所有Holidy
                List<T_MD_EXCEPTIONS> excepts = XRep.GetQueryable<T_MD_EXCEPTIONS>().ToList();

                CustomerPaymentCircle custPayment = new CustomerPaymentCircle();

                if (custPayments.Count > 0)
                {
                    custPayment = custPayments.Where(o => o.CustomerNum == customerNum && o.LegalEntity == legalEntity).Select(o => o).OrderByDescending(o => o.PaymentDay).FirstOrDefault();
                }

                DunningReminderConfig dun = GetCustomerSpecificReminderConfig(customerNum, legalEntity, duns, custLevels);

                DunningReminderService service = SpringFactory.GetObjectImpl<DunningReminderService>("DunningReminderService");
                return service.reminderDateGet(dt, excepts, dun, "3", per, "3", custPayment);
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }
    
        private static DunningReminderConfig GetCustomerSpecificReminderConfig(string customerNum, string legalEntity, List<DunningReminderConfig> duns, List<CustomerLevelView> custLevels)
        {
            //取得对应DunningReminderConfig by CustomerNum && LegalEntity
            DunningReminderConfig dun = duns.Where(o => o.CustomerNum == customerNum && o.LegalEntity == legalEntity).Select(o => o).FirstOrDefault();

            //没有得到DunningReminderConfig 的时候，取得customer的level对应的DunningReminderConfig
            if (dun == null)
            {
                CustomerLevelView custLevel = custLevels.Where(o => o.CustomerNum == customerNum).ToList().FirstOrDefault();
                if (custLevel == null)//customer的level没有取到的时候，取得defult DunningReminderConfig
                {
                    dun = duns.Where(o => o.CustomerNum == "Default999").Select(o => o).FirstOrDefault();
                }
                else
                {
                    var strClass = custLevel.ClassLevel + custLevel.RiskLevel;
                    dun = duns.Where(o => o.VRClass == strClass).Select(o => o).FirstOrDefault();
                }
            }
            return dun;
        }

        public string CurrentDeal
        {
            get
            {
                return AppContext.Current.User.Deal.ToString();
            }
        }

        public string CurrentUser
        {
            get
            {
                return AppContext.Current.User.EID.ToString();
            }
        }

        public DateTime CurrentTime
        {
            get
            {
                return AppContext.Current.User.Now;
            }
        }


        public void changeInvoiceStates(InvoiceLog invoLogInstance)
        {
            List<InvoiceAging> invos = (from invo in CommonRep.GetQueryable<InvoiceAging>()
                                        where invoLogInstance.invoiceIds.Contains(invo.Id)
                                        select invo).ToList<InvoiceAging>();
            foreach (var invo in invos)
            {
                invo.States = invoLogInstance.NewStatus;
                invo.TrackStates = invoLogInstance.NewTrack;
                invo.UpdateDate = AppContext.Current.User.Now;
            }

        }
        public void changeInvoiceStatesByIds(int[] ids,string NewStatus)
        {
            List<InvoiceAging> invos = (from invo in CommonRep.GetQueryable<InvoiceAging>()
                                        where ids.Contains(invo.Id)
                                        select invo).ToList<InvoiceAging>();
            foreach (var invo in invos)
            {
                invo.States = Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Broken_PTP);
                invo.TrackStates = NewStatus;
                invo.UpdateDate = AppContext.Current.User.Now;
            }
        }

        public void insertInvoiceLog(InvoiceLog invoLogInstance)
        {
            List<InvoiceLog> listInvLog = new List<InvoiceLog>();
            int[] invoiceIdArr = invoLogInstance.invoiceIds;

            if (invoiceIdArr.Count() > 0)
            {
                List<InvoiceAging> invos = (from invo in CommonRep.GetQueryable<InvoiceAging>()
                                            where invoiceIdArr.Contains(invo.Id)
                                            select invo).ToList<InvoiceAging>();

                foreach (var invo in invos)
                {
                    InvoiceLog involog = new InvoiceLog();
                    involog.Deal = AppContext.Current.User.Deal;
                    involog.CustomerNum = invo.CustomerNum;
                    involog.InvoiceId = invo.InvoiceNum;
                    involog.LogDate = AppContext.Current.User.Now;
                    involog.LogPerson = AppContext.Current.User.EID;
                    involog.LogAction = invoLogInstance.LogAction;
                    involog.LogType = invoLogInstance.LogType;
                    involog.OldStatus = invo.States;
                    involog.NewStatus = invoLogInstance.NewStatus; 
                    involog.OldTrack = invo.TrackStates;
                    involog.NewTrack = invoLogInstance.NewTrack;
                    involog.ContactPerson = invoLogInstance.ContactPerson;
                    involog.ProofId = invoLogInstance.ProofId;
                    involog.Discription = invoLogInstance.Discription;
                    listInvLog.Add(involog);
                    
                }
                CommonRep.AddRange<InvoiceLog>(listInvLog);
            }
        }

        public MailTmp GetNewMailInstance(string customerNums)
        {
            MailTmp res = new MailTmp();
            MailTemplate tpl = new MailTemplate();

            string strTemplateType = Helper.EnumToCode<MailTemplateType>(MailTemplateType.BreakPTP);

            IMailService ms = SpringFactory.GetObjectImpl<IMailService>("MailService");
            tpl = ms.GetMailTemplate(strTemplateType);
            res.From = AppContext.Current.User.Email;

            if (tpl != null)
            {
                res = ms.GetInstanceFromTemplate(tpl, (parser) =>
                {
                    parser.RegistContext("collector", AppContext.Current.User);
                });

                res.From = AppContext.Current.User.Email;
            }
            else
            {
                Exception ex = new OTCServiceException("No matching template was found!", System.Net.HttpStatusCode.NotFound);
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }

            return res;
        }

        public void SendBreakPTPLetter( MailTmp mail)
        {
           List<CustomerKey> cus = new List<CustomerKey>();
           cus=mail.GetRelatedCustomers();
            if(cus.Count >0)
            {
                string customer = string.Join(",", cus.Select(x=>x.CustomerNum).ToArray());
                string siteUseId = string.Join(",", cus.Select(x=>x.SiteUseId).ToArray());
                // 1. write contact history
                ContactService cs = SpringFactory.GetObjectImpl<ContactService>("ContactService");
                cs.AddMailContactHistory(customer, siteUseId, mail.To, mail.MessageId);
            }
        
            // 2. send mail
            MailService mailService = SpringFactory.GetObjectImpl<MailService>("MailService");
            mailService.SendMail(mail);
        }
    }
}
