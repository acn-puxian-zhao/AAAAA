using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Intelligent.OTC.Business
{
    public class JobService : IJobService
    {
        public OTCRepository CommonRep { get; set; }

        public List<SysTypeDetail> GetSysTypeDetail(string strTypecode)
        {

            List<SysTypeDetail> res = CommonRep.GetDbSet<SysTypeDetail>().OrderBy(td => td.Seq).ToList();
            var Result = res.Where(d => d.TypeCode == strTypecode).ToList();
            return Result;
        }

        public bool addUpdFileHistory(FileUploadHistory updFileHistory)
        {
            try
            {
                CommonRep.Add(updFileHistory);
                CommonRep.Commit();
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public List<FileUploadHistory> GetAutoData(string legal)
        {
            //string strDeal = AppContext.Current.User.Deal.ToString();
            string strUntreated = Helper.EnumToCode<UploadStates>(UploadStates.Untreated);
            var nowDate = DateTime.Now.Date;
            return CommonRep.GetQueryable<FileUploadHistory>().Where(o =>
                                         o.Operator == "auto"
                                        && o.ProcessFlag == strUntreated
                                        && o.LegalEntity == legal
                                        && o.UploadTime >= nowDate
                                        ).Select(o => o).OrderByDescending(o => o.UploadTime).ToList();
        }

        public List<FileUploadHistory> GetAutoDataCus()
        {
            string strUntreated = Helper.EnumToCode<UploadStates>(UploadStates.Untreated);
            var nowDate = DateTime.Now.Date;
            string cus = Helper.EnumToCode<FileType>(FileType.CustLocalize);
            return CommonRep.GetQueryable<FileUploadHistory>().Where(o =>
                                         o.Operator == "auto"
                                        && o.ProcessFlag == strUntreated
                                        && o.UploadTime >= nowDate
                                        && o.FileType == cus
                                        ).Select(o => o).OrderByDescending(o => o.UploadTime).ToList();
        }

        public List<FileUploadHistory> GetAutoDataVAT()
        {
            string strUntreated = Helper.EnumToCode<UploadStates>(UploadStates.Untreated);
            var nowDate = DateTime.Now.Date;
            string cus = Helper.EnumToCode<FileType>(FileType.VAT);
            return CommonRep.GetQueryable<FileUploadHistory>().Where(o =>
                                         o.Operator == "auto"
                                        && o.ProcessFlag == strUntreated
                                        && o.UploadTime >= nowDate
                                        && o.FileType == cus
                                        ).Select(o => o).OrderByDescending(o => o.UploadTime)
                                        .Take(1) //由于VAT发票所有Legal都在一个文件上,所以只取最新的一个文件,-付林林
                                        .ToList();
        }

        public List<FileUploadHistory> GetAutoDataInvoiceDetail()
        {
            string strUntreated = Helper.EnumToCode<UploadStates>(UploadStates.Untreated);
            var nowDate = DateTime.Now.Date;
            string cus = Helper.EnumToCode<FileType>(FileType.InvoiceDetail);
            return CommonRep.GetQueryable<FileUploadHistory>().Where(o =>
                                         o.Operator == "auto"
                                        && o.ProcessFlag == strUntreated
                                        && o.UploadTime >= nowDate
                                        && o.FileType == cus
                                        ).Select(o => o).OrderByDescending(o => o.UploadTime).ToList();
        }

        public List<FileUploadHistory> GetAutoDataToday()
        {
            string strUntreated = Helper.EnumToCode<UploadStates>(UploadStates.Untreated);
            string strSuccess = Helper.EnumToCode<UploadStates>(UploadStates.Success);
            var nowDate = DateTime.Now.Date;
            return CommonRep.GetQueryable<FileUploadHistory>().Where(o =>
                                         o.Operator == "auto"
                                        && (o.ProcessFlag == strUntreated || o.ProcessFlag == strSuccess)
                                        && o.UploadTime >= nowDate
                                        ).Select(o => o).OrderByDescending(o => o.UploadTime).ToList();
        }

        public string GetDealByLegal(string legal)
        {
            string deal = string.Empty;
            deal = CommonRep.GetQueryable<Sites>().Where(o =>
                                          o.LegalEntity == legal
                                        ).Select(o => o.Deal).FirstOrDefault().ToString();
            return deal;
        }

        public List<string> GetLegalEntitys()
        {
            return CommonRep.GetQueryable<Sites>().Select(o => o.LegalEntity).Distinct().ToList();
        }

        public bool GetLegalEntityIsFinish(string legalEntity)
        {
            var nowDate = DateTime.Now.Date;
            string strState = Helper.EnumToCode<UploadStates>(UploadStates.Success);//后期需要改，改成submit
            string strAcc = Helper.EnumToCode<FileType>(FileType.Account);
            string strInv = Helper.EnumToCode<FileType>(FileType.Invoice);
            var succLegalAcc = CommonRep.GetQueryable<FileUploadHistory>().Where(o =>
                                          o.Operator == "auto"
                                         && o.ProcessFlag == strState
                                         && o.LegalEntity == legalEntity
                                         && o.UploadTime >= nowDate
                                         && o.FileType == strAcc
                                        ).Select(o => o).ToList();

            var succLegalInv = CommonRep.GetQueryable<FileUploadHistory>().Where(o =>
                                          o.Operator == "auto"
                                         && o.ProcessFlag == strState
                                         && o.LegalEntity == legalEntity
                                         && o.UploadTime >= nowDate
                                         && o.FileType == strInv
                                        ).Select(o => o).ToList();
            if (succLegalAcc.Count() > 0 && succLegalInv.Count() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<FileUploadHistory> GetAllPendingUploadFile()
        {
            string strUploadSuccess = Helper.EnumToCode<UploadStates>(UploadStates.Success);
            string strUploadSubmitted = Helper.EnumToCode<UploadStates>(UploadStates.Submitted);
            return CommonRep.GetQueryable<FileUploadHistory>().Where(o =>
                                                 o.ProcessFlag == strUploadSuccess
                                           && o.SubmitFlag != strUploadSubmitted
                                           ).ToList().Select(o => new FileUploadHistory { Deal = o.Deal }).GroupBy(o => o.Deal).Select(g => g.First()).OrderByDescending(o => o.Deal).ToList();
        }

        public DateTime CurrentTime
        {
            get
            {
                return AppContext.Current.User.Now;
            }
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

        //这段代码用于备份AllAccount数据到临时表
        //从原来的AllAccount查询COPY过来（由于使用Linq，用SQL重写太复杂）
        public void getAllInvoiceByUserForArrow(string isPTPOverDue, string invoiceState = "", string invoiceTrackState = "", string invoiceNum = "", string soNum = "", string poNum = "", string invoiceMemo = "")
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
            DateTime dtperEndOver90 = DateTime.Now;

            try
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    //取得当前peroid
                    PeroidService perService = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");
                    PeriodControl per = perService.getcurrentPeroid();

                    if (per == null)
                    {
                        Helper.Log.Info("There is no Data");
                        return;
                    }

                    //1 .find all customer by collector
                    CustomerService custService = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");


                    var customerLevels = from level in custService.GetCustomerLevelForAllCus(per.Id, 1)
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
                                             ValueLevel = level.ValueLevel,
                                             RiskLevel = level.RiskLevel,
                                             TeamName = "",
                                             CS = level.CS,
                                             SiteUseId = level.SiteUseId,
                                             iclass = level.Class,
                                             Sales = level.Sales
                                         };

                    agings = CommonRep.GetDbSet<CustomerAging>().Where(o => o.Deal == CurrentDeal);
                    invagings = CommonRep.GetDbSet<InvoiceAging>().Where(o => o.Deal == CurrentDeal &&
                                                                (o.States == strOpen || o.States == strPtp || o.States == strBrokenPTP ||
                                                                o.States == strDisp || o.States == strPartialPay || o.States == strHold || o.States == strPayment));
                    
                    //var rates = from rate in CommonRep.GetDbSet<RateTran>()
                    //            where rate.Deal == CurrentDeal && rate.EffectiveDate <= dt && rate.ExpiredDate >= dt
                    //            select new { rate.Rate, rate.ForeignCurrency };

                    //notes = CommonRep.GetDbSet<SpecialNote>().Where(o => o.Deal == CurrentDeal);
                    
                    //alerts = CommonRep.GetDbSet<CollectorAlert>().Where(o => o.Deal == CurrentDeal
                    //                                                    && o.PeriodId == per.Id
                    //                                                    && o.Status == "Finish");

                    //var alert1s = from alert1 in alerts.Where(o => o.AlertType == 1)
                    //              group alert1 by new { CustomerNum = alert1.CustomerNum, SiteUseId = alert1.SiteUseId } into g
                    //              select new { CustomerNum = g.Key.CustomerNum, SiteUseId = g.Key.SiteUseId, Status1 = g.Max(s => s.ActionDate) };

                    //var alerts2 = from alert2 in alerts.Where(o => o.AlertType == 2)
                    //              group alert2 by new { LegalEntity = alert2.LegalEntity, CustomerNum = alert2.CustomerNum, SiteUseId = alert2.SiteUseId } into g
                    //              select new { LegalEntity = g.Key.LegalEntity, CustomerNum = g.Key.CustomerNum, SiteUseId = g.Key.SiteUseId, ActionDate = g.Max(s => s.ActionDate) };

                    //var alerts3 = from alert3 in alerts.Where(o => o.AlertType == 3)
                    //              group alert3 by new { LegalEntity = alert3.LegalEntity, CustomerNum = alert3.CustomerNum, SiteUseId = alert3.SiteUseId } into g
                    //              select new { LegalEntity = g.Key.LegalEntity, CustomerNum = g.Key.CustomerNum, SiteUseId = g.Key.SiteUseId, ActionDate = g.Max(s => s.ActionDate) };

                    //var alert23s = from alert2 in alerts2
                    //               join alert3 in alerts3
                    //                on new { alert2.LegalEntity, alert2.SiteUseId }
                    //                   equals new { alert3.LegalEntity, alert3.SiteUseId }
                    //                into alertstemp
                    //               from alerttemp in alertstemp.DefaultIfEmpty()
                    //               select new
                    //               {
                    //                   LegalEntity = alert2.LegalEntity,
                    //                   CustomerNum = alert2.CustomerNum,
                    //                   SiteUseId = alert2.SiteUseId,
                    //                   Status2 = alert2.ActionDate,
                    //                   Status3 = alerttemp.ActionDate
                    //               };
                    var conalls = CommonRep.GetQueryable<Contactor>().Where(o => o.Deal == AppContext.Current.User.Deal);

                    //var assType = from x in CommonRep.GetDbSet<T_CustomerAssessment>()
                    //              join y in CommonRep.GetDbSet<T_AssessmentType>()
                    //              on x.AssessmentType equals y.Id
                    //              into xy
                    //              from y in xy.DefaultIfEmpty()
                    //              select new { CustomerNum = x.CustomerId, DunningPirority = y.DunningPirority, SiteUseId = x.SiteUseId };

                    var invSum = from x in invagings
                                 where x.DaysLateSys > 0 && x.Class == "INV" && x.TrackStates != "016" && x.TrackStates != "014"
                                 group x by new { LegalEntity = x.LegalEntity, CustomerNum = x.CustomerNum, SiteUseId = x.SiteUseId }
                                 into g
                                 select new
                                 {
                                     LegalEntity = g.Key.LegalEntity,
                                     CustomerNum = g.Key.CustomerNum,
                                     SiteUseId = g.Key.SiteUseId,
                                     PastDueAmount = g.Sum(p => p.BalanceAmt)
                                 };

                    // added by albert 增加 Account Level 级别的 PTP Amount
                    var ptpPaymentList = CommonRep.GetQueryable<V_PTPPayment>();

                    var accPtpAmtList = from x in ptpPaymentList
                                        where x.PTPPaymentType == "PTP" && x.PTPStatus == "001"
                                        group x by new { x.CustomerNum, x.SiteUseId }
                                        into g
                                        select new
                                        {
                                            CustomerNum = g.Key.CustomerNum,
                                            SiteUseId = g.Key.SiteUseId,
                                            AccountPtpAmount = g.Sum(p => p.PromissAmount)
                                        };

                    IQueryable<AllAccountInfo> invsList = from invGrp in
                                                               (from invaging in
                                                                   (from inv in invagings
                                                                    join level in customerLevels on new { inv.CustomerNum, inv.SiteUseId } equals new { level.CustomerNum, level.SiteUseId }
                                                                    //join rate in rates on inv.Currency equals rate.ForeignCurrency
                                                                    //into rateDefts
                                                                    //from rateDeft in rateDefts.DefaultIfEmpty()
                                                                    join ccv in CommonRep.GetQueryable<CustomerContactorView>() on new { Deal = inv.Deal, CustomerNum = inv.CustomerNum, SiteUseId = inv.SiteUseId } equals new { Deal = ccv.Deal, CustomerNum = ccv.CustomerNum, SiteUseId = ccv.SiteUseId }
                                                                    into ccvs
                                                                    from ccvsss in ccvs.DefaultIfEmpty()
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
                                                                        //Rate = rateDeft != null ? rateDeft.Rate : 1,
                                                                        BalanceAmt = inv.BalanceAmt,
                                                                        States = inv.States,
                                                                        CreateDate = inv.CreateDate,
                                                                        Class = level.iclass,
                                                                        DueDate = inv.DueDate,
                                                                        OverDue90Amt = (inv.DueDate <= dtOver90) ? inv.BalanceAmt : 0,
                                                                        AdjustedOver90 = (inv.DueDate <= dtperEndOver90 && inv.BalanceAmt > 0) ? inv.BalanceAmt : 0,
                                                                        PtpAmt = (inv.States == strPtp) ? inv.BalanceAmt : 0,
                                                                        BrokenPTPAmt = inv.States == strBrokenPTP ? inv.BalanceAmt : 0,
                                                                        DisputeAmt = inv.States == strDisp ? inv.BalanceAmt : 0
                                                                        ,Collector = level.Collector
                                                                        ,Team = level.TeamName
                                                                        //Start add by xuan.wu for Arrow adding
                                                                        ,SiteUseId = inv.SiteUseId,
                                                                        COLLECTOR_CONTACT = ccvsss.Contactor,
                                                                        CS = level.CS,
                                                                        CREDIT_LIMIT = inv.CreditLmt,
                                                                        CreditTremDescription = inv.CreditTremDescription,
                                                                        trackstatus = inv.TrackStates
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
                                                                    //invaging.Sales,
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
                                                                    //OverDue90Amt = invgrp.Sum(o => o.OverDue90Amt * o.Rate),
                                                                    //PtpAmt = invgrp.Sum(o => o.PtpAmt * o.Rate),
                                                                    //BrokenPTPAmt = invgrp.Sum(o => o.BrokenPTPAmt * o.Rate),
                                                                    //DisputeAmt = invgrp.Sum(o => o.DisputeAmt * o.Rate),
                                                                    //AdjustedOver90 = invgrp.Sum(o => o.AdjustedOver90 * o.Rate),
                                                                    UnapplidPayment = 0
                                                                    ,Collector = invgrp.Key.Collector
                                                                    ,Team = invgrp.Key.Team
                                                                    //Start add by xuan.wu for Arrow adding
                                                                    ,SiteUseId = invgrp.Key.SiteUseId,
                                                                    COLLECTOR_CONTACT = invgrp.Key.COLLECTOR_CONTACT,
                                                                    CS = invgrp.Key.CS,
                                                                    CREDIT_LIMIT = invgrp.Key.CREDIT_LIMIT,
                                                                    CreditTremDescription = invgrp.Key.CreditTremDescription
                                                                    //End add by xuan.wu for Arrow adding
                                                                })
                                                          join aging in agings on new { invGrp.CustomerNum, invGrp.LegalEntity, invGrp.SiteUseId } equals new { aging.CustomerNum, aging.LegalEntity, aging.SiteUseId }
                                                          //join note in notes on new { invGrp.CustomerNum, invGrp.LegalEntity, invGrp.SiteUseId } equals new { note.CustomerNum, note.LegalEntity, note.SiteUseId }
                                                          //into notestmp
                                                          //from notetmp in notestmp.DefaultIfEmpty()
                                                          //join alert1 in alert1s on new { invGrp.CustomerNum, invGrp.SiteUseId } equals new { alert1.CustomerNum, alert1.SiteUseId }
                                                          //into alert1sTmp
                                                          //from alert1tmp in alert1sTmp.DefaultIfEmpty()
                                                          //join alert23 in alert23s on new { invGrp.CustomerNum, invGrp.LegalEntity, invGrp.SiteUseId } equals new { alert23.CustomerNum, alert23.LegalEntity, alert23.SiteUseId }
                                                          //into alert23sTmp
                                                          //from alert23tmp in alert23sTmp.DefaultIfEmpty()
                                                          join cont in
                                                              (from cust in customerLevels
                                                               from con in conalls
                                                               where cust.CustomerNum == con.CustomerNum && cust.SiteUseId == con.SiteUseId
                                                               select new
                                                               {
                                                                   Id = con.Id,
                                                                   CustomerNum = cust.CustomerNum,
                                                                   EmailAddress = con.EmailAddress,
                                                                   LegalEntity = con.LegalEntity,
                                                                   Name = con.Name,
                                                                   SiteUseId = con.SiteUseId
                                                               })
                                                            on new { invGrp.CustomerNum, invGrp.LegalEntity, invGrp.SiteUseId } equals new { cont.CustomerNum, cont.LegalEntity, cont.SiteUseId }
                                                            into conts
                                                          from contss in conts.DefaultIfEmpty()
                                                          //join custorder in assType on new { CustomerNum = contss.CustomerNum, SiteUseId = contss.SiteUseId } equals new { CustomerNum = custorder.CustomerNum, SiteUseId = custorder.SiteUseId }
                                                          //into custorders
                                                          //from custorderss in custorders.DefaultIfEmpty()
                                                          join invsums in invSum on new { LegalEntity = invGrp.LegalEntity, CustomerNum = invGrp.CustomerNum, SiteUseId = invGrp.SiteUseId } equals new { LegalEntity = invsums.LegalEntity, CustomerNum = invsums.CustomerNum, SiteUseId = invsums.SiteUseId }
                                                          into invsumss
                                                          from invsumsss in invsumss.DefaultIfEmpty()
                                                          select new
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
                                                              //OverDue90Amt = invGrp.OverDue90Amt,
                                                              //PtpAmt = invGrp.PtpAmt,
                                                              //BrokenPTPAmt = invGrp.BrokenPTPAmt,
                                                              //DisputeAmt = invGrp.DisputeAmt,
                                                              //SpecialNotes = notetmp.SpecialNotes,
                                                              //SoaDate = alert1tmp.Status1,
                                                              //SecondDate = alert23tmp.Status2,
                                                              //FinalDate = alert23tmp.Status3,
                                                              //AdjustedOver90 = invGrp.AdjustedOver90,
                                                              PaymentTerm = aging.CreditTrem,
                                                              Collector = invGrp.Collector,
                                                              Team = invGrp.Team,
                                                              Country = aging.CountryCode,
                                                              ContactList = from c in conts
                                                                            select string.Concat(c.EmailAddress, "(", c.Name, ")"),
                                                              //Start add by xuan.wu for Arrow adding
                                                              SiteUseId = invGrp.SiteUseId,
                                                              COLLECTOR_CONTACT = invGrp.COLLECTOR_CONTACT,
                                                              Sales = aging.Sales,
                                                              CS = invGrp.CS,
                                                              CreditLimit = invGrp.CREDIT_LIMIT,
                                                              CreditTremDescription = invGrp.CreditTremDescription,
                                                              TotalFutureDue = aging.TotalFutureDue,
                                                              //DunningPirority = custorderss != null ? custorderss.DunningPirority : 3,
                                                              PastDueAmount = invsumsss.PastDueAmount,
                                                              Comment = aging.Comments
                                                              //End add by xuan.wu for Arrow adding
                                                          }

                                                          into querybydunpirority
                                                          //orderby querybydunpirority.DunningPirority
                                                          select new AllAccountInfo
                                                          {
                                                              Id = querybydunpirority.Id,
                                                              LegalEntity = querybydunpirority.LegalEntity,
                                                              CustomerNum = querybydunpirority.CustomerNum,
                                                              CustomerName = querybydunpirority.CustomerName,
                                                              BillGroupCode = querybydunpirority.BillGroupCode,
                                                              BillGroupName = querybydunpirority.BillGroupName,
                                                              Class = querybydunpirority.Class,
                                                              ArBalanceAmtPeroid = querybydunpirority.ArBalanceAmtPeroid,
                                                              BalanceAmt = querybydunpirority.BalanceAmt,
                                                              //OverDue90Amt = querybydunpirority.OverDue90Amt,
                                                              //PtpAmt = querybydunpirority.PtpAmt,
                                                              //BrokenPTPAmt = querybydunpirority.BrokenPTPAmt,
                                                              //DisputeAmt = querybydunpirority.DisputeAmt,
                                                              //SpecialNotes = querybydunpirority.SpecialNotes,
                                                              //SoaDate = querybydunpirority.SoaDate,
                                                              //SecondDate = querybydunpirority.SecondDate,
                                                              //FinalDate = querybydunpirority.FinalDate,
                                                              //AdjustedOver90 = querybydunpirority.AdjustedOver90,
                                                              PaymentTerm = querybydunpirority.PaymentTerm,
                                                              Collector = querybydunpirority.Collector,
                                                              Team = querybydunpirority.Team,
                                                              Country = querybydunpirority.Country,
                                                              ContactList = querybydunpirority.ContactList,
                                                              //Start add by xuan.wu for Arrow adding
                                                              SiteUseId = querybydunpirority.SiteUseId,
                                                              COLLECTOR_CONTACT = querybydunpirority.COLLECTOR_CONTACT,
                                                              Sales = querybydunpirority.Sales,
                                                              CS = querybydunpirority.CS,
                                                              CreditLimit = querybydunpirority.CreditLimit,
                                                              CreditTremDescription = querybydunpirority.CreditTremDescription,
                                                              TotalFutureDue = querybydunpirority.TotalFutureDue,
                                                              PastDueAmount = querybydunpirority.PastDueAmount,
                                                              //End add by xuan.wu for Arrow adding

                                                              AccountPtpAmount = accPtpAmtList.FirstOrDefault(x => x.CustomerNum == querybydunpirority.CustomerNum && x.SiteUseId == querybydunpirority.SiteUseId).AccountPtpAmount,
                                                              Comment = querybydunpirority.Comment
                                                          };

                    DateTime dtToday = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
                    if (isPTPOverDue.Equals("true"))
                    {
                        invsList = from invs in invsList
                                   where ptpPaymentList.Any(t => t.CustomerNum == invs.CustomerNum && t.SiteUseId == invs.SiteUseId
                                   && t.PromiseDate.Value < dtToday && t.PTPStatus == "001" && t.PTPPaymentType == "PTP")
                                   select invs;
                    }
                    Helper.Log.Info("All Account End!");
                    CommonRep.GetDBContext().Database.ExecuteSqlCommand("TRUNCATE TABLE dbo.T_ALLACCOUNT_TMP");
                    List<T_ALLACCOUNT_TMP> allAccountTmp = new List<T_ALLACCOUNT_TMP>();
                    List<AllAccountInfo> list = invsList.ToList();
                    foreach (AllAccountInfo item in list)
                    {
                        T_ALLACCOUNT_TMP newItem = new T_ALLACCOUNT_TMP();
                        newItem.LegalEntity = item.LegalEntity;
                        newItem.CustomerNum = item.CustomerNum;
                        newItem.CustomerName = item.CustomerName;
                        newItem.BillGroupCode = item.BillGroupCode;
                        newItem.BillGroupName = item.BillGroupName;
                        newItem.Class = item.Class;
                        newItem.ArBalanceAmtPeroid = item.ArBalanceAmtPeroid;
                        newItem.BalanceAmt = item.BalanceAmt;
                        newItem.PaidAmt = item.PaidAmt;
                        newItem.OverDue90Amt = item.OverDue90Amt;
                        newItem.PtpAmt = item.PtpAmt;
                        newItem.BrokenPTPAmt = item.BrokenPTPAmt;
                        newItem.DisputeAmt = item.DisputeAmt;
                        newItem.UnapplidPayment = item.UnapplidPayment;
                        newItem.DueDate = item.DueDate;
                        newItem.States = item.States;
                        newItem.CreateDate = item.CreateDate;
                        newItem.PaymentTerm = item.PaymentTerm;
                        newItem.SoaDate = item.SoaDate;
                        newItem.SecondDate = item.SecondDate;
                        newItem.FinalDate = item.FinalDate;
                        newItem.SpecialNotes = item.SpecialNotes;
                        newItem.AdjustedOver90 = item.AdjustedOver90;
                        newItem.Collector = item.Collector;
                        newItem.Team = item.Team;
                        newItem.SiteUseId = item.SiteUseId;
                        newItem.COLLECTOR_CONTACT = item.COLLECTOR_CONTACT;

                        newItem.Country = item.Country;
                        newItem.CS = item.CS;

                        newItem.Sales = item.Sales;
                        newItem.CreditLimit = item.CreditLimit;
                        newItem.CreditTremDescription = item.CreditTremDescription;
                        newItem.TotalFutureDue = item.TotalFutureDue;
                        newItem.PastDueAmount = item.PastDueAmount;
                        newItem.FinishedStatus = item.FinishedStatus;
                        newItem.Comment = item.Comment;
                        newItem.AccountPtpAmount = item.AccountPtpAmount;
                        if (allAccountTmp.Find(o => o.SiteUseId == newItem.SiteUseId) == null)
                        {
                            allAccountTmp.Add(newItem);
                        }
                    }
                    try
                    {
                        CommonRep.BulkInsert(allAccountTmp);
                        CommonRep.Commit();
                    }
                    catch (Exception ex) {
                        Helper.Log.Error(ex.Message, ex);
                        Helper.Log.Error(ex.InnerException.Message, ex.InnerException);
                        throw new Exception(ex.Message);
                    }

                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                Helper.Log.Error(ex.InnerException.Message, ex.InnerException);
                throw new Exception(ex.Message);
            }

        }

    }
}
