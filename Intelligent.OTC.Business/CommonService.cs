using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Intelligent.OTC.Business
{
    public class CommonService
    {
        public CommonService()
        { 
        }

        public OTCRepository CommonRep { private get; set; }
        public XcceleratorService XccService { get; set; }

        public List<MyClass> WorkFlowPendingNum()
        {
            List<MyClass> lstPendingNum = new List<MyClass>();
            try
            {
                //Get SOA Count
                lstPendingNum.Add(this.SoaCount());

                //Get Dispute Tracking Count
                lstPendingNum.Add(this.DisputeTrackingCount());

                //Get Mail View Count
                MyClass mailView = this.MailViewCount();
                lstPendingNum.Add(mailView);

                //Get BreakPTP View Count
                lstPendingNum.Add(this.BreakPTP());

                //Get holdCustomer
                lstPendingNum.Add(this.HoldCustomer());

                //Get Dunning Reminder
                lstPendingNum.Add(this.DunningReminder());

                return lstPendingNum;
            }
            catch (Exception ex) 
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        public class MyClass
        {
            public int Key { get; set; }
            public int Value { get; set; }
        }

        private PeriodControl _per;
        //Get Current Period
        public PeriodControl per 
        {
            get
            {
                if (_per == null)
                {
                    PeroidService perService = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");
                    _per = perService.getcurrentPeroid();
                    return _per;
                }
                else
                {
                    return _per;
                }
            }
        }

        private MyClass MailViewCount()
        {
            IMailService ms = SpringFactory.GetObjectImpl<IMailService>("MailService");

            MailCountDto mailCount = ms.QueryMailCount();

            MyClass mailView = new MyClass();
            mailView.Key = 7;
            mailView.Value = Convert.ToInt32((mailCount.CustomerNew == null ? 0 : mailCount.CustomerNew) + (mailCount.Unknow == null ? 0 : mailCount.Unknow));

            return mailView;
        }

        /// <summary>
        /// Get SOA Count
        /// </summary>
        /// <returns></returns>
        private MyClass SoaCount() 
        {
            ISoaService service = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
            int SoaCount = service.GetSoaList().Count();

            MyClass Soa = new MyClass();
            Soa.Key = 1;
            Soa.Value = SoaCount;
            return Soa;
        }

        /// <summary>
        /// Get Dunning Reminder Count
        /// </summary>
        /// <returns></returns>
        private MyClass DunningReminder() 
        {
            List<MyClass> lstDunningCount = new List<MyClass>();
            //Get Dunning Reminder Count
            IDunningService service = SpringFactory.GetObjectImpl<IDunningService>("DunningReminderService");
            int DunningCount = service.GetDunningList().Where(o => (o.LastRemind != null && o.TaskId == "") || (o.TaskId != "")).Count();

            MyClass Dunning = new MyClass();
            Dunning.Key = 3;
            Dunning.Value = DunningCount;

            return Dunning;
        }

        /// <summary>
        /// Get Contact Customer Count
        /// </summary>
        /// <returns></returns>
        private MyClass ContactCustomerCount()
        {
            Exception ex = new NotImplementedException();
            Helper.Log.Error(ex.Message, ex);
            throw ex;
        }

        private MyClass BreakPTP() {
            string deal = AppContext.Current.User.Deal.ToString();
            string eid = AppContext.Current.User.EID.ToString();
            DateTime dt = AppContext.Current.User.Now;
            string invoOpen = Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Open);
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

            MyClass breakPTP = new MyClass();
            breakPTP.Key = 5;
            breakPTP.Value = result.Count();

            return breakPTP;
        }

        private MyClass HoldCustomer() 
        {
            string CurrentDeal = AppContext.Current.User.Deal.ToString();
            string CurrentUser = AppContext.Current.User.EID.ToString();
            DateTime CurrentTime = AppContext.Current.User.Now;

            string invoOpen = Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Open);
            string invoBrPTP = Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Broken_PTP);
            string invoFinRem = Helper.EnumToCode<TrackStatus>(TrackStatus.Wait_for_Payment_Reminding);
            string invo2ndBroSent = Helper.EnumToCode<TrackStatus>(TrackStatus.Wait_for_2nd_Time_Dispute_respond);

            PeroidService service = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");
            PeriodControl period = new PeriodControl();
            period = service.getcurrentPeroid();
            int CurrentPeriod = 0;
            if (period != null) { CurrentPeriod = period.Id; }

            var customer = CommonRep.GetDbSet<HoldCustomerView>().Where(
                    cs => cs.ActionDate <= CurrentTime && cs.Deal == CurrentDeal && cs.Operator == CurrentUser

                );

            //  2016-03-23 pxc update    states => trackstates  ################ s 
            var invoiceOpen = from invo in CommonRep.GetQueryable<InvoiceAging>()
                              where invo.Deal == CurrentDeal && invo.TrackStates == invoFinRem
                              select invo;
            //  2016-03-23 pxc update    states => trackstates  ################ e 

            var alert = from al in CommonRep.GetQueryable<CollectorAlert>()
                        where al.Deal == CurrentDeal && al.PeriodId == CurrentPeriod
                        && al.AlertType == 3 && al.Status == "Finish"
                        select al;

            var invoAlert = (from inv in invoiceOpen
                             join al in alert on new { inv.CustomerNum, inv.LegalEntity } equals new { al.CustomerNum, al.LegalEntity }
                             select new { inv.CustomerNum, inv.LegalEntity }).Union
                             (from invo in CommonRep.GetDbSet<InvoiceAging>()
                              where invo.Deal == CurrentDeal &&
                              (invo.States == invoBrPTP && invo.TrackStates == invo2ndBroSent)
                              select new { invo.CustomerNum, invo.LegalEntity });


            var res = (from cus in customer
                       join invoP in invoAlert on new { cus.CustomerNum, cus.LegalEntity } equals new { invoP.CustomerNum, invoP.LegalEntity }
                       select cus).Distinct();


            MyClass holdCustomer= new MyClass();
            holdCustomer.Key = 6;
            holdCustomer.Value = res.Count();

            return holdCustomer;
        
        }

        /// <summary>
        /// Get Dispute Tracking Count
        /// </summary>
        /// <returns></returns>
        private MyClass DisputeTrackingCount()
        {
            string deal = AppContext.Current.User.Deal.ToString();

            List<SysUser> listUser = new List<SysUser>();
            listUser = XccService.GetUserTeamList(AppContext.Current.User.EID);
            string[] userGroup = listUser.Select(t => t.EID).Distinct().ToArray();
            string collecotrList = "," + string.Join(",", userGroup.ToArray()) + ",";

            int disTrackCount = 0;
            if (AppContext.Current.User.ActionPermissions.IndexOf("alldataforsupervisor") >= 0)
            {
                disTrackCount = CommonRep.GetQueryable<DisputeTrackingView>()
                    .Where(m => m.Deal == deal && m.DisputeType == "0").Count();
            }
            else
            {
                disTrackCount = CommonRep.GetQueryable<DisputeTrackingView>()
                   .Where(m => m.Deal == deal && m.DisputeType == "0" && collecotrList.Contains("," + m.Collector + ",")).Count();
            }

            MyClass disputeTrack = new MyClass();
            disputeTrack.Key = 4;
            disputeTrack.Value = disTrackCount;

            return disputeTrack;
        }

        /// <summary>
        /// Get Call List
        /// </summary>
        /// <param name="customerNum"></param>
        /// <returns></returns>
        public IEnumerable<ContactHistory> GetCallList(string customerNum) {
            try
            {
                return CommonRep.GetQueryable<ContactHistory>()
                        .Where(o => o.CustomerNum == customerNum 
                            && o.ContactType.ToUpper() == "Call".ToUpper())
                            .OrderByDescending(o => o.Id).ToList().AsQueryable();
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }
    }
}
