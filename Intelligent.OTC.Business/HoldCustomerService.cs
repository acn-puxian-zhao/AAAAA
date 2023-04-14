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
using Intelligent.OTC.Domain.Dtos;

namespace Intelligent.OTC.Business
{
    public class HoldCustomerService
    {
        public OTCRepository CommonRep { get; set; }
        public IEnumerable<HoldCustomerView> GetHoldCustomer()
        {
            PeroidService service = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");
            string CurrentDeal = AppContext.Current.User.Deal.ToString();
            string CurrentUser = AppContext.Current.User.EID.ToString();
            DateTime CurrentTime = AppContext.Current.User.Now;
            PeriodControl period = new PeriodControl();
            period=service.getcurrentPeroid();
            int CurrentPeriod=0;
            if (period != null) { CurrentPeriod = period.Id; }

            string invoOpen=Helper .EnumToCode<InvoiceStatus>(InvoiceStatus.Open);
            string invoBrPTP = Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Broken_PTP);
            string invoFinRem=Helper.EnumToCode<TrackStatus>(TrackStatus.Wait_for_Payment_Reminding);
            string invo2ndBroSent=Helper.EnumToCode<TrackStatus>(TrackStatus.Wait_for_2nd_Time_Dispute_respond);

            var customer = CommonRep.GetDbSet<HoldCustomerView>().Where(
                    cs => cs.ActionDate <= CurrentTime && cs.Deal == CurrentDeal && cs.Operator == CurrentUser
                   
                );

            //  2016-03-23 pxc update    states => trackstates  ################ s 
            var invoiceOpen =from invo in CommonRep.GetQueryable<InvoiceAging>()
                             where invo.Deal == CurrentDeal && invo.TrackStates == invoFinRem 
                             select invo;
            //  2016-03-23 pxc update    states => trackstates  ################ e 

            var alert= from al in CommonRep.GetQueryable<CollectorAlert>() 
                             where al.Deal == CurrentDeal && al.PeriodId == CurrentPeriod
                             && al.AlertType == 3 && al.Status == "Finish"
                             select al;

            var invoAlert = (from inv in invoiceOpen
                             join al in alert on new { inv.CustomerNum, inv.LegalEntity } equals new { al.CustomerNum, al.LegalEntity }
                             select new { inv.CustomerNum, inv.LegalEntity }).Union
                             (from invo in CommonRep.GetDbSet<InvoiceAging>()
                              where invo.Deal == CurrentDeal &&
                              (invo.States == invoBrPTP && invo.TrackStates == invo2ndBroSent)
                              select new { invo.CustomerNum,invo.LegalEntity});
            

            var res = (from cus in customer
                       join invoP in invoAlert on new { cus.CustomerNum, cus.LegalEntity } equals new { invoP.CustomerNum, invoP.LegalEntity }
                       select cus).Distinct();

            return res.AsEnumerable();

        }
        public IEnumerable<UnHoldCustomer> GetUnHoldCustomer()
        {
            string CurrentDeal = AppContext.Current.User.Deal.ToString();
            string CurrentUser = AppContext.Current.User.EID.ToString();
            DateTime CurrentTime = AppContext.Current.User.Now;

            //get the holded customerAging Info
            var res = from cc in CommonRep.GetQueryable<UnHoldCustomer>()
                      where cc.Deal == CurrentDeal && cc.Operator == CurrentUser 
                       select cc;

            return res.AsEnumerable(); 
        }
        public void updateHoldFlg(string customerNum,string legalEntity,string holdFlg)
        {
            DateTime CurrentTime = AppContext.Current.User.Now;
            string CurrentUser = AppContext.Current.User.EID;
            var result = CommonRep.GetQueryable<HoldCustomer>().Where(h=>h.CustomerNum==customerNum &&
                                                                      h.LegalEntity==legalEntity);
            if (result.Count() > 0)
            {
                foreach (HoldCustomer hc in result)
                {
                    hc.IsHoldFlg = holdFlg;
                    hc.LastUpdateTime = CurrentTime;
                    hc.LastUpdateUser = CurrentUser;
                }
            }
            else
            {
                HoldCustomer addHC = new HoldCustomer();
                addHC.Deal = AppContext.Current.User.Deal.ToString();
                addHC.CustomerNum = customerNum;
                addHC.LegalEntity = legalEntity;
                addHC.IsHoldFlg = holdFlg;
                addHC.CreateTime = AppContext.Current.User.Now;
                addHC.CreateUser = AppContext.Current.User.EID;
                addHC.LastUpdateTime = AppContext.Current.User.Now;
                addHC.LastUpdateUser = AppContext.Current.User.EID;
                CommonRep.Add<HoldCustomer>(addHC);
            }
        }

        //Hold Customer
        public void confirmHoldCustomer(InvoiceLog invoiceLogInstance)
        {
            //no invoice selected ,only insert into T_hold_customer
            if (invoiceLogInstance.invoiceIds.Count()==0)
            {
                string customerNum = invoiceLogInstance.CustomerNum;
                string legalEntity = invoiceLogInstance.legalEntity;
                string strIsHold = "1";
                int alertType = 4;

                //update T_Hold_Customer
                updateHoldFlg(customerNum, legalEntity, strIsHold);
                //update T_Hold_Alert
                UpdateAlert(customerNum, legalEntity, "Finish", alertType);
                CommonRep.Commit();
            }
            else
            {
            string invoHold = Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Hold);
            var invos = (from invo in CommonRep.GetQueryable<InvoiceAging>()
                         where invo.States != invoHold && invoiceLogInstance.invoiceIds.Contains(invo.Id)
                         select invo.Id);
            //aviod 2nd brokensent=>breakPTP
            invoiceLogInstance.invoiceIds = invos.ToArray();
            if (invoiceLogInstance.invoiceIds.Count() > 0)
            { 
                //insert T_invoiceLog
                insertInvoiceLog(invoiceLogInstance);
                //update T_invoice
                changeInvoiceStates(invoiceLogInstance);

                //3.update the T_alert ActionDate
                DunningReminderService ds = SpringFactory.GetObjectImpl<DunningReminderService>("DunningReminderService");
                int [] arrInvoIds = invoiceLogInstance.invoiceIds;
                if (arrInvoIds != null && arrInvoIds.Count() > 0)
                {
                    var customer = CommonRep.FindBy<InvoiceAging>(arrInvoIds[0]);
                    string customerNum = customer.CustomerNum;
                    string legalEntity = customer.LegalEntity;
                    string strIsHold = "1";
                    int alertType = 4;

                    //update T_Hold_Customer
                    updateHoldFlg(customerNum, legalEntity, strIsHold);
                    //update T_Hold_Alert
                    UpdateAlert(customerNum, legalEntity, "Finish", alertType);
                }
                CommonRep.Commit();
            }//invoiceIds.Count
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

        //change status
        public void changeStatus(int[] ids, string NewStatus,string mailId)
        {
            //1.insert invoiceLog
            InvoiceLog invoLogInstance = new InvoiceLog();
            invoLogInstance.invoiceIds = ids;
            invoLogInstance.LogAction = "HOLD";
            invoLogInstance.LogType = "7";
            invoLogInstance.NewStatus = Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Hold);
            invoLogInstance.NewTrack = NewStatus;
            invoLogInstance.ContactPerson = "";
            invoLogInstance.ProofId = mailId;
            invoLogInstance.Discription = "";
            //invoiceLog
            insertInvoiceLog(invoLogInstance);

            //2.update invoiceAging states
            changeInvoiceStatesByIds(ids, NewStatus);

            //3.update the T_alert ActionDate,update the T_hold_customer
            DunningReminderService ds = SpringFactory.GetObjectImpl<DunningReminderService>("DunningReminderService");
            List<string> customerLi = new List<string>();
            if (ids != null && ids.Count() > 0)
            {
                var customer = CommonRep.FindBy<InvoiceAging>(ids[0]);
                string customerNum = customer.CustomerNum;
                string legalEntity = customer.LegalEntity;
                string strIsHold = "1";
                int alertType = 4;

                //update T_Hold_Customer
                updateHoldFlg(customerNum, legalEntity, strIsHold);
                //update T_alert
                UpdateAlert(customerNum, legalEntity, "Finish", alertType);
            }
            CommonRep.Commit();
        }

        #region update alert
        public void UpdateAlert(string cusnums, string legalEntity,string status, int alertType)
        {
            string CurrentDeal = AppContext.Current.User.Deal;
            string CurrentUser = AppContext.Current.User.EID;
            DateTime CurrentNow=AppContext.Current.User.Now ;
            List<CollectorAlert> alertList = CommonRep.GetQueryable<CollectorAlert>()
                                            .Where(o => o.Deal == CurrentDeal && 
                                                o.CustomerNum == cusnums &&
                                                o.LegalEntity == legalEntity &&
                                                o.AlertType == alertType && 
                                                o.Eid == CurrentUser && 
                                                o.Status != "Cancelled" &&
                                                o.Status != "Finish").ToList();

            if (alertList != null)
            {
                foreach (var alert in alertList)
                {
                    alert.Status = status;
                    alert.ActionDate = AppContext.Current.User.Now.Date;
                }     
            }

        }

        #endregion
        public void changeInvoiceStatesByIds(int[] ids, string NewStatus)
        {
            List<InvoiceAging> invos = (from invo in CommonRep.GetQueryable<InvoiceAging>()
                                        where ids.Contains(invo.Id)
                                        select invo).ToList<InvoiceAging>();
            foreach (var invo in invos)
            {
                invo.States=Helper .EnumToCode <InvoiceStatus>(InvoiceStatus.Hold);
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

            string strTemplateType = Helper.EnumToCode<MailTemplateType>(MailTemplateType.HoldCustomer);

            IMailService ms = SpringFactory.GetObjectImpl<IMailService>("MailService");
            string nMailAdderss = ms.GetSenderMailAddress();

            tpl = ms.GetMailTemplate(strTemplateType);
            res.From = nMailAdderss;

            if (tpl != null)
            {
                res = ms.GetInstanceFromTemplate(tpl, (parser) =>
                {
                    parser.RegistContext("collector", AppContext.Current.User);
                });

                res.From = nMailAdderss;
            }
            else
            {
                throw new OTCServiceException("No matching template was found!", System.Net.HttpStatusCode.NotFound);
            }

            return res;

        }

        public void SendHoldMail( MailTmp mail)
        {
            List<CustomerKey> cus = new List<CustomerKey>();
            cus = mail.GetRelatedCustomers();
            if (cus.Count>0)
            {
                string customer = string.Join(",", cus.Select(x=>x.CustomerNum).ToArray());
                string siteuseid = string.Join(",", cus.Select(x=>x.SiteUseId).ToArray());
                // 1. write contact history
                ContactService cs = SpringFactory.GetObjectImpl<ContactService>("ContactService");
                cs.AddMailContactHistory(customer, siteuseid, mail.To, mail.MessageId);
            }

            // 2. send mail
            MailService mailService = SpringFactory.GetObjectImpl<MailService>("MailService");
            mailService.SendMail(mail);
        }

        public void cancelHoldCustomer(string customerNum,string legalEntity)
        {
            string strCancel = "Cancelled";
            List<CollectorAlert> listAlert = new List<CollectorAlert>();
            listAlert = CommonRep.GetQueryable<CollectorAlert>().
                Where(a => 
                    a.CustomerNum == customerNum &&
                    a.LegalEntity == legalEntity &&
                    a.AlertType == 4 &&
                    a.Status.ToUpper() != "Finish".ToUpper() &&
                    a.Status.ToUpper() != "Cancelled".ToUpper()).ToList<CollectorAlert>();

            if (listAlert != null)
            {
                foreach (CollectorAlert alert in listAlert)
                {
                    alert.Status = strCancel;
                }
            }
            CommonRep.Commit();
        }//cancelHoldCustomer end

        public void unHoldCustomer(string customerNum, string legalEntity, string reMailId)
        {
            List<InvoiceLog> invoLogList = new List<InvoiceLog>();
            string strIsHold = "0";
            //update T_Hold_Customer
            updateHoldFlg(customerNum, legalEntity, strIsHold);

            //update invoice status
            string statesHold=Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Hold);
            string trackHold=Helper.EnumToCode<TrackStatus>(TrackStatus.Open);
            var invos = CommonRep.GetQueryable<InvoiceAging>().Where(inv => inv.CustomerNum == customerNum &&
                                                                    inv.LegalEntity==legalEntity &&
                                                                    inv.States == statesHold );

            if (invos.Count()>0)
            {
                foreach (InvoiceAging invo in invos)
                {
                    

                    //insert invoiceLog
                    InvoiceLog invoLogInstance = new InvoiceLog();

                    invoLogInstance.Deal = AppContext.Current.User.Deal;
                    invoLogInstance.CustomerNum = invo.CustomerNum;
                    invoLogInstance.InvoiceId = invo.InvoiceNum;
                    invoLogInstance.LogDate = AppContext.Current.User.Now;
                    invoLogInstance.LogPerson = AppContext.Current.User.EID;
                    invoLogInstance.LogAction = "UNHOLD";
                    invoLogInstance.LogType = "9";
                    invoLogInstance.OldStatus = invo.States;
                    invoLogInstance.NewStatus = Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Open);
                    invoLogInstance.OldTrack = invo.TrackStates;
                    invoLogInstance.NewTrack = Helper.EnumToCode<TrackStatus>(TrackStatus.Open);
                    invoLogInstance.ContactPerson = "";
                    invoLogInstance.ProofId = reMailId;
                    invoLogInstance.Discription = "";

                    //update invoice states to open
                    invo.States = Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Open);
                    invo.TrackStates = Helper.EnumToCode<TrackStatus>(TrackStatus.Open);
                    //invoiceLog
                    invoLogList.Add(invoLogInstance);
                }
            }
            else
            { 
                //do nothing
            }

            CommonRep.AddRange<InvoiceLog>(invoLogList);
            CommonRep.Commit();
        }//unHoldCustomer end
    }
}
