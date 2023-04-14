using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common.UnitOfWork;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Repositories;
using System.Configuration;
using System.IO;
using Intelligent.OTC.Domain;
using Intelligent.OTC.Common;
using System.Data.Entity.Validation;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Common.Exceptions;
using System.Data.Entity;
using System.Web;
using System.Text.RegularExpressions;
using Intelligent.OTC.Common.Repository;
using System.Transactions;
using System.Data.SqlClient;
using System.Data;
using EntityFramework.BulkInsert.Extensions;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Metadata.Edm;
using System.Collections.ObjectModel;
using EntityFramework.MappingAPI.Mappings;
using System.Reflection;
using EntityFramework.MappingAPI.Exceptions;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;

namespace Intelligent.OTC.Business
{
    public class ContactindexService
    {
        public XcceleratorService XccService { get; set; }

        #region Parameters
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

        public int SupervisorPermission
        {
            get 
            {
                if (AppContext.Current.User.ActionPermissions.IndexOf("alldataforsupervisor") >= 0)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }

            }
        }

        public OTCRepository CommonRep { get; set; }

        #endregion
        public IQueryable<AllAccountInfo> getAllInvoiceByUser(string invoiceState = "", string invoiceTrackState = "", string invoiceNum = "", string soNum = "", string poNum = "", string invoiceMemo = "")
        {
            Helper.Log.Info("All Account Start!");
            IQueryable<CollectorAlert> alerts;
            IQueryable<SpecialNote> notes;
            IQueryable<CustomerAging> agings;
            IQueryable<InvoiceAging> invagings;
            DateTime dt;
            dt = CurrentTime.Date;
            DateTime dtOver90;
            dtOver90 = CurrentTime.Date.AddDays(-90);
            string strPtp = Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.PTP);
            string strBrokenPTP = Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Broken_PTP);
            string strDisp = Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Dispute);
            string strOpen = Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Open);
            string strPartialPay = Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.PartialPay);
            string strHold = Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Hold);
            string strPayment = Helper.EnumToCode<InvoiceStatus>(InvoiceStatus.Payment);
            DateTime dtperEndOver90 = new DateTime(); 
            try
            {
                //取得当前peroid
                PeroidService perService = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");
                PeriodControl per = perService.getcurrentPeroid();
                
                if (per != null)
                {
                    dtperEndOver90 = per.PeriodEnd.AddDays(-90);
                    if (per.SoaFlg != "1" || per.IsCurrentFlg != "1")
                    {
                        Helper.Log.Info("SOA is not Started");
                        return null;
                    }
                }
                else
                {
                    Helper.Log.Info("There is no Data");
                    return null;
                }

                //1 .find all customer by collector
                CustomerService custService = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");

                var customerLevels = from level in custService.GetCustomerLevelForAllCus(per.Id, SupervisorPermission)
                                     join clt in CommonRep.GetDbSet<CollectorTeam>().Where(o => o.Deal == CurrentDeal)
                                     on level.Collector equals clt.Collector
                                     select new 
                                     { 
                                     Id = level.Id,
                                     Deal = level.Deal,
                                     CustomerNum = level.CustomerNum,
                                     CustomerName = level.CustomerName,
                                     BillGroupCode = level.BillGroupCode,
                                     BillGroupName = level.BillGroupName,
                                     Collector = level.Collector,
                                     Value = level.Value,
                                     Risk = level.Risk,
                                     ValueLevel =level.ValueLevel,
                                     RiskLevel =level.RiskLevel,
                                     TeamName = clt.Team.TeamName,
                                     CS=level.CS
                                     };

                agings = CommonRep.GetDbSet<CustomerAging>().Where(o => o.Deal == CurrentDeal);

                invagings = CommonRep.GetDbSet<InvoiceAging>().Where(o => o.Deal == CurrentDeal &&
                                                            (o.States == strOpen || o.States == strPtp || o.States == strBrokenPTP ||
                                                            o.States == strDisp || o.States == strPartialPay || o.States == strHold || o.States == strPayment));
                                               
                var rates = from rate in CommonRep.GetDbSet<RateTran>()
                            where rate.Deal == CurrentDeal && rate.EffectiveDate <= dt && rate.ExpiredDate >= dt
                            select new { rate.Rate, rate.ForeignCurrency };

                notes = CommonRep.GetDbSet<SpecialNote>().Where(o => o.Deal == CurrentDeal);

                alerts = CommonRep.GetDbSet<CollectorAlert>().Where(o => o.Deal == CurrentDeal
                                                                    && o.PeriodId == per.Id
                                                                    && o.Status == "Finish");

                var alert1s = from alert1 in alerts.Where(o => o.AlertType == 1)
                              group alert1 by alert1.SiteUseId into g
                              select new { SiteUseId = g.Key, Status1 = g.Max(s => s.ActionDate) };

                var alert23s = from alert2 in alerts.Where(o => o.AlertType == 2)
                               join alert3 in alerts.Where(o => o.AlertType == 3)
                                on new { alert2.LegalEntity, alert2.SiteUseId }
                                   equals new { alert3.LegalEntity, alert3.SiteUseId }
                                into alertstemp
                               from alerttemp in alertstemp.DefaultIfEmpty()
                                select new
                                {
                                    LegalEntity = alert2.LegalEntity,
                                    SiteUseId = alert2.SiteUseId,
                                    Status2 = alert2.ActionDate,
                                    Status3 = alerttemp.ActionDate
                                };
            var conalls = CommonRep.GetQueryable<Contactor>().Where(o => o.Deal == AppContext.Current.User.Deal);

            IQueryable<AllAccountInfo> invsList = from invGrp in
                                                        (from invaging in
                                                            (from inv in invagings
                                                            join level in customerLevels on inv.CustomerNum equals level.CustomerNum
                                                            join rate in rates on inv.Currency equals rate.ForeignCurrency
                                                                into rateDefts
                                                            from rateDeft in rateDefts.DefaultIfEmpty()
                                                            where invagings.Any(
                                                            i =>
                                                                (i.States == invoiceState || string.IsNullOrEmpty(invoiceState))
                                                                && (i.TrackStates == invoiceTrackState || string.IsNullOrEmpty(invoiceTrackState))
                                                                && (i.InvoiceNum.IndexOf(invoiceNum) >= 0 || string.IsNullOrEmpty(invoiceNum))
                                                                && (i.SoNum.IndexOf(soNum) >= 0 || string.IsNullOrEmpty(soNum))
                                                                && (i.PoNum.IndexOf(poNum) >= 0 || string.IsNullOrEmpty(poNum))
                                                                && (i.Comments.IndexOf(invoiceMemo) >= 0 || string.IsNullOrEmpty(invoiceMemo))
                                                            )
                                                            select new
                                                            {
                                                                LegalEntity = inv.LegalEntity,
                                                                CustomerNum = inv.CustomerNum,
                                                                CustomerName = level.CustomerName,
                                                                BillGroupCode = level.BillGroupCode,
                                                                BillGroupName = level.BillGroupName,
                                                                Rate = rateDeft != null ? rateDeft.Rate : 1,
                                                                BalanceAmt = inv.BalanceAmt,
                                                                States = inv.States,
                                                                CreateDate = inv.CreateDate,
                                                                Class = (string.IsNullOrEmpty(level.ValueLevel) == true ? "LV" : level.ValueLevel) +
                                                                        (string.IsNullOrEmpty(level.RiskLevel) == true ? "LR" : level.RiskLevel),
                                                                DueDate = inv.DueDate,
                                                                OverDue90Amt = (inv.DueDate <= dtOver90) ? inv.BalanceAmt : 0,
                                                                AdjustedOver90 = (inv.DueDate <= dtperEndOver90 && inv.BalanceAmt > 0) ? inv.BalanceAmt : 0,
                                                                PtpAmt = (inv.States == strPtp) ? inv.BalanceAmt : 0,
                                                                BrokenPTPAmt = inv.States == strBrokenPTP ? inv.BalanceAmt : 0,
                                                                DisputeAmt = inv.States == strDisp ? inv.BalanceAmt : 0
                                                                ,Collector = level.Collector
                                                                ,Team = level.TeamName
                                                                ,SiteUseId=inv.SiteUseId,
                                                                COLLECTOR_CONTACT=inv.CollectorContact,
                                                                Sales=inv.Sales,
                                                                CS=level.CS,
                                                                CREDIT_LIMIT=inv.CreditLmt,
                                                                CreditTremDescription=inv.CreditTremDescription
                                                                //End add by xuan.wu for Arrow adding
                                                            })
                                                        group invaging by new
                                                        {
                                                            invaging.LegalEntity,
                                                            invaging.CustomerNum,
                                                            invaging.BillGroupCode,
                                                            invaging.BillGroupName,
                                                            invaging.CustomerName,
                                                            invaging.Class
                                                            ,invaging.Collector
                                                            ,invaging.Team
                                                            //Start add by xuan.wu for Arrow adding,
                                                            ,invaging.SiteUseId,
                                                            invaging.COLLECTOR_CONTACT,
                                                            invaging.Sales,
                                                            invaging.CS,
                                                            invaging.CREDIT_LIMIT,
                                                            invaging.CreditTremDescription
                                                            //End add by xuan.wu for Arrow adding
                                                        }
                                                            into invgrp
                                                            select new
                                                            {
                                                                LegalEntity = invgrp.Key.LegalEntity,
                                                                CustomerNum = invgrp.Key.CustomerNum,
                                                                CustomerName = invgrp.Key.CustomerName,
                                                                BillGroupCode = invgrp.Key.BillGroupCode,
                                                                BillGroupName = invgrp.Key.BillGroupName,
                                                                Class = invgrp.Key.Class,
                                                                OverDue90Amt = invgrp.Sum(o => o.OverDue90Amt * o.Rate),
                                                                PtpAmt = invgrp.Sum(o => o.PtpAmt * o.Rate),
                                                                BrokenPTPAmt = invgrp.Sum(o => o.BrokenPTPAmt * o.Rate),
                                                                DisputeAmt = invgrp.Sum(o => o.DisputeAmt * o.Rate),
                                                                AdjustedOver90 = invgrp.Sum(o =>o.AdjustedOver90 * o.Rate),
                                                                UnapplidPayment = 0
                                                                ,Collector = invgrp.Key.Collector
                                                                ,Team = invgrp.Key.Team
                                                                //Start add by xuan.wu for Arrow adding
                                                                ,SiteUseId=invgrp.Key.SiteUseId,
                                                                COLLECTOR_CONTACT= invgrp.Key.COLLECTOR_CONTACT,
                                                                Sales=invgrp.Key.Sales,
                                                                CS= invgrp.Key.CS,
                                                                CREDIT_LIMIT=invgrp.Key.CREDIT_LIMIT,
                                                                CreditTremDescription=invgrp.Key.CreditTremDescription                                                               
                                                               //End add by xuan.wu for Arrow adding
                                                            })
                                                    join aging in agings on new { invGrp.CustomerNum, invGrp.LegalEntity } equals new { aging.CustomerNum, aging.LegalEntity }
                                                    join note in notes on new { invGrp.CustomerNum, invGrp.LegalEntity } equals new { note.CustomerNum, note.LegalEntity }
                                                    into notestmp
                                                    from notetmp in notestmp.DefaultIfEmpty()
                                                    join alert1 in alert1s on invGrp.SiteUseId equals alert1.SiteUseId
                                                    into alert1sTmp
                                                    from alert1tmp in alert1sTmp.DefaultIfEmpty()
                                                    join alert23 in alert23s on new { invGrp.SiteUseId, invGrp.LegalEntity } equals new { alert23.SiteUseId, alert23.LegalEntity }
                                                    into alert23sTmp
                                                    from alert23tmp in alert23sTmp.DefaultIfEmpty()
                                                    join cont in
                                                        (from cust in customerLevels
                                                        from con in conalls
                                                        where cust.CustomerNum == con.CustomerNum || cust.BillGroupCode == con.GroupCode
                                                        select new
                                                        {
                                                            Id = con.Id,
                                                            CustomerNum = cust.CustomerNum,
                                                            EmailAddress = con.EmailAddress,
                                                            LegalEntity = con.LegalEntity,
                                                            Name = con.Name
                                                        })
                                                      on new { invGrp.CustomerNum, invGrp.LegalEntity } equals new { cont.CustomerNum, cont.LegalEntity }
                                                      into conts 
                                                    select new AllAccountInfo
                                                    {
                                                        Id = aging.Id,
                                                        LegalEntity = invGrp.LegalEntity,
                                                        CustomerNum = invGrp.CustomerNum,
                                                        CustomerName = invGrp.CustomerName,
                                                        BillGroupCode = invGrp.BillGroupCode,
                                                        BillGroupName = invGrp.BillGroupName,
                                                        Class = invGrp.Class,
                                                        ArBalanceAmtPeroid = aging.ArBalancePeriod,
                                                        BalanceAmt = aging.TotalAmt,
                                                        OverDue90Amt = invGrp.OverDue90Amt,
                                                        PtpAmt = invGrp.PtpAmt,
                                                        BrokenPTPAmt = invGrp.BrokenPTPAmt,
                                                        DisputeAmt = invGrp.DisputeAmt,
                                                        SpecialNotes = notetmp.SpecialNotes,
                                                        SoaDate = alert1tmp.Status1,
                                                        SecondDate = alert23tmp.Status2,
                                                        FinalDate = alert23tmp.Status3,
                                                        AdjustedOver90 = invGrp.AdjustedOver90,
                                                        PaymentTerm = aging.CreditTrem
                                                        ,Collector = invGrp.Collector
                                                        ,Team = invGrp.Team
                                                        ,Country = aging.CountryCode
                                                        ,ContactList = from c in conts
                                                                  select string.Concat(c.EmailAddress, "(", c.Name, ")"),
                                                        //Start add by xuan.wu for Arrow adding
                                                        SiteUseId = invGrp.SiteUseId,
                                                        COLLECTOR_CONTACT = invGrp.COLLECTOR_CONTACT,
                                                        Sales = invGrp.Sales,
                                                        CS = invGrp.CS,
                                                        CreditLimit = invGrp.CREDIT_LIMIT,
                                                        CreditTremDescription = invGrp.CreditTremDescription,
                                                        TotalFutureDue = aging.TotalFutureDue
                                                        //End add by xuan.wu for Arrow adding


                                                    };


                Helper.Log.Info("All Account End!");
                return invsList;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new Exception(ex.Message);
            }

        }

        public IQueryable<T_ALLACCOUNT_TMP> getAllInvoiceByUserForArrow(string isPTPOverDue)
        {
            Helper.Log.Info("All Account Start!");
            try
            {
                IQueryable<T_ALLACCOUNT_TMP> invsList = null;

                List<SysUser> listUser = new List<SysUser>();
                listUser = XccService.GetUserTeamList(AppContext.Current.User.EID);
                listUser.Add(new SysUser { });
                string[] userGroup = listUser.Select(t => t.EID).Distinct().ToArray();
                string collecotrList = ", " + string.Join(",", userGroup.ToArray()) + ",";
                if (SupervisorPermission == 1)
                {
                    invsList = from ac in CommonRep.GetDbSet<T_ALLACCOUNT_TMP>()
                               join site in CommonRep.GetDbSet<Customer>()
                               on new { STATUS = "1", SITEUSEID = ac.SiteUseId } equals new { STATUS = (site.IsActive == true ? "1" : "0"), SITEUSEID = site.SiteUseId }
                               select ac;

                    var ptpPaymentList = CommonRep.GetQueryable<V_PTPPayment>();

                    DateTime dtToday = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
                    if (isPTPOverDue.Equals("true"))
                    {
                        invsList = from invs in invsList
                                   where ptpPaymentList.Any(t => t.CustomerNum == invs.CustomerNum && t.SiteUseId == invs.SiteUseId
                                   && t.PromiseDate.Value < dtToday && t.PTPStatus == "001" && t.PTPPaymentType == "PTP")
                                   select invs;
                    }
                }
                else
                {
                    invsList = from ac in CommonRep.GetDbSet<T_ALLACCOUNT_TMP>()
                               join site in CommonRep.GetDbSet<Customer>()
                               on new { STATUS = "1", SITEUSEID = ac.SiteUseId } equals new { STATUS = (site.IsActive == true ? "1" : "0"), SITEUSEID = site.SiteUseId }
                               where collecotrList.Contains("," + ac.Collector + ",")
                               select ac;

                    var ptpPaymentList = CommonRep.GetQueryable<V_PTPPayment>();

                    DateTime dtToday = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
                    if (isPTPOverDue.Equals("true"))
                    {
                        invsList = from invs in invsList
                                   where ptpPaymentList.Any(t => t.CustomerNum == invs.CustomerNum && t.SiteUseId == invs.SiteUseId
                                   && t.PromiseDate.Value < dtToday && t.PTPStatus == "001" && t.PTPPaymentType == "PTP")
                                   select invs;
                    }

                }
                Helper.Log.Info("All Account End!");

                return invsList;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new Exception(ex.Message);
            }

        }

        public IEnumerable<SendSoaHead> CreateSendMail(string strCustomerSites)
        {
            string oper = AppContext.Current.User.Id.ToString();
            string deal = CurrentDeal;

            IBaseDataService bdSer = SpringFactory.GetObjectImpl<IBaseDataService>("BaseDataService");

            #region createsoalist
            string[] cusGroup = strCustomerSites.Split(',');

            string[] cusCust = new string[cusGroup.Count()];
            for (int i = 0; i < cusGroup.Count();i++)
            {
                cusCust[i] = cusGroup[i].Split(';')[0];
            }

            //cus
            var cusList = CommonRep.GetQueryable<Customer>()
                .Where(o => o.Deal == CurrentDeal && cusCust.Distinct().Contains(o.CustomerNum)).Include<Customer, CustomerGroupCfg>(c => c.CustomerGroupCfg).ToList();
            Customer cus = new Customer();
            //aging
            var cusAgingList = CommonRep.GetQueryable<CustomerAging>()
                .Where(o => o.Deal == deal && cusGroup.Contains(o.CustomerNum + ";" +o.LegalEntity)).ToList();
            //sendsoa
            List<SendSoaHead> sendsoaList = new List<SendSoaHead>();
            SendSoaHead sendsoa = new SendSoaHead();
            var classList = CommonRep.GetQueryable<CustomerLevelView>()
                .Where(o => o.Deal == deal && cusCust.Distinct().Contains(o.CustomerNum)).ToList();
            CustomerLevelView level = new CustomerLevelView();
            //SpecialNotes
            var SNList = CommonRep.GetQueryable<SpecialNote>().Where(o => o.Deal == deal 
                                                                && cusGroup.Contains(o.CustomerNum + ";" + o.LegalEntity)).ToList();
            //Rate
            var rateList = CommonRep.GetQueryable<RateTran>()
                .Where(o => o.Deal == CurrentDeal && o.EffectiveDate <= AppContext.Current.User.Now.Date && o.ExpiredDate >= AppContext.Current.User.Now.Date).ToList();
            //agingDT
            DateTime agingDT = new DateTime();
            PeroidService pservice = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");
            PeriodControl currentP = pservice.getcurrentPeroid();
            agingDT = dataConvertToDT(currentP.PeriodEnd.ToString());
            DateTime agingDT90 = new DateTime();
            agingDT90 = agingDT.AddDays(-90);
            //invoice
            var oldinvoiceList = CommonRep.GetQueryable<InvoiceAging>()
                .Where(o => o.Deal == deal && cusGroup.Contains(o.CustomerNum + ";" + o.LegalEntity)).ToList();
            List<InvoiceAging> newinvoiceList = new List<InvoiceAging>();
            newinvoiceList = oldinvoiceList;
            try
            {
                foreach (var item in newinvoiceList)
                {
                    if (item.Currency != "USD")
                    {
                        item.StandardBalanceAmt = (rateList.Find(m => m.ForeignCurrency == item.Currency).Rate == null ? 1 : rateList.Find(m => m.ForeignCurrency == item.Currency).Rate) * item.BalanceAmt;
                    }
                    else { item.StandardBalanceAmt = item.BalanceAmt; }
                }
            }
            catch (Exception)
            {

                throw;
            }

            IDunningService service = SpringFactory.GetObjectImpl<IDunningService>("DunningReminderService");
            List<CollectorAlert> reminders = service.GetEstimatedReminders(cusCust.Distinct().ToList(), null);

            List<Dispute> dispList= CommonRep.GetQueryable<Dispute>().Where(o => cusCust.Distinct().Contains(o.CustomerNum) && o.Deal == AppContext.Current.User.Deal).Select(o => o).ToList();

            List<ContactHistory> ContactList = new List<ContactHistory>();
            ContactList = CommonRep.GetDbSet<ContactHistory>().Where(o => o.Deal == deal && cusCust.Distinct().Contains(o.CustomerNum)).ToList();
            foreach (var item in cusCust.Distinct())
            {
                sendsoa = new SendSoaHead();
                cus = cusList.Find(m => m.Deal == deal && m.CustomerNum == item);
                level = classList.Find(m => m.Deal == deal && m.CustomerNum == item);
                var newCusAgingList = cusAgingList.FindAll(m => m.Deal == deal && m.CustomerNum == item);
                sendsoa.Deal = deal;
                sendsoa.CustomerCode = item;
                sendsoa.CustomerName = cus.CustomerName;
                sendsoa.TotalBalance = newCusAgingList.Sum(m => m.TotalAmt);//sites
                sendsoa.CustomerClass = (string.IsNullOrEmpty(level.ClassLevel) == true ? "LV" : level.ClassLevel)
                    + (string.IsNullOrEmpty(level.RiskLevel) == true ? "LR" : level.RiskLevel);

                //contactHistory
                List<SubContactHistory> ContactHisList = new List<SubContactHistory>();
                SubContactHistory ContactHis = new SubContactHistory();
                var OldConHisList = ContactList.Where(o => o.CustomerNum == item);
                int ihis = 1;
                foreach (var his in OldConHisList)
                {
                    ContactHis = new SubContactHistory();
                    ContactHis.SortId = ihis;
                    ContactHis.Deal = his.Deal;
                    ContactHis.CustomerNum = his.CustomerNum;
                    ContactHis.LegalEntity = his.LegalEntity;
                    ContactHis.ContactType = his.ContactType;
                    ContactHis.ContactDate = his.ContactDate;
                    ContactHis.ContactId = his.ContactId;
                    ContactHis.Comments = his.Comments;
                    ContactHisList.Add(ContactHis);
                    ihis++;
                }
                sendsoa.SubContactHistory = ContactHisList;

                var disputeList = dispList.Where(o => o.CustomerNum == item); ;
                List<Dispute> DisputeList = new List<Dispute>();
                Dispute dispute = new Dispute();
                int countId = 1;
                foreach (var disp in disputeList)
                {
                    dispute = new Dispute();
                    dispute.Id = disp.Id;
                    dispute.Deal = disp.Deal;
                    dispute.Eid = disp.Eid;
                    dispute.CloseDate = disp.CloseDate;
                    dispute.Comments = disp.Comments;
                    dispute.ContactId = disp.ContactId;
                    dispute.CreateDate = disp.CreateDate;
                    dispute.CreatePerson = disp.CreatePerson;
                    dispute.CustomerNum = disp.CustomerNum;
                    dispute.IssueReason = Helper.CodeToEnum<DisputeReason>(disp.IssueReason).ToString();
                    dispute.Status = Helper.CodeToEnum<DisputeStatus>(disp.Status).ToString();
                    dispute.sortId = countId;
                    DisputeList.Add(dispute);

                    countId++;
                }
                sendsoa.SubDisputeList = DisputeList;

                //Legal
                List<SoaLegal> sublegalList = new List<SoaLegal>();
                SoaLegal sublegal = new SoaLegal();
                foreach (var legal in newCusAgingList)
                {
                    var invoice = newinvoiceList
                        .FindAll(m => m.CustomerNum == item && m.LegalEntity == legal.LegalEntity);
                    var inv1 = invoice.FindAll(m => m.States == "004001" || m.States == "004002" || m.States == "004004" || m.States == "004008" || m.States == "004010" || m.States == "004011" || m.States == "004012").OrderBy(m => m.DueDate).ToList();
                    sublegal = new SoaLegal();
                    sublegal.LegalEntity = legal.LegalEntity;
                    sublegal.Country = legal.Country;
                    sublegal.CreditLimit = legal.CreditLimit;
                    sublegal.TotalARBalance = legal.TotalAmt;
                    sublegal.PastDueAmount = legal.DueoverTotalAmt;
                    sublegal.CreditBalance = invoice.FindAll(m => m.BalanceAmt < 0).Sum(m => m.StandardBalanceAmt);
                    sublegal.CurrentBalance = legal.CurrentAmt;
                    sublegal.FCollectableAmount = inv1
                        .FindAll(m => m.DueDate <= agingDT && (m.Class == "DM" || m.Class == "INV")).Sum(m => m.StandardBalanceAmt);
                    sublegal.FOverdue90Amount = inv1
                        .FindAll(m => m.DueDate <= agingDT90 && (m.Class == "DM" || m.Class == "INV")).Sum(m => m.StandardBalanceAmt);
                    var SN = SNList.Find(m => m.CustomerNum == item && m.LegalEntity == legal.LegalEntity);
                    if (SN == null)
                    {
                        sublegal.SpecialNotes = "";
                    }
                    else
                    {
                        sublegal.SpecialNotes = SN.SpecialNotes;
                    }
                    List<SoaInvoice> subinvoiceList = new List<SoaInvoice>();
                    SoaInvoice subinvoice = new SoaInvoice();
                    if (inv1.Count > 0)
                    {
                        foreach (var inv in inv1)
                        {
                            subinvoice = new SoaInvoice();
                            subinvoice.InvoiceId = inv.Id;
                            subinvoice.InvoiceNum = inv.InvoiceNum;
                            subinvoice.CustomerNum = inv.CustomerNum;
                            subinvoice.CustomerName = inv.CustomerName;
                            subinvoice.LegalEntity = inv.LegalEntity;
                            subinvoice.InvoiceDate = inv.InvoiceDate;
                            subinvoice.CreditTerm = inv.CreditTrem;
                            subinvoice.DueDate = inv.DueDate;
                            subinvoice.PurchaseOrder = inv.PoNum;
                            subinvoice.SaleOrder = inv.SoNum;
                            subinvoice.RBO = inv.MstCustomer;
                            subinvoice.InvoiceCurrency = inv.Currency;
                            subinvoice.OriginalInvoiceAmount = inv.OriginalAmt.ToString();
                            subinvoice.OutstandingInvoiceAmount = inv.BalanceAmt;
                            subinvoice.DaysLate = (AppContext.Current.User.Now.Date - Convert.ToDateTime(inv.DueDate).Date).Days.ToString();
                            subinvoice.InvoiceTrack = !string.IsNullOrEmpty(inv.TrackStates) == false ? "" : Helper.CodeToEnum<TrackStatus>(inv.TrackStates).ToString().Replace("_", " ");
                            subinvoice.Status = !String.IsNullOrEmpty(inv.States) ? Helper.CodeToEnum<InvoiceStatus>(inv.States).ToString().Replace("_", " ") : "";
                            //added by zhangYu 20151205 start
                            subinvoice.PtpDate = inv.PtpDate;
                            //added by zhangYu 20151205 End
                            subinvoice.DocumentType = inv.Class;
                            subinvoice.Comments = inv.Comments;
                            subinvoice.StandardInvoiceAmount = inv.StandardBalanceAmt;
                            subinvoiceList.Add(subinvoice);
                        }
                    }
                    else
                    {
                        subinvoice = new SoaInvoice();
                        subinvoiceList.Add(subinvoice);
                    }
                    // logic to build reminder calendars
                    ReminderCalendar calendar = new ReminderCalendar();
                    // 1. SOA
                    var tracking = calendar.GetTracking(reminders.FindAll(a => a.CustomerNum == item && string.IsNullOrEmpty(a.LegalEntity)));
                    // 2. Other reminders
                    tracking = calendar.GetTracking(reminders.FindAll(a => a.CustomerNum == item && a.LegalEntity == legal.LegalEntity), tracking);
                    // 3. Append other information shown in UI;
                    //bdSer.AppendTrackingConfig(tracking, deal, item, legal.LegalEntity);
                    sublegal.SubTracking = tracking;
                    sublegal.SubInvoice = subinvoiceList;
                    sublegalList.Add(sublegal);
                }
                sendsoa.SubLegal = sublegalList;
                sendsoaList.Add(sendsoa);
            }
            #endregion
           
            return sendsoaList.AsQueryable<SendSoaHead>();

        }

        public IEnumerable<SendSoaHead> CreateSendMailForArrow(string strCustomerSites)
        {
            string oper = AppContext.Current.User.Id.ToString();
            string deal = CurrentDeal;

            IBaseDataService bdSer = SpringFactory.GetObjectImpl<IBaseDataService>("BaseDataService");

            #region createsoalist
            string[] cusGroup = strCustomerSites.Split(',');
            string suid = string.Empty;
            string custno = string.Empty;
            string entityid = string.Empty;
            string[] cusCust = new string[cusGroup.Count()];
            string[] siteCust = new string[cusGroup.Count()];
            for (int i = 0; i < cusGroup.Count(); i++)
            {
                cusCust[i] = cusGroup[i].Split(';')[0];
                siteCust[i]= cusGroup[i].Split(';')[2];
                suid = cusGroup[i].Split(';')[2];
                custno = cusGroup[i].Split(';')[0];
                entityid= cusGroup[i].Split(';')[1];
            }

            //cus
            var cusList = CommonRep.GetQueryable<Customer>()
                .Where(o => o.Deal == CurrentDeal && siteCust.Distinct().Contains(o.SiteUseId) &&cusCust.Distinct().Contains(o.CustomerNum)).Include<Customer, CustomerGroupCfg>(c => c.CustomerGroupCfg).ToList();
            Customer cus = new Customer();
            //aging
            var cusAgingList = CommonRep.GetQueryable<CustomerAging>()
                .Where(o => o.Deal == deal && cusGroup.Contains(o.CustomerNum + ";" + o.LegalEntity+";"+o.SiteUseId)).ToList();
            //sendsoa
            List<SendSoaHead> sendsoaList = new List<SendSoaHead>();
            SendSoaHead sendsoa = new SendSoaHead();
            //customerchangehis=>class
            var classList = CommonRep.GetQueryable<CustomerLevelView>()
                .Where(o => o.Deal == deal && cusCust.Distinct().Contains(o.CustomerNum)&&siteCust.Distinct().Contains(o.SiteUseId)).ToList();
            CustomerLevelView level = new CustomerLevelView();
            //SpecialNotes
            var SNList = CommonRep.GetQueryable<SpecialNote>().Where(o => o.Deal == deal
                                                                && cusGroup.Contains(o.CustomerNum + ";" + o.LegalEntity + ";" + o.SiteUseId)).ToList();
            //Rate
            var rateList = CommonRep.GetQueryable<RateTran>()
                .Where(o => o.Deal == CurrentDeal && o.EffectiveDate <= AppContext.Current.User.Now.Date && o.ExpiredDate >= AppContext.Current.User.Now.Date).ToList();
            //agingDT
            DateTime agingDT = new DateTime();
            PeroidService pservice = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");
            PeriodControl currentP = pservice.getcurrentPeroid();
            agingDT = dataConvertToDT(currentP.PeriodEnd.ToString());
            DateTime agingDT90 = new DateTime();
            agingDT90 = agingDT.AddDays(-90);
            //invoice
            var oldinvoiceList = CommonRep.GetQueryable<InvoiceAging>()
                .Where(o => o.Deal == deal && cusGroup.Contains(o.CustomerNum + ";" + o.LegalEntity + ";" + o.SiteUseId)).ToList();
            List<InvoiceAging> newinvoiceList = new List<InvoiceAging>();
            newinvoiceList = oldinvoiceList;

            // IsCostomerContact
            bool isCusContact = CommonRep.GetQueryable<Contactor>().Where(o => siteCust.Distinct().Contains(o.SiteUseId) && cusCust.Distinct().Contains(o.CustomerNum) && o.IsCostomerContact == true).Count() > 0;

            PeriodControl periodCtrl = CommonRep.GetQueryable<PeriodControl>().Where(x => x.SoaFlg == "1").FirstOrDefault();
            DateTime? ReconciliationDay = CommonRep.GetQueryable<CustomerPaymentCircle>().Where(o => siteCust.Distinct().Contains(o.SiteUseId) && cusCust.Distinct().Contains(o.CustomerNum) && o.Reconciliation_Day != null)
                    .ToList()
                    .Where(o => o.Reconciliation_Day >= periodCtrl.PeriodBegin && o.Reconciliation_Day < periodCtrl.PeriodEnd)
                    .Select(x => x.Reconciliation_Day).FirstOrDefault();

            try
            {
                foreach (var item in newinvoiceList)
                {
                    item.StandardBalanceAmt = item.BalanceAmt;
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw;
            }

            IDunningService service = SpringFactory.GetObjectImpl<IDunningService>("DunningReminderService");
            List<CollectorAlert> reminders = service.GetEstimatedRemindersForArrow(cusCust.Distinct().ToList(), suid, null);
            SqlParameter[] para = new SqlParameter[3];
            para[0] = new SqlParameter("@SiteUseId", suid);
            para[1] = new SqlParameter("@CustomerNo", custno);
            para[2] = new SqlParameter("@LegalEntity", entityid);
            DataTable dt = CommonRep.GetDBContext().Database.ExecuteDataTable("P_Dun_STRATEGY", para);
            int dunflag = int.Parse(dt.Rows[0][0].ToString());
            List<Dispute> dispList = CommonRep.GetQueryable<Dispute>().Where(o => cusCust.Distinct().Contains(o.CustomerNum)&&siteCust.Distinct().Contains(o.SiteUseId) && o.Deal == AppContext.Current.User.Deal).Select(o => o).ToList();

            List<ContactHistory> ContactList = new List<ContactHistory>();
            ContactList = CommonRep.GetDbSet<ContactHistory>().Where(o => o.Deal == deal && cusCust.Distinct().Contains(o.CustomerNum) && siteCust.Distinct().Contains(o.SiteUseId)).ToList();
            string siteusrid = siteCust[0];
            string DunningName = "New Customer";
            IQueryable<T_CustomerAssessment> custass = CommonRep.GetDbSet<T_CustomerAssessment>().Where(i => i.CustomerId == custno && i.SiteUseId == suid);
            IQueryable<T_AssessmentType> assmapping = CommonRep.GetDbSet<T_AssessmentType>();
            var assType = from x in custass
                          join y in assmapping
                          on x.AssessmentType equals y.Id
                          into xy
                          from y in xy.DefaultIfEmpty()
                          select new { Name = y != null ? y.Name : "New Customer" };
            if (assType != null && assType.Count() > 0)
            {
                foreach (var name in assType)
                {
                    DunningName = name.Name;
                    break;
                }
            }
            foreach (var item in cusCust.Distinct())
            {
                sendsoa = new SendSoaHead();
                cus = cusList.Find(m => m.Deal == deal && m.CustomerNum == item&&m.SiteUseId== siteusrid);
                level = classList.Find(m => m.Deal == deal && m.CustomerNum == item&& m.SiteUseId== siteusrid);
                var newCusAgingList = cusAgingList.FindAll(m => m.Deal == deal && m.CustomerNum == item&&m.SiteUseId== siteusrid);
                sendsoa.Deal = deal;
                sendsoa.CustomerCode = item;
                sendsoa.CustomerName = cus.CustomerName;
                sendsoa.IsCostomerContact = isCusContact == true ? "Y" : "N";
                sendsoa.ReconciliationDay = ReconciliationDay == null ? new DateTime(2099, 12, 31) : (DateTime)ReconciliationDay;
                sendsoa.TotalBalance = newCusAgingList.Sum(m => m.TotalAmt);//sites
                sendsoa.CustomerClass = (string.IsNullOrEmpty(level.ClassLevel) == true ? "LV" : level.ClassLevel)
                    + (string.IsNullOrEmpty(level.RiskLevel) == true ? "LR" : level.RiskLevel);
                //Start add by xuan.wu for Arrow adding
                sendsoa.SiteUseId = cus.SiteUseId;
                sendsoa.LegalEntity = newCusAgingList[0].LegalEntity;
                sendsoa.Sales = cus.FSR;
                sendsoa.CreditLimit = newCusAgingList[0].CreditLimit;
                sendsoa.CreditTremDescription = newCusAgingList[0].CreditTrem;
                sendsoa.Total_Balance = newCusAgingList[0].TotalAmt;
                sendsoa.Current_Balance = newCusAgingList[0].CurrentAmt;
                sendsoa.Amount = (newinvoiceList.Where(c => c.TrackStates != "013" && c.TrackStates != "016").Sum(m => m.BalanceAmt));
                sendsoa.DunFlag = dunflag;
                sendsoa.Assessment = DunningName;
                //End add by xuan.wu for Arrow adding
                //contactHistory
                List<SubContactHistory> ContactHisList = new List<SubContactHistory>();
                SubContactHistory ContactHis = new SubContactHistory();
                var OldConHisList = ContactList.Where(o => o.CustomerNum == item&&o.SiteUseId==siteusrid);
                int ihis = 1;
                foreach (var his in OldConHisList)
                {
                    ContactHis = new SubContactHistory();
                    ContactHis.SortId = ihis;
                    ContactHis.Deal = his.Deal;
                    ContactHis.CustomerNum = his.CustomerNum;
                    ContactHis.LegalEntity = his.LegalEntity;
                    ContactHis.ContactType = his.ContactType;
                    ContactHis.ContactDate = his.ContactDate;
                    ContactHis.ContactId = his.ContactId;
                    ContactHis.Comments = his.Comments;
                    ContactHisList.Add(ContactHis);
                    ihis++;
                }
                sendsoa.SubContactHistory = ContactHisList;

                var disputeList = dispList.Where(o => o.CustomerNum == item&&o.SiteUseId==suid&&(o.Status!= "026011"&& o.Status != "026012")); ;
                List<Dispute> DisputeList = new List<Dispute>();
                List<string> trackerStatusList = new List<string>() { "000", "001", "002", "003", "004", "005", "006", "007", "008", "009", "010", "011", "012", "015" };
                Dispute dispute = new Dispute();
                int countId = 1;
                foreach (var disp in disputeList)
                {

                    dispute = new Dispute();
                    dispute.Id = disp.Id;
                    dispute.Deal = disp.Deal;
                    dispute.Eid = disp.Eid;
                    dispute.CloseDate = disp.CloseDate;
                    dispute.Comments = disp.Comments;
                    dispute.ContactId = disp.ContactId;
                    dispute.CreateDate = disp.CreateDate;
                    dispute.CreatePerson = disp.CreatePerson;
                    dispute.CustomerNum = disp.CustomerNum;
                    dispute.IssueReason = Helper.CodeToEnum<DisputeReason>(disp.IssueReason).ToString();
                    dispute.Status = Helper.CodeToEnum<DisputeStatus>(disp.Status).ToString();
                    dispute.sortId = countId;
                    DisputeList.Add(dispute);

                    countId++;
                }
                sendsoa.SubDisputeList = DisputeList;

                //Legal
                List<SoaLegal> sublegalList = new List<SoaLegal>();
                SoaLegal sublegal = new SoaLegal();
                foreach (var legal in newCusAgingList)
                {
                    var invoice = newinvoiceList
                        .FindAll(m => m.CustomerNum == item && m.LegalEntity == legal.LegalEntity&&m.SiteUseId==siteusrid);
                    var inv1 = invoice.Where(m => trackerStatusList.Contains(m.TrackStates) && m.Class == "INV").OrderBy(m => m.DueDate)
                        .Union(invoice.Where(m => trackerStatusList.Contains(m.TrackStates) && m.Class != "INV").OrderBy(m => m.DueDate)).ToList();
                    sublegal = new SoaLegal();
                    sublegal.LegalEntity = legal.LegalEntity;
                    sublegal.Country = legal.Country;
                    sublegal.CreditLimit = legal.CreditLimit;
                    sublegal.TotalARBalance = legal.TotalAmt;
                    sublegal.PastDueAmount = legal.DueoverTotalAmt;
                    sublegal.CreditBalance = invoice.FindAll(m => m.BalanceAmt < 0).Sum(m => m.StandardBalanceAmt);
                    sublegal.CurrentBalance = legal.CurrentAmt;
                    sublegal.FCollectableAmount = inv1
                        .FindAll(m => m.DueDate <= agingDT && (m.Class == "DM" || m.Class == "INV")).Sum(m => m.StandardBalanceAmt);
                    sublegal.FOverdue90Amount = inv1
                        .FindAll(m => m.DueDate <= agingDT90 && (m.Class == "DM" || m.Class == "INV")).Sum(m => m.StandardBalanceAmt);
                    var SN = SNList.Find(m => m.CustomerNum == item && m.LegalEntity == legal.LegalEntity&&m.SiteUseId==siteusrid);
                    if (SN == null)
                    {
                        sublegal.SpecialNotes = "";
                    }
                    else
                    {
                        sublegal.SpecialNotes = SN.SpecialNotes;
                    }
                    List<SoaInvoice> subinvoiceList = new List<SoaInvoice>();
                    SoaInvoice subinvoice = new SoaInvoice();
                    if (inv1.Count > 0)
                    {
                        foreach (var inv in inv1)
                        {
                            //Wait_for_1st_Time_Confirm_PTP
                            if (inv.TrackStates.Equals(Helper.EnumToCode<TrackStatus>(TrackStatus.Responsed_OverDue_Reason)))
                            {
                                sendsoa.Count1PTP += 1;
                            }
                            //Wait_for_2st_Time_Confirm_PTP
                            if (inv.TrackStates.Equals(Helper.EnumToCode<TrackStatus>(TrackStatus.Wait_for_2nd_Time_Confirm_PTP)))
                            {
                                sendsoa.Count2PTP += 1;
                            }
                            //Wait_for_Payment_Reminding
                            if (inv.TrackStates.Equals(Helper.EnumToCode<TrackStatus>(TrackStatus.Wait_for_Payment_Reminding)))
                            {
                                sendsoa.CountPaymentReminding += 1;
                            }
                            //Wait_for_1st_Time_Dunning
                            if (inv.TrackStates.Equals(Helper.EnumToCode<TrackStatus>(TrackStatus.Wait_for_1st_Time_Dunning)))
                            {
                                sendsoa.Count1Dunning += 1;
                            }
                            //Wait_for_2nd_Time_Dunning
                            if (inv.TrackStates.Equals(Helper.EnumToCode<TrackStatus>(TrackStatus.Wait_for_2nd_Time_Dunning)))
                            {
                                sendsoa.Count2Dunning += 1;
                            }

                            subinvoice = new SoaInvoice();
                            subinvoice.InvoiceId = inv.Id;
                            subinvoice.InvoiceNum = inv.InvoiceNum;
                            subinvoice.CustomerNum = inv.CustomerNum;
                            subinvoice.CustomerName = inv.CustomerName;
                            subinvoice.LegalEntity = inv.LegalEntity;
                            subinvoice.InvoiceDate = inv.InvoiceDate;
                            subinvoice.CreditTerm = inv.CreditTrem;
                            subinvoice.DueDate = inv.DueDate;
                            subinvoice.PurchaseOrder = inv.PoNum;
                            subinvoice.SaleOrder = inv.SoNum;
                            subinvoice.RBO = inv.MstCustomer;
                            subinvoice.InvoiceCurrency = inv.Currency;
                            subinvoice.OriginalInvoiceAmount = inv.OriginalAmt.ToString();
                            subinvoice.OutstandingInvoiceAmount = inv.BalanceAmt;
                            subinvoice.DaysLate = (AppContext.Current.User.Now.Date - Convert.ToDateTime(inv.DueDate).Date).Days.ToString();
                            subinvoice.InvoiceTrack = !string.IsNullOrEmpty(inv.TrackStates) == false ? "" : Helper.CodeToEnum<TrackStatus>(inv.TrackStates).ToString().Replace("_", " ");
                            subinvoice.Status = !String.IsNullOrEmpty(inv.States) ? Helper.CodeToEnum<InvoiceStatus>(inv.States).ToString().Replace("_", " ") : "";
                            //added by zhangYu 20151205 start
                            subinvoice.PtpDate = inv.PtpDate;
                            //added by zhangYu 20151205 End
                            subinvoice.DocumentType = inv.Class;
                            subinvoice.Comments = inv.Comments;
                            subinvoice.StandardInvoiceAmount = inv.StandardBalanceAmt;
                            //Start add by xuan.wu for Arrow adding
                            subinvoice.SiteUseId = inv.SiteUseId;
                            subinvoice.Sales = inv.Sales;
                            subinvoice.States = inv.States;
                            subinvoice.BALANCE_AMT = inv.BalanceAmt;
                            subinvoice.WoVat_AMT = inv.WoVat_AMT;
                            subinvoice.AgingBucket = inv.AgingBucket;
                            subinvoice.Ebname = inv.Eb;
                            subinvoice.COLLECTOR_NAME = inv.CollectorName;
                            subinvoice.DueDays = inv.DaysLateSys;
                            subinvoice.InClass = inv.Class;
                            //End add by xuan.wu for Arrow adding
                            subinvoiceList.Add(subinvoice);
                        }
                    }
                    else
                    {
                        subinvoice = new SoaInvoice();
                        subinvoiceList.Add(subinvoice);
                    }
                    // logic to build reminder calendars
                    ReminderCalendar calendar = new ReminderCalendar();
                    // 1. SOA
                    var tracking = calendar.GetTracking(reminders.FindAll(a => a.CustomerNum == item && string.IsNullOrEmpty(a.LegalEntity)));
                    // 2. Other reminders
                    tracking = calendar.GetTracking(reminders.FindAll(a => a.CustomerNum == item && a.LegalEntity == legal.LegalEntity), tracking);
                    // 3. Append other information shown in UI;
                    sublegal.SubTracking = tracking;
                    sublegal.SubInvoice = subinvoiceList;
                    sublegalList.Add(sublegal);
                }
                sendsoa.SubLegal = sublegalList;
                sendsoaList.Add(sendsoa);
            }
            #endregion

            return sendsoaList.AsQueryable<SendSoaHead>();

        }

        public DateTime dataConvertToDT(string strData)
        {
            DateTime dt = new DateTime();
            if (!string.IsNullOrEmpty(strData.Trim()))
            {
                return Convert.ToDateTime(strData);
            }

            return dt;
        }

        public IEnumerable<ContactHistory> GetContactHistory(string CustNumsFCH)
        {
            string[] cus = CustNumsFCH.Split(',');
            List<ContactHistory> ConHisList = new List<ContactHistory>();
            List<ContactHistory> subConHisList = new List<ContactHistory>();
            foreach (var item in cus)
            {
                int i = 1;
                subConHisList = new List<ContactHistory>();
                subConHisList = CommonRep.GetDbSet<ContactHistory>().Where(o => (o.CustomerNum)  == item).ToList();
                subConHisList.ForEach(sb =>
                {
                    sb.sortId = i++;
                });
                ConHisList.AddRange(subConHisList);
            }
            return ConHisList.AsQueryable();
        }

        public HttpResponseMessage ExportAccountList(string custCode, string custName, string level, string billCode, string billName, string legal,
                                                       string state, string tstate, string invNum, string poNum, string soNum, string memo,string oper)
        {
            List<Customer.ExpCustomerDto> lstCustomer = new List<Customer.ExpCustomerDto>();
            List<InvoiceAging> invoiceList = new List<InvoiceAging>();
            InvoiceAging invoice = new InvoiceAging();
            string templateFile = "";
            string fileName = "";
            string tmpFile = "";

            try
            {
                //模板文件  
                templateFile = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["ExportAccountsTemplate"].ToString());
                fileName = AppContext.Current.User.EID + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xlsx";
                tmpFile = Path.Combine(Path.GetTempPath(), fileName);
                if (state == "undefined") 
                {
                    state = null;
                }
                if (tstate == "undefined")
                {
                    tstate = null;
                }
                if (invNum == "undefined")
                {
                    invNum = null;
                }
                if (soNum == "undefined")
                {
                    soNum = null;
                }
                if (poNum == "undefined")
                {
                    poNum = null;
                }
                if (memo == "undefined")
                {
                    memo = null;
                }

                var tempInvoices = getAllInvoiceByUser(state, tstate, invNum, soNum, poNum, memo);

                if (!string.IsNullOrEmpty(custCode) && custCode != "undefined")
                {
                    tempInvoices = tempInvoices.Where(o => o.CustomerNum.IndexOf(custCode) >= 0);
                }
                if (!string.IsNullOrEmpty(custName) && custName != "undefined")
                {
                    tempInvoices = tempInvoices.Where(o => o.CustomerName.IndexOf(custName) >= 0);
                }
                if (!string.IsNullOrEmpty(billCode) && billCode != "undefined")
                {
                    tempInvoices = tempInvoices.Where(o => o.BillGroupCode.IndexOf(billCode) >= 0);
                }
                if (!string.IsNullOrEmpty(billName) && billName != "undefined")
                {
                    tempInvoices = tempInvoices.Where(o => o.BillGroupName.IndexOf(billName) >= 0);
                }
                if (!string.IsNullOrEmpty(level) && level != "null" && level != "undefined")
                {
                    tempInvoices = tempInvoices.Where(o => o.Class == level);
                }
                if (!string.IsNullOrEmpty(legal) && legal != "null" && legal != "undefined")
                {
                    tempInvoices = tempInvoices.Where(o => o.LegalEntity == legal);
                }
                if (!string.IsNullOrEmpty(oper) && oper != "null" && oper != "undefined")
                {
                    tempInvoices = tempInvoices.Where(o => o.Collector == oper);
                }


                this.setData(templateFile, tmpFile, tempInvoices);

                HttpResponseMessage response = new HttpResponseMessage();
                response.StatusCode = HttpStatusCode.OK;
                MemoryStream fileStream = new MemoryStream();
                if (File.Exists(tmpFile))
                {
                    using (FileStream fs = File.OpenRead(tmpFile))
                    {
                        fs.CopyTo(fileStream);
                    }
                }
                else
                {
                    Exception ex = new OTCServiceException("Get file failed because file not exist with physical path: " + tmpFile);
                    Helper.Log.Error(ex.Message, ex);
                    throw ex;
                }
                Stream ms = fileStream;
                ms.Position = 0;
                response.Content = new StreamContent(ms);
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = fileName
                };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response.Content.Headers.ContentLength = ms.Length;

                return response;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
            finally
            {
            }
        }

        public List<string> getPayerList(string siteUseID)
        {
            var paymentInovice = CommonRep.GetQueryable<T_PTPPayment>()
                .Where(p => p.SiteUseId == siteUseID && p.Payer != "" && p.Payer != null).Select(p => p.Payer).Distinct();
            return paymentInovice.ToList();
        }

        public IQueryable<T_PTPPayment> getPTPPayment(string custNum, string siteUseID)
        {
            var paymentInovice = CommonRep.GetQueryable<T_PTPPayment_Invoice>()
                .Select(p => p.PTPPaymentId).Distinct();

            var result = from p in CommonRep.GetQueryable<T_PTPPayment>()
                         where !paymentInovice.Contains(p.Id) && p.CustomerNum == custNum 
                         && p.SiteUseId == siteUseID && p.PTPStatus == "001"
                         select p;
                          
            return result;
        }

        public bool updatePTPPayment(T_PTPPayment ptpPayment)
        {
            try
            {
                DateTime? promiseDate = ptpPayment.PromiseDate;
                bool? isPartialPay = ptpPayment.IsPartialPay;
                string payer = ptpPayment.Payer;
                decimal? promissAmount = ptpPayment.PromissAmount;
                string paymentMethod = ptpPayment.PaymentMethod;
                string contact = ptpPayment.Contact;
                string ptpStatus = ptpPayment.PTPStatus;
                string comments = ptpPayment.Comments;
                bool? isForwarder = ptpPayment.IsForwarder;

                Helper.Log.Info(ptpPayment);
                T_PTPPayment old = CommonRep.FindBy<T_PTPPayment>(ptpPayment.Id);
                ptpPayment = old;
                ptpPayment.PromiseDate = promiseDate;
                ptpPayment.IsPartialPay = isPartialPay;
                ptpPayment.Payer = payer;
                ptpPayment.PromissAmount = promissAmount;
                ptpPayment.PaymentMethod = paymentMethod;
                ptpPayment.Contact = contact;
                ptpPayment.PTPStatus = ptpStatus;
                ptpPayment.Comments = comments;
                ptpPayment.IsForwarder = isForwarder;
                ObjectHelper.CopyObjectWithUnNeed(ptpPayment, old, new string[] { "Id", "CustomerNum", "SiteUseId" });
                CommonRep.Commit();
                return true;
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);
            }
        }

        #region Set Accounts Report Datas
        private void setData(string templateFileName, string tmpFile, IQueryable<AllAccountInfo> lstDatas)
        {
            int rowNo = 1;

            try
            {
                NpoiHelper helper = new NpoiHelper(templateFileName);
                helper.Save(tmpFile, true);
                helper = new NpoiHelper(tmpFile);
                string sheetName = "";

                foreach (string sheet in helper.Sheets)
                {
                    sheetName = sheet;
                    break;
                }

                //设置sheet
                helper.ActiveSheetName = sheetName;

                //设置Excel的内容信息
                foreach (var lst in lstDatas)
                {
                    helper.SetData(rowNo, 0, lst.LegalEntity);
                    helper.SetData(rowNo, 1, lst.CustomerNum);
                    helper.SetData(rowNo, 2, lst.CustomerName);
                    helper.SetData(rowNo, 3, lst.BillGroupCode);
                    helper.SetData(rowNo, 4, lst.BillGroupName);
                    helper.SetData(rowNo, 5, lst.Collector);
                    helper.SetData(rowNo, 6, lst.Team);
                    helper.SetData(rowNo, 7, lst.Class);
                    helper.SetData(rowNo, 8, lst.Country);
                    helper.SetData(rowNo, 9, lst.ArBalanceAmtPeroid);
                    helper.SetData(rowNo, 10, lst.BalanceAmt);
                    helper.SetData(rowNo, 11, lst.OverDue90Amt);
                    helper.SetData(rowNo, 12, lst.AdjustedOver90);
                    helper.SetData(rowNo, 13, lst.PtpAmt);
                    helper.SetData(rowNo, 14, lst.BrokenPTPAmt);
                    helper.SetData(rowNo, 15, lst.DisputeAmt);
                    helper.SetData(rowNo, 16, string.IsNullOrEmpty(lst.SoaDate.ToString()) ? "" : lst.SoaDate.Value.ToString("yyyy-MM-dd"));
                    helper.SetData(rowNo, 17, string.IsNullOrEmpty(lst.SecondDate.ToString()) ? "" : lst.SecondDate.Value.ToString("yyyy-MM-dd"));
                    helper.SetData(rowNo, 18, string.IsNullOrEmpty(lst.FinalDate.ToString()) ? "" : lst.FinalDate.Value.ToString("yyyy-MM-dd"));
                    helper.SetData(rowNo, 19, lst.SpecialNotes);
                    helper.SetData(rowNo, 20, lst.PaymentTerm);
                    helper.SetData(rowNo, 21, lst.Contact);
                    rowNo++;
                }

                //formula calcuate result
                helper.Save(tmpFile, true);
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }
        #endregion
    }
}
