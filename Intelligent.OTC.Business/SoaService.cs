using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.Repository;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.Domain.Repositories;
using log4net.Repository.Hierarchy;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.Linq.SqlClient;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;

namespace Intelligent.OTC.Business
{
    public class SoaService : ISoaService
    {
        public OTCRepository CommonRep { get; set; }
        public XcceleratorService XccService { get; set; }
        private string IsWF = ConfigurationManager.AppSettings["IsWF"].ToString();

        public int CurrentPeriod
        {
            get
            {
                return CommonRep.GetDbSet<PeriodControl>()
                    .Where(o => o.Deal == AppContext.Current.User.Deal).Max(o => o.Id);
            }
        }
        public DateTime CurrentTime
        {
            get
            {
                return AppContext.Current.User.Now;
            }
        }
        public IEnumerable<SoaDto> GetSoaList(string invoiceState = "", string invoiceTrackState = "", string legalEntity = "", string invoiceNum = "", string soNum = "", string poNum = "", string invoiceMemo = "", string customerNum = "", string customerName = "", string customerClass = "", string siteUseId = "", string EB = "")
        {
            IQueryable<SoaDto> r = null;

            List<SysUser> listUser = new List<SysUser>();
            listUser = XccService.GetUserTeamList(AppContext.Current.User.EID);
            string[] userGroup = listUser.Select(t => t.EID).Distinct().ToArray();
            string collecotrList = "," + string.Join(",", userGroup.ToArray()) + ",";

            var soas = CommonRep.GetDbSet<CollectorAlert>().Where(o => o.Status != "Finish" && o.Status != "Cancelled" && (o.AlertType == 1));
            if (AppContext.Current.User.ActionPermissions.IndexOf("alldataforsupervisor") >= 0)
            {
            }
            else
            {
                soas = soas.Where(o => collecotrList.Contains("," + o.Eid + ","));
            }
            if (string.IsNullOrEmpty(invoiceState) && string.IsNullOrEmpty(invoiceTrackState) && string.IsNullOrEmpty(invoiceNum) && string.IsNullOrEmpty(soNum) && string.IsNullOrEmpty(poNum) && string.IsNullOrEmpty(invoiceMemo) && string.IsNullOrEmpty(customerNum) && string.IsNullOrEmpty(customerName) && string.IsNullOrEmpty(customerClass) && string.IsNullOrEmpty(siteUseId) && string.IsNullOrEmpty(EB))
            {
                #region Without InvoiceState and InvoiceTrackState
                var assType = from x in CommonRep.GetDbSet<T_CustomerAssessment>()
                              join y in CommonRep.GetDbSet<T_AssessmentType>()
                              on x.AssessmentType equals y.Id
                              into xy
                              from y in xy.DefaultIfEmpty()
                              select new { CustomerNum = x.CustomerId, DunningPirority = y.DunningPirority, SiteUseId = x.SiteUseId };

                // base table: dunning
                r = (from final in
                         (from res in
                              (from soa in soas
                                   // left join customer level
                               join cust in SpringFactory.GetObjectImpl<CustomerService>("CustomerService").GetCustomerLevel(CurrentPeriod) on new { soa.CustomerNum, soa.Deal, soa.SiteUseId } equals new { cust.CustomerNum, cust.Deal, cust.SiteUseId }
                                  into custs
                               from cust in custs.DefaultIfEmpty()
                               join age in CommonRep.GetQueryable<CustomerAging>().Where(o => o.Deal == AppContext.Current.User.Deal) on new { cust.CustomerNum, cust.Deal, cust.SiteUseId } equals new { age.CustomerNum, age.Deal, age.SiteUseId }
                                  into ages
                               from age in ages.DefaultIfEmpty()
                               join custorder in assType on new { CustomerNum = age.CustomerNum, SiteUseId = age.SiteUseId } equals new { CustomerNum = custorder.CustomerNum, SiteUseId = custorder.SiteUseId }
                               into custorders
                               from custorderss in custorders.DefaultIfEmpty()
                               join ccv in CommonRep.GetQueryable<CustomerContactorView>() on new { Deal = cust.Deal, CustomerNum = cust.CustomerNum, SiteUseId = cust.SiteUseId } equals new { Deal = ccv.Deal, CustomerNum = ccv.CustomerNum, SiteUseId = ccv.SiteUseId }
                               into ccvs
                               from ccvsss in ccvs.DefaultIfEmpty()
                               join duncount in CommonRep.GetQueryable<V_DUN_COUNT>() on new { CustomerNum = cust.CustomerNum, SiteUseId = cust.SiteUseId, LegalEntity = age.LegalEntity } equals new { CustomerNum = duncount.CUSTOMER_NUM, SiteUseId = duncount.SiteUseId, LegalEntity = duncount.LEGAL_ENTITY }
                               into ccdu
                               from ccvsst in ccdu.DefaultIfEmpty()
                               select new
                               {
                                   Id = soa.Id,
                                   ActionDate = soa.ActionDate,
                                   Deal = cust.Deal,
                                   TaskId = soa.TaskId,
                                   ReferenceNo = soa.ReferenceNo,
                                   ProcessId = soa.ProcessId,
                                   SoaStatus = soa.Status,
                                   CauseObjectNumber = soa.CauseObjectNumber,
                                   BatchType = soa.BatchType,
                                   FailedReason = soa.FailedReason,
                                   PeriodId = soa.PeriodId,
                                   AlertType = soa.AlertType,
                                   CustomerNum = cust.CustomerNum,
                                   CustomerName = cust.CustomerName,
                                   BillGroupCode = cust.BillGroupCode,
                                   BillGroupName = cust.BillGroupName,
                                   Operator = cust.Collector,
                                   CreditLimit = age == null ? 0 : age.CreditLimit,
                                   TotalAmt = age == null ? 0 : age.TotalAmt,
                                   CurrentAmt = age == null ? 0 : age.CurrentAmt,
                                   FDueOver90Amt = age == null ? 0 : (age.Due120Amt + age.Due150Amt + age.Due180Amt + age.Due210Amt + age.Due240Amt + age.Due270Amt + age.Due300Amt + age.Due330Amt + age.Due360Amt + age.DueOver360Amt),
                                   PastDueAmt = age == null ? 0 : age.DueoverTotalAmt,
                                   Risk = cust.Risk,
                                   Value = cust.Value,
                                   Class = cust.Class,
                                   SiteUseId = age.SiteUseId,
                                   CreditTrem = age.CreditTrem,
                                   CollectorName = ccvsss.Contactor,
                                   Sales = age.Sales,
                                   Due15Amt = age.DUE15_AMT,
                                   Due30Amt = age.Due30Amt,
                                   Due45Amt = age.DUE45_AMT,
                                   Due60Amt = age.Due60Amt,
                                   Due90Amt = age.Due90Amt,
                                   Due120Amt = age.Due120Amt,
                                   TotalFutureDue = age.TotalFutureDue,
                                   CS = cust.CS,
                                   overDueAMT = age.DueoverTotalAmt,
                                   arBalance = age.TotalAmt,
                                   DunningPirority = custorderss != null ? custorderss.DunningPirority : 3,
                                   PTP_1 = ccvsst != null ? ccvsst.PTP_1 : 0,
                                   PTP_2 = ccvsst != null ? ccvsst.PTP_2 : 0,
                                   Remindering = ccvsst != null ? ccvsst.REMINDING : 0,
                                   Dunning_1 = ccvsst != null ? ccvsst.Dunning_1 : 0,
                                   Dunning_2 = ccvsst != null ? ccvsst.Dunning_2 : 0,
                                   EB = age.Ebname
                               }
                               into querybydunpirority
                               orderby querybydunpirority.DunningPirority
                               select new
                               {
                                   Id = querybydunpirority.Id,
                                   ActionDate = querybydunpirority.ActionDate,
                                   Deal = querybydunpirority.Deal,
                                   TaskId = querybydunpirority.TaskId,
                                   ReferenceNo = querybydunpirority.ReferenceNo,
                                   ProcessId = querybydunpirority.ProcessId,
                                   SoaStatus = querybydunpirority.SoaStatus,
                                   CauseObjectNumber = querybydunpirority.CauseObjectNumber,
                                   BatchType = querybydunpirority.BatchType,
                                   FailedReason = querybydunpirority.FailedReason,
                                   PeriodId = querybydunpirority.PeriodId,
                                   AlertType = querybydunpirority.AlertType,
                                   CustomerNum = querybydunpirority.CustomerNum,
                                   CustomerName = querybydunpirority.CustomerName,
                                   BillGroupCode = querybydunpirority.BillGroupCode,
                                   BillGroupName = querybydunpirority.BillGroupName,
                                   Operator = querybydunpirority.Operator,
                                   CreditLimit = querybydunpirority.CreditLimit,
                                   TotalAmt = querybydunpirority.TotalAmt,
                                   CurrentAmt = querybydunpirority.CurrentAmt,
                                   FDueOver90Amt = querybydunpirority.FDueOver90Amt,
                                   PastDueAmt = querybydunpirority.PastDueAmt,
                                   Risk = querybydunpirority.Risk,
                                   Value = querybydunpirority.Value,
                                   Class = querybydunpirority.Class,
                                   SiteUseId = querybydunpirority.SiteUseId,
                                   CreditTrem = querybydunpirority.CreditTrem,
                                   CollectorName = querybydunpirority.CollectorName,
                                   Sales = querybydunpirority.Sales,
                                   Due15Amt = querybydunpirority.Due15Amt,
                                   Due30Amt = querybydunpirority.Due30Amt,
                                   Due45Amt = querybydunpirority.Due45Amt,
                                   Due60Amt = querybydunpirority.Due60Amt,
                                   Due90Amt = querybydunpirority.Due90Amt,
                                   Due120Amt = querybydunpirority.Due120Amt,
                                   TotalFutureDue = querybydunpirority.TotalFutureDue,
                                   CS = querybydunpirority.CS,
                                   overDueAMT = querybydunpirority.overDueAMT,
                                   arBalance = querybydunpirority.arBalance,
                                   PTP_1 = querybydunpirority.PTP_1,
                                   PTP_2 = querybydunpirority.PTP_2,
                                   Remindering = querybydunpirority.Remindering,
                                   Dunning_1 = querybydunpirority.Dunning_1,
                                   Dunning_2 = querybydunpirority.Dunning_2,
                                   EB = querybydunpirority.EB
                               })
                          group res by new
                          {
                              res.Id,
                              res.ActionDate,
                              res.Deal,
                              res.TaskId,
                              res.ReferenceNo,
                              res.ProcessId,
                              res.SoaStatus,
                              res.CauseObjectNumber,
                              res.BatchType,
                              res.FailedReason,
                              res.PeriodId,
                              res.AlertType,
                              res.CustomerNum,
                              res.CustomerName,
                              res.BillGroupCode,
                              res.BillGroupName,
                              res.Operator,
                              res.Risk,
                              res.Value,
                              res.Class,
                              res.SiteUseId,
                              res.CreditTrem,
                              res.CollectorName,
                              res.Sales,
                              res.Due15Amt,
                              res.Due30Amt,
                              res.Due45Amt,
                              res.Due60Amt,
                              res.Due90Amt,
                              res.Due120Amt,
                              res.TotalFutureDue,
                              res.CS,
                              res.overDueAMT,
                              res.arBalance,
                              res.PTP_1,
                              res.PTP_2,
                              res.Remindering,
                              res.Dunning_1,
                              res.Dunning_2,
                              res.EB
                          }
                              into reses
                          select new
                          {
                              Id = reses.Key.Id,
                              ActionDate = reses.Key.ActionDate,
                              Deal = reses.Key.Deal,
                              TaskId = reses.Key.TaskId,
                              ReferenceNo = reses.Key.ReferenceNo,
                              ProcessId = reses.Key.ProcessId,
                              SoaStatus = reses.Key.SoaStatus,
                              CauseObjectNumber = reses.Key.CauseObjectNumber,
                              BatchType = reses.Key.BatchType,
                              FailedReason = reses.Key.FailedReason,
                              PeriodId = reses.Key.PeriodId,
                              AlertType = reses.Key.AlertType,
                              CustomerNum = reses.Key.CustomerNum,
                              CustomerName = reses.Key.CustomerName,
                              BillGroupCode = reses.Key.BillGroupCode,
                              BillGroupName = reses.Key.BillGroupName,
                              Class = reses.Key.Class,
                              Risk = reses.Key.Risk,
                              CreditLimit = reses.Sum(age => age.CreditLimit),
                              TotalAmt = reses.Sum(age => age.TotalAmt),
                              CurrentAmt = reses.Sum(age => age.CurrentAmt),
                              FDueOver90Amt = reses.Sum(age => age.FDueOver90Amt),
                              PastDueAmt = reses.Sum(age => age.PastDueAmt),
                              Operator = reses.Key.Operator,
                              SiteUseId = reses.Key.SiteUseId,
                              CreditTrem = reses.Key.CreditTrem,
                              CollectorName = reses.Key.CollectorName,
                              Sales = reses.Key.Sales,
                              Due15Amt = reses.Key.Due15Amt,
                              Due30Amt = reses.Key.Due30Amt,
                              Due45Amt = reses.Key.Due45Amt,
                              Due60Amt = reses.Key.Due60Amt,
                              Due90Amt = reses.Key.Due90Amt,
                              Due120Amt = reses.Key.Due120Amt,
                              TotalFutureDue = reses.Key.TotalFutureDue,
                              CS = reses.Key.CS,
                              PTP_1 = reses.Key.PTP_1,
                              PTP_2 = reses.Key.PTP_2,
                              Remindering = reses.Key.Remindering,
                              Dunning_1 = reses.Key.Dunning_1,
                              Dunning_2 = reses.Key.Dunning_2,
                              overDueAMT = reses.Key.overDueAMT,
                              arBalance = reses.Key.arBalance,
                              EB = reses.Key.EB
                          })
                     join aging in CommonRep.GetDbSet<CustomerAging>().Where(o => o.Deal == AppContext.Current.User.Deal) on new { final.CustomerNum, final.Deal, final.SiteUseId } equals new { aging.CustomerNum, aging.Deal, aging.SiteUseId }
                        into agings
                     where agings.Any(age => age.LegalEntity == legalEntity || string.IsNullOrEmpty(legalEntity))
                     select new SoaDto
                     {
                         Id = final.Id,
                         ActionDate = final.ActionDate,
                         Deal = final.Deal,
                         TaskId = final.TaskId,
                         ReferenceNo = final.ReferenceNo,
                         ProcessId = final.ProcessId,
                         SoaStatus = final.SoaStatus,
                         CauseObjectNumber = final.CauseObjectNumber,
                         BatchType = final.BatchType,
                         FailedReason = final.FailedReason,
                         PeriodId = final.PeriodId,
                         AlertType = final.AlertType,
                         CustomerNum = final.CustomerNum,
                         CustomerName = final.CustomerName,
                         BillGroupCode = string.IsNullOrEmpty(final.BillGroupCode) == true ? final.CustomerName : final.BillGroupCode,
                         BillGroupName = string.IsNullOrEmpty(final.BillGroupName) == true ? final.CustomerName : final.BillGroupName,
                         CreditLimit = final.CreditLimit,
                         TotalAmt = final.TotalAmt,
                         CurrentAmt = final.CurrentAmt,
                         FDueOver90Amt = final.FDueOver90Amt,
                         PastDueAmt = final.PastDueAmt,
                         LegalEntityList = from l in agings
                                           select l.LegalEntity,
                         Class = final.Class,
                         Risk = final.Risk,
                         Operator = final.Operator,
                         SiteUseId = final.SiteUseId,
                         CreditTrem = final.CreditTrem,
                         CollectorName = final.CollectorName,
                         Sales = final.Sales,
                         Due15Amt = final.Due15Amt,
                         Due30Amt = final.Due30Amt,
                         Due45Amt = final.Due45Amt,
                         Due60Amt = final.Due60Amt,
                         Due90Amt = final.Due90Amt,
                         Due120Amt = final.Due120Amt,
                         TotalFutureDue = final.TotalFutureDue,
                         CS = final.CS,
                         PTP_1 = final.PTP_1,
                         PTP_2 = final.PTP_2,
                         Remindering = final.Remindering,
                         Dunning_1 = final.Dunning_1,
                         Dunning_2 = final.Dunning_2,
                         overDueAMT = final.overDueAMT,
                         arBalance = final.arBalance,
                         EB = final.EB
                     });
                #endregion
            }
            else
            {
                #region With InvoiceState Or InvoiceTrackState
                // base table: dunning
                var assType = from x in CommonRep.GetDbSet<T_CustomerAssessment>()
                              join y in CommonRep.GetDbSet<T_AssessmentType>()
                              on x.AssessmentType equals y.Id
                              into xy
                              from y in xy.DefaultIfEmpty()
                              select new { CustomerNum = x.CustomerId, DunningPirority = y.DunningPirority, SiteUseId = x.SiteUseId };
                r = (from final in
                         (from res in
                              (from soa in soas
                                   // left join customer level
                               join cust in SpringFactory.GetObjectImpl<CustomerService>("CustomerService").GetCustomerLevel(CurrentPeriod) on new { soa.CustomerNum, soa.Deal, soa.SiteUseId } equals new { cust.CustomerNum, cust.Deal, cust.SiteUseId }
                                  into custs
                               from cust in custs.DefaultIfEmpty()
                               where (cust.CustomerNum.IndexOf(customerNum) >= 0 || string.IsNullOrEmpty(customerNum))
                               && (cust.CustomerName.IndexOf(customerName) >= 0 || string.IsNullOrEmpty(customerName))
                               && (cust.Class == customerClass || string.IsNullOrEmpty(customerClass))
                               && (cust.SiteUseId.IndexOf(siteUseId) >= 0 || string.IsNullOrEmpty(siteUseId))
                               // left join aging
                               join age in CommonRep.GetQueryable<CustomerAging>().Where(o => o.Deal == AppContext.Current.User.Deal) on new { cust.CustomerNum, cust.Deal, cust.SiteUseId } equals new { age.CustomerNum, age.Deal, age.SiteUseId }
                                  into ages
                               from age in ages.DefaultIfEmpty()
                               where (age.Ebname == EB || string.IsNullOrEmpty(EB))
                               join inv in CommonRep.GetDbSet<InvoiceAging>()
                               on new { cust.CustomerNum, cust.Deal, cust.SiteUseId } equals new { inv.CustomerNum, inv.Deal, inv.SiteUseId }
                               into invs
                               where invs.Any(
                                            i => (i.States == invoiceState || string.IsNullOrEmpty(invoiceState))
                                         && (i.TrackStates == invoiceTrackState || string.IsNullOrEmpty(invoiceTrackState))
                                         && (i.InvoiceNum.IndexOf(invoiceNum) >= 0 || string.IsNullOrEmpty(invoiceNum))
                                         && (i.SoNum.IndexOf(soNum) >= 0 || string.IsNullOrEmpty(soNum))
                                         && (i.PoNum.IndexOf(poNum) >= 0 || string.IsNullOrEmpty(poNum))
                                         && (i.Comments.IndexOf(invoiceMemo) >= 0 || string.IsNullOrEmpty(invoiceMemo))
                                         )
                               from grp in invs.DefaultIfEmpty()
                               join custorder in assType on new { CustomerNum = grp.CustomerNum, SiteUseId = grp.SiteUseId } equals new { CustomerNum = custorder.CustomerNum, SiteUseId = custorder.SiteUseId }
                                into custorders

                               from custorderss in custorders.DefaultIfEmpty()
                               join duncount in CommonRep.GetQueryable<V_DUN_COUNT>() on new { cust.CustomerNum, cust.SiteUseId } equals new { CustomerNum = duncount.CUSTOMER_NUM, duncount.SiteUseId }
                               into ccdu
                               from ccvsst in ccdu.DefaultIfEmpty()
                               select new
                               {
                                   Id = soa.Id,
                                   ActionDate = soa.ActionDate,
                                   Deal = cust.Deal,
                                   TaskId = soa.TaskId,
                                   ReferenceNo = soa.ReferenceNo,
                                   ProcessId = soa.ProcessId,
                                   SoaStatus = soa.Status,
                                   CauseObjectNumber = soa.CauseObjectNumber,
                                   BatchType = soa.BatchType,
                                   FailedReason = soa.FailedReason,
                                   PeriodId = soa.PeriodId,
                                   AlertType = soa.AlertType,
                                   CustomerNum = cust.CustomerNum,
                                   CustomerName = cust.CustomerName,
                                   BillGroupCode = cust.BillGroupCode,
                                   BillGroupName = cust.BillGroupName,
                                   Operator = cust.Collector,
                                   CreditLimit = age == null ? 0 : age.CreditLimit,
                                   TotalAmt = age == null ? 0 : age.TotalAmt,
                                   CurrentAmt = age == null ? 0 : age.CurrentAmt,
                                   FDueOver90Amt = age == null ? 0 : (age.Due120Amt + age.Due150Amt + age.Due180Amt + age.Due210Amt + age.Due240Amt + age.Due270Amt + age.Due300Amt + age.Due330Amt + age.Due360Amt + age.DueOver360Amt),
                                   PastDueAmt = age == null ? 0 : age.DueoverTotalAmt,
                                   Risk = cust.Risk,
                                   Value = cust.Value,
                                   Class = cust.Class,
                                   SiteUseId = age.SiteUseId,
                                   CreditTrem = age.CreditTrem,
                                   CollectorName = age.Collector,
                                   Sales = age.Sales,
                                   Due15Amt = age.DUE15_AMT,
                                   Due30Amt = age.Due30Amt,
                                   Due45Amt = age.DUE45_AMT,
                                   Due60Amt = age.Due60Amt,
                                   Due90Amt = age.Due90Amt,
                                   Due120Amt = age.Due120Amt,
                                   TotalFutureDue = age.TotalFutureDue,
                                   CS = cust.CS,
                                   overDueAMT = age.DueoverTotalAmt,
                                   arBalance = age.TotalAmt,
                                   DunningPirority = custorderss != null ? custorderss.DunningPirority : 3,
                                   PTP_1 = ccvsst != null ? ccvsst.PTP_1 : 0,
                                   PTP_2 = ccvsst != null ? ccvsst.PTP_2 : 0,
                                   Remindering = ccvsst != null ? ccvsst.REMINDING : 0,
                                   Dunning_1 = ccvsst != null ? ccvsst.Dunning_1 : 0,
                                   Dunning_2 = ccvsst != null ? ccvsst.Dunning_2 : 0,
                                   EB = age.Ebname
                               }
                               into querybydunpirority
                               orderby querybydunpirority.DunningPirority
                               select new
                               {
                                   Id = querybydunpirority.Id,
                                   ActionDate = querybydunpirority.ActionDate,
                                   Deal = querybydunpirority.Deal,
                                   TaskId = querybydunpirority.TaskId,
                                   ReferenceNo = querybydunpirority.ReferenceNo,
                                   ProcessId = querybydunpirority.ProcessId,
                                   SoaStatus = querybydunpirority.SoaStatus,
                                   CauseObjectNumber = querybydunpirority.CauseObjectNumber,
                                   BatchType = querybydunpirority.BatchType,
                                   FailedReason = querybydunpirority.FailedReason,
                                   PeriodId = querybydunpirority.PeriodId,
                                   AlertType = querybydunpirority.AlertType,
                                   CustomerNum = querybydunpirority.CustomerNum,
                                   CustomerName = querybydunpirority.CustomerName,
                                   BillGroupCode = querybydunpirority.BillGroupCode,
                                   BillGroupName = querybydunpirority.BillGroupName,
                                   Operator = querybydunpirority.Operator,
                                   CreditLimit = querybydunpirority.CreditLimit,
                                   TotalAmt = querybydunpirority.TotalAmt,
                                   CurrentAmt = querybydunpirority.CurrentAmt,
                                   FDueOver90Amt = querybydunpirority.FDueOver90Amt,
                                   PastDueAmt = querybydunpirority.PastDueAmt,
                                   Risk = querybydunpirority.Risk,
                                   Value = querybydunpirority.Value,
                                   Class = querybydunpirority.Class,
                                   SiteUseId = querybydunpirority.SiteUseId,
                                   CreditTrem = querybydunpirority.CreditTrem,
                                   CollectorName = querybydunpirority.CollectorName,
                                   Sales = querybydunpirority.Sales,
                                   Due15Amt = querybydunpirority.Due15Amt,
                                   Due30Amt = querybydunpirority.Due30Amt,
                                   Due45Amt = querybydunpirority.Due45Amt,
                                   Due60Amt = querybydunpirority.Due60Amt,
                                   Due90Amt = querybydunpirority.Due90Amt,
                                   Due120Amt = querybydunpirority.Due120Amt,
                                   TotalFutureDue = querybydunpirority.TotalFutureDue,
                                   CS = querybydunpirority.CS,
                                   overDueAMT = querybydunpirority.overDueAMT,
                                   arBalance = querybydunpirority.arBalance,
                                   PTP_1 = querybydunpirority.PTP_1,
                                   PTP_2 = querybydunpirority.PTP_2,
                                   Remindering = querybydunpirority.Remindering,
                                   Dunning_1 = querybydunpirority.Dunning_1,
                                   Dunning_2 = querybydunpirority.Dunning_2,
                                   EB = querybydunpirority.EB
                               })
                          group res by new
                          {
                              res.Id,
                              res.ActionDate,
                              res.Deal,
                              res.TaskId,
                              res.ReferenceNo,
                              res.ProcessId,
                              res.SoaStatus,
                              res.CauseObjectNumber,
                              res.BatchType,
                              res.FailedReason,
                              res.PeriodId,
                              res.AlertType,
                              res.CustomerNum,
                              res.CustomerName,
                              res.BillGroupCode,
                              res.BillGroupName,
                              res.Operator,
                              res.Risk,
                              res.Value,
                              res.Class,
                              res.SiteUseId,
                              res.CreditTrem,
                              res.CollectorName,
                              res.Sales,
                              res.Due15Amt,
                              res.Due30Amt,
                              res.Due45Amt,
                              res.Due60Amt,
                              res.Due90Amt,
                              res.Due120Amt,
                              res.TotalFutureDue,
                              res.CS,
                              res.overDueAMT,
                              res.arBalance,
                              res.PTP_1,
                              res.PTP_2,
                              res.Remindering,
                              res.Dunning_1,
                              res.Dunning_2,
                              res.EB
                          }
                              into reses
                          select new
                          {
                              Id = reses.Key.Id,
                              ActionDate = reses.Key.ActionDate,
                              Deal = reses.Key.Deal,
                              TaskId = reses.Key.TaskId,
                              ReferenceNo = reses.Key.ReferenceNo,
                              ProcessId = reses.Key.ProcessId,
                              SoaStatus = reses.Key.SoaStatus,
                              CauseObjectNumber = reses.Key.CauseObjectNumber,
                              BatchType = reses.Key.BatchType,
                              FailedReason = reses.Key.FailedReason,
                              PeriodId = reses.Key.PeriodId,
                              AlertType = reses.Key.AlertType,
                              CustomerNum = reses.Key.CustomerNum,
                              CustomerName = reses.Key.CustomerName,
                              BillGroupCode = reses.Key.BillGroupCode,
                              BillGroupName = reses.Key.BillGroupName,
                              Class = reses.Key.Class,
                              Risk = reses.Key.Risk,
                              CreditLimit = reses.Sum(age => age.CreditLimit),
                              TotalAmt = reses.Sum(age => age.TotalAmt),
                              CurrentAmt = reses.Sum(age => age.CurrentAmt),
                              FDueOver90Amt = reses.Sum(age => age.FDueOver90Amt),
                              PastDueAmt = reses.Sum(age => age.PastDueAmt),
                              Operator = reses.Key.Operator,
                              SiteUseId = reses.Key.SiteUseId,
                              CreditTrem = reses.Key.CreditTrem,
                              CollectorName = reses.Key.CollectorName,
                              Sales = reses.Key.Sales,
                              Due15Amt = reses.Key.Due15Amt,
                              Due30Amt = reses.Key.Due30Amt,
                              Due45Amt = reses.Key.Due45Amt,
                              Due60Amt = reses.Key.Due60Amt,
                              Due90Amt = reses.Key.Due90Amt,
                              Due120Amt = reses.Key.Due120Amt,
                              TotalFutureDue = reses.Key.TotalFutureDue,
                              CS = reses.Key.CS,
                              PTP_1 = reses.Key.PTP_1,
                              PTP_2 = reses.Key.PTP_2,
                              Remindering = reses.Key.Remindering,
                              Dunning_1 = reses.Key.Dunning_1,
                              Dunning_2 = reses.Key.Dunning_2,
                              overDueAMT = reses.Key.overDueAMT,
                              arBalance = reses.Key.arBalance,
                              EB = reses.Key.EB
                          })

                     join aging in CommonRep.GetDbSet<CustomerAging>().Where(o => o.Deal == AppContext.Current.User.Deal) on new { final.CustomerNum, final.Deal, final.SiteUseId } equals new { aging.CustomerNum, aging.Deal, aging.SiteUseId }
                        into agings
                     where agings.Any(age => age.LegalEntity == legalEntity || string.IsNullOrEmpty(legalEntity))
                     select new SoaDto
                     {
                         Id = final.Id,
                         ActionDate = final.ActionDate,
                         Deal = final.Deal,
                         TaskId = final.TaskId,
                         ReferenceNo = final.ReferenceNo,
                         ProcessId = final.ProcessId,
                         SoaStatus = final.SoaStatus,
                         CauseObjectNumber = final.CauseObjectNumber,
                         BatchType = final.BatchType,
                         FailedReason = final.FailedReason,
                         PeriodId = final.PeriodId,
                         AlertType = final.AlertType,
                         CustomerNum = final.CustomerNum,
                         CustomerName = final.CustomerName,
                         BillGroupCode = string.IsNullOrEmpty(final.BillGroupCode) == true ? final.CustomerName : final.BillGroupCode,
                         BillGroupName = string.IsNullOrEmpty(final.BillGroupName) == true ? final.CustomerName : final.BillGroupName,
                         CreditLimit = final.CreditLimit,
                         TotalAmt = final.TotalAmt,
                         CurrentAmt = final.CurrentAmt,
                         FDueOver90Amt = final.FDueOver90Amt,
                         PastDueAmt = final.PastDueAmt,
                         LegalEntityList = from l in agings
                                           select l.LegalEntity,
                         Class = final.Class,
                         Risk = final.Risk,
                         Operator = final.Operator,
                         SiteUseId = final.SiteUseId,
                         CreditTrem = final.CreditTrem,
                         CollectorName = final.CollectorName,
                         Sales = final.Sales,
                         Due15Amt = final.Due15Amt,
                         Due30Amt = final.Due30Amt,
                         Due45Amt = final.Due45Amt,
                         Due60Amt = final.Due60Amt,
                         Due90Amt = final.Due90Amt,
                         Due120Amt = final.Due120Amt,
                         TotalFutureDue = final.TotalFutureDue,
                         CS = final.CS,
                         PTP_1 = final.PTP_1,
                         PTP_2 = final.PTP_2,
                         Remindering = final.Remindering,
                         Dunning_1 = final.Dunning_1,
                         Dunning_2 = final.Dunning_2,
                         overDueAMT = final.overDueAMT,
                         arBalance = final.arBalance,
                         EB = final.EB
                     });
                #endregion
            }

            return r;
        }

        public IEnumerable<SoaDto> GetNoPaging(string ListType)
        {

            IQueryable<SoaDto> r = null;
            List<SysUser> listUser = new List<SysUser>();
            listUser = XccService.GetUserTeamList(AppContext.Current.User.EID);
            string[] userGroup = listUser.Select(t => t.EID).Distinct().ToArray();
            string collecotrList = "," + string.Join(",", userGroup.ToArray()) + ",";

            var soas = CommonRep.GetDbSet<CollectorAlert>().Where(o => o.Status == "Finish" && (o.AlertType == 1));
            if (AppContext.Current.User.ActionPermissions.IndexOf("alldataforsupervisor") >= 0)
            {
            }
            else
            {
                soas = soas.Where(o => collecotrList.Contains("," + o.Eid + ","));
            }

            if (ListType == "batch")
            {
                #region Without InvoiceState and InvoiceTrackState
                var assType = from x in CommonRep.GetDbSet<T_CustomerAssessment>()
                              join y in CommonRep.GetDbSet<T_AssessmentType>()
                              on x.AssessmentType equals y.Id
                              into xy
                              from y in xy.DefaultIfEmpty()
                              select new { CustomerNum = x.CustomerId, DunningPirority = y.DunningPirority, SiteUseId = x.SiteUseId };
                // base table: dunning
                r = (from final in
                         (from res in
                              (from soa in CommonRep.GetDbSet<CollectorAlert>()
                                   // left join customer level
                               join cust in SpringFactory.GetObjectImpl<CustomerService>("CustomerService").GetCustomerLevel(CurrentPeriod) on new { soa.CustomerNum, soa.Deal, soa.SiteUseId } equals new { cust.CustomerNum, cust.Deal, cust.SiteUseId }
                               where soa.Status == "Initialized" && (soa.AlertType == 1)
                               && soa.Eid == AppContext.Current.User.EID
                               join age in CommonRep.GetQueryable<CustomerAging>().Where(o => o.Deal == AppContext.Current.User.Deal) on new { cust.CustomerNum, cust.Deal, cust.SiteUseId } equals new { age.CustomerNum, age.Deal, age.SiteUseId }
                               join custorder in assType on new { CustomerNum = age.CustomerNum, SiteUseId = age.SiteUseId } equals new { CustomerNum = custorder.CustomerNum, SiteUseId = custorder.SiteUseId }
                               into custorders

                               from custorderss in custorders.DefaultIfEmpty()
                               join duncount in CommonRep.GetQueryable<V_DUN_COUNT>() on new { cust.CustomerNum, cust.SiteUseId } equals new { CustomerNum = duncount.CUSTOMER_NUM, duncount.SiteUseId }
                               into ccdu
                               from ccvsst in ccdu.DefaultIfEmpty()
                               select new
                               {
                                   Id = soa.Id,
                                   ActionDate = soa.ActionDate,
                                   Deal = cust.Deal,
                                   TaskId = soa.TaskId,
                                   ReferenceNo = soa.ReferenceNo,
                                   ProcessId = soa.ProcessId,
                                   SoaStatus = soa.Status,
                                   CauseObjectNumber = soa.CauseObjectNumber,
                                   BatchType = soa.BatchType,
                                   FailedReason = soa.FailedReason,
                                   PeriodId = soa.PeriodId,
                                   AlertType = soa.AlertType,
                                   CustomerNum = cust.CustomerNum,
                                   CustomerName = cust.CustomerName,
                                   BillGroupCode = cust.BillGroupCode,
                                   BillGroupName = cust.BillGroupName,
                                   Operator = cust.Collector,
                                   CreditLimit = age == null ? 0 : age.CreditLimit,
                                   TotalAmt = age == null ? 0 : age.TotalAmt,
                                   CurrentAmt = age == null ? 0 : age.CurrentAmt,
                                   FDueOver90Amt = age == null ? 0 : (age.Due120Amt + age.Due150Amt + age.Due180Amt + age.Due210Amt + age.Due240Amt + age.Due270Amt + age.Due300Amt + age.Due330Amt + age.Due360Amt + age.DueOver360Amt),
                                   PastDueAmt = age == null ? 0 : age.DueoverTotalAmt,
                                   Risk = cust.Risk,
                                   Value = cust.Value,
                                   Class = cust.Class,
                                   SiteUseId = age.SiteUseId,
                                   CreditTrem = age.CreditTrem,
                                   CollectorName = age.Collector,
                                   Sales = age.Sales,
                                   Due15Amt = age.DUE15_AMT,
                                   Due30Amt = age.Due30Amt,
                                   Due45Amt = age.DUE45_AMT,
                                   Due60Amt = age.Due60Amt,
                                   Due90Amt = age.Due90Amt,
                                   Due120Amt = age.Due120Amt,
                                   TotalFutureDue = age.TotalFutureDue,
                                   CS = cust.CS,
                                   overDueAMT = age.DueoverTotalAmt,
                                   arBalance = age.TotalAmt,
                                   DunningPirority = custorderss != null ? custorderss.DunningPirority : 3,
                                   PTP_1 = ccvsst != null ? ccvsst.PTP_1 : 0,
                                   PTP_2 = ccvsst != null ? ccvsst.PTP_2 : 0,
                                   Remindering = ccvsst != null ? ccvsst.REMINDING : 0,
                                   Dunning_1 = ccvsst != null ? ccvsst.Dunning_1 : 0,
                                   Dunning_2 = ccvsst != null ? ccvsst.Dunning_2 : 0
                               }
                               into lastquery
                               orderby lastquery.DunningPirority
                               select new
                               {
                                   Id = lastquery.Id,
                                   ActionDate = lastquery.ActionDate,
                                   Deal = lastquery.Deal,
                                   TaskId = lastquery.TaskId,
                                   ReferenceNo = lastquery.ReferenceNo,
                                   ProcessId = lastquery.ProcessId,
                                   SoaStatus = lastquery.SoaStatus,
                                   CauseObjectNumber = lastquery.CauseObjectNumber,
                                   BatchType = lastquery.BatchType,
                                   FailedReason = lastquery.FailedReason,
                                   PeriodId = lastquery.PeriodId,
                                   AlertType = lastquery.AlertType,
                                   CustomerNum = lastquery.CustomerNum,
                                   CustomerName = lastquery.CustomerName,
                                   BillGroupCode = lastquery.BillGroupCode,
                                   BillGroupName = lastquery.BillGroupName,
                                   Operator = lastquery.Operator,
                                   CreditLimit = lastquery.CreditLimit,
                                   TotalAmt = lastquery.TotalAmt,
                                   CurrentAmt = lastquery.CurrentAmt,
                                   FDueOver90Amt = lastquery.FDueOver90Amt,
                                   PastDueAmt = lastquery.PastDueAmt,
                                   Risk = lastquery.Risk,
                                   Value = lastquery.Value,
                                   Class = lastquery.Class,
                                   SiteUseId = lastquery.SiteUseId,
                                   CreditTrem = lastquery.CreditTrem,
                                   CollectorName = lastquery.CollectorName,
                                   Sales = lastquery.Sales,
                                   Due15Amt = lastquery.Due15Amt,
                                   Due30Amt = lastquery.Due30Amt,
                                   Due45Amt = lastquery.Due45Amt,
                                   Due60Amt = lastquery.Due60Amt,
                                   Due90Amt = lastquery.Due90Amt,
                                   Due120Amt = lastquery.Due120Amt,
                                   TotalFutureDue = lastquery.TotalFutureDue,
                                   CS = lastquery.CS,
                                   overDueAMT = lastquery.overDueAMT,
                                   arBalance = lastquery.arBalance,
                                   PTP_1 = lastquery.PTP_1,
                                   PTP_2 = lastquery.PTP_2,
                                   Remindering = lastquery.Remindering,
                                   Dunning_1 = lastquery.Dunning_1,
                                   Dunning_2 = lastquery.Dunning_2
                               })
                          group res by new
                          {
                              res.Id,
                              res.ActionDate,
                              res.Deal,
                              res.TaskId,
                              res.ReferenceNo,
                              res.ProcessId,
                              res.SoaStatus,
                              res.CauseObjectNumber,
                              res.BatchType,
                              res.FailedReason,
                              res.PeriodId,
                              res.AlertType,
                              res.CustomerNum,
                              res.CustomerName,
                              res.BillGroupCode,
                              res.BillGroupName,
                              res.Operator,
                              res.Risk,
                              res.Value,
                              res.Class,
                              res.SiteUseId,
                              res.CreditTrem,
                              res.CollectorName,
                              res.Sales,
                              res.Due15Amt,
                              res.Due30Amt,
                              res.Due45Amt,
                              res.Due60Amt,
                              res.Due90Amt,
                              res.Due120Amt,
                              res.TotalFutureDue,
                              res.CS,
                              res.overDueAMT,
                              res.arBalance,
                              res.PTP_1,
                              res.PTP_2,
                              res.Remindering,
                              res.Dunning_1,
                              res.Dunning_2
                          }
                              into reses
                          select new
                          {
                              Id = reses.Key.Id,
                              ActionDate = reses.Key.ActionDate,
                              Deal = reses.Key.Deal,
                              TaskId = reses.Key.TaskId,
                              ReferenceNo = reses.Key.ReferenceNo,
                              ProcessId = reses.Key.ProcessId,
                              SoaStatus = reses.Key.SoaStatus,
                              CauseObjectNumber = reses.Key.CauseObjectNumber,
                              BatchType = reses.Key.BatchType,
                              FailedReason = reses.Key.FailedReason,
                              PeriodId = reses.Key.PeriodId,
                              AlertType = reses.Key.AlertType,
                              CustomerNum = reses.Key.CustomerNum,
                              CustomerName = reses.Key.CustomerName,
                              BillGroupCode = reses.Key.BillGroupCode,
                              BillGroupName = reses.Key.BillGroupName,
                              Class = reses.Key.Class,
                              Risk = reses.Key.Risk,
                              CreditLimit = reses.Sum(age => age.CreditLimit),
                              TotalAmt = reses.Sum(age => age.TotalAmt),
                              CurrentAmt = reses.Sum(age => age.CurrentAmt),
                              FDueOver90Amt = reses.Sum(age => age.FDueOver90Amt),
                              PastDueAmt = reses.Sum(age => age.PastDueAmt),
                              Operator = reses.Key.Operator,
                              SiteUseId = reses.Key.SiteUseId,
                              CreditTrem = reses.Key.CreditTrem,
                              CollectorName = reses.Key.CollectorName,
                              Sales = reses.Key.Sales,
                              Due15Amt = reses.Key.Due15Amt,
                              Due30Amt = reses.Key.Due30Amt,
                              Due45Amt = reses.Key.Due45Amt,
                              Due60Amt = reses.Key.Due60Amt,
                              Due90Amt = reses.Key.Due90Amt,
                              Due120Amt = reses.Key.Due120Amt,
                              TotalFutureDue = reses.Key.TotalFutureDue,
                              CS = reses.Key.CS,
                              PTP_1 = reses.Key.PTP_1,
                              PTP_2 = reses.Key.PTP_2,
                              Remindering = reses.Key.Remindering,
                              Dunning_1 = reses.Key.Dunning_1,
                              Dunning_2 = reses.Key.Dunning_2,
                              overDueAMT = reses.Key.overDueAMT,
                              arBalance = reses.Key.arBalance
                          })

                     join aging in CommonRep.GetDbSet<CustomerAging>().Where(o => o.Deal == AppContext.Current.User.Deal) on new { final.CustomerNum, final.Deal, final.SiteUseId } equals new { aging.CustomerNum, aging.Deal, aging.SiteUseId }
                        into agings
                     select new SoaDto
                     {
                         Id = final.Id,
                         ActionDate = final.ActionDate,
                         Deal = final.Deal,
                         TaskId = final.TaskId,
                         ReferenceNo = final.ReferenceNo,
                         ProcessId = final.ProcessId,
                         SoaStatus = final.SoaStatus,
                         CauseObjectNumber = final.CauseObjectNumber,
                         BatchType = final.BatchType,
                         FailedReason = final.FailedReason,
                         PeriodId = final.PeriodId,
                         AlertType = final.AlertType,
                         CustomerNum = final.CustomerNum,
                         CustomerName = final.CustomerName,
                         BillGroupCode = string.IsNullOrEmpty(final.BillGroupCode) == true ? final.CustomerName : final.BillGroupCode,
                         BillGroupName = string.IsNullOrEmpty(final.BillGroupName) == true ? final.CustomerName : final.BillGroupName,
                         CreditLimit = final.CreditLimit,
                         TotalAmt = final.TotalAmt,
                         CurrentAmt = final.CurrentAmt,
                         FDueOver90Amt = final.FDueOver90Amt,
                         PastDueAmt = final.PastDueAmt,
                         LegalEntityList = from l in agings
                                           select l.LegalEntity,
                         Class = final.Class,
                         Risk = final.Risk,
                         Operator = final.Operator,
                         SiteUseId = final.SiteUseId,
                         CreditTrem = final.CreditTrem,
                         CollectorName = final.CollectorName,
                         Sales = final.Sales,
                         Due15Amt = final.Due15Amt,
                         Due30Amt = final.Due30Amt,
                         Due45Amt = final.Due45Amt,
                         Due60Amt = final.Due60Amt,
                         Due90Amt = final.Due90Amt,
                         Due120Amt = final.Due120Amt,
                         TotalFutureDue = final.TotalFutureDue,
                         CS = final.CS,
                         PTP_1 = final.PTP_1,
                         PTP_2 = final.PTP_2,
                         Remindering = final.Remindering,
                         Dunning_1 = final.Dunning_1,
                         Dunning_2 = final.Dunning_2,
                         overDueAMT = final.overDueAMT,
                         arBalance = final.arBalance
                     });
                #endregion
            }
            else
            {
                #region Without InvoiceState and InvoiceTrackState
                var assType = from x in CommonRep.GetDbSet<T_CustomerAssessment>()
                              join y in CommonRep.GetDbSet<T_AssessmentType>()
                              on x.AssessmentType equals y.Id
                              into xy
                              from y in xy.DefaultIfEmpty()
                              select new { CustomerNum = x.CustomerId, DunningPirority = y.DunningPirority, SiteUseId = x.SiteUseId };

                // base table: dunning
                r = (from final in
                         (from res in
                              (from soa in soas
                                   // left join customer level
                               join cust in SpringFactory.GetObjectImpl<CustomerService>("CustomerService").GetCustomerLevel(CurrentPeriod) on new { soa.CustomerNum, soa.Deal, soa.SiteUseId } equals new { cust.CustomerNum, cust.Deal, cust.SiteUseId }
                                  into custs
                               from cust in custs.DefaultIfEmpty()
                               join age in CommonRep.GetQueryable<CustomerAging>().Where(o => o.Deal == AppContext.Current.User.Deal) on new { cust.CustomerNum, cust.Deal, cust.SiteUseId } equals new { age.CustomerNum, age.Deal, age.SiteUseId }
                                  into ages
                               from age in ages.DefaultIfEmpty()
                               join custorder in assType on new { CustomerNum = age.CustomerNum, SiteUseId = age.SiteUseId } equals new { CustomerNum = custorder.CustomerNum, SiteUseId = custorder.SiteUseId }
                               into custorders
                               from custorderss in custorders.DefaultIfEmpty()
                               join ccv in CommonRep.GetQueryable<CustomerContactorView>() on new { Deal = cust.Deal, CustomerNum = cust.CustomerNum, SiteUseId = cust.SiteUseId } equals new { Deal = ccv.Deal, CustomerNum = ccv.CustomerNum, SiteUseId = ccv.SiteUseId }
                               into ccvs
                               from ccvsss in ccvs.DefaultIfEmpty()
                               join duncount in CommonRep.GetQueryable<V_DUN_COUNT>() on new { CustomerNum = cust.CustomerNum, SiteUseId = cust.SiteUseId, LegalEntity = age.LegalEntity } equals new { CustomerNum = duncount.CUSTOMER_NUM, SiteUseId = duncount.SiteUseId, LegalEntity = duncount.LEGAL_ENTITY }
                               into ccdu
                               from ccvsst in ccdu.DefaultIfEmpty()
                               select new
                               {
                                   Id = soa.Id,
                                   ActionDate = soa.ActionDate,
                                   Deal = cust.Deal,
                                   TaskId = soa.TaskId,
                                   ReferenceNo = soa.ReferenceNo,
                                   ProcessId = soa.ProcessId,
                                   SoaStatus = soa.Status,
                                   CauseObjectNumber = soa.CauseObjectNumber,
                                   BatchType = soa.BatchType,
                                   FailedReason = soa.FailedReason,
                                   PeriodId = soa.PeriodId,
                                   AlertType = soa.AlertType,
                                   CustomerNum = cust.CustomerNum,
                                   CustomerName = cust.CustomerName,
                                   BillGroupCode = cust.BillGroupCode,
                                   BillGroupName = cust.BillGroupName,
                                   Operator = cust.Collector,
                                   CreditLimit = age == null ? 0 : age.CreditLimit,
                                   TotalAmt = age == null ? 0 : age.TotalAmt,
                                   CurrentAmt = age == null ? 0 : age.CurrentAmt,
                                   FDueOver90Amt = age == null ? 0 : (age.Due120Amt + age.Due150Amt + age.Due180Amt + age.Due210Amt + age.Due240Amt + age.Due270Amt + age.Due300Amt + age.Due330Amt + age.Due360Amt + age.DueOver360Amt),
                                   PastDueAmt = age == null ? 0 : age.DueoverTotalAmt,
                                   Risk = cust.Risk,
                                   Value = cust.Value,
                                   Class = cust.Class,
                                   SiteUseId = age.SiteUseId,
                                   CreditTrem = age.CreditTrem,
                                   CollectorName = ccvsss.Contactor,
                                   Sales = age.Sales,
                                   Due15Amt = age.DUE15_AMT,
                                   Due30Amt = age.Due30Amt,
                                   Due45Amt = age.DUE45_AMT,
                                   Due60Amt = age.Due60Amt,
                                   Due90Amt = age.Due90Amt,
                                   Due120Amt = age.Due120Amt,
                                   TotalFutureDue = age.TotalFutureDue,
                                   CS = cust.CS,
                                   overDueAMT = age.DueoverTotalAmt,
                                   arBalance = age.TotalAmt,
                                   DunningPirority = custorderss != null ? custorderss.DunningPirority : 3,
                                   PTP_1 = ccvsst != null ? ccvsst.PTP_1 : 0,
                                   PTP_2 = ccvsst != null ? ccvsst.PTP_2 : 0,
                                   Remindering = ccvsst != null ? ccvsst.REMINDING : 0,
                                   Dunning_1 = ccvsst != null ? ccvsst.Dunning_1 : 0,
                                   Dunning_2 = ccvsst != null ? ccvsst.Dunning_2 : 0,
                                   EB = age.Ebname
                               }
                               into querybydunpirority
                               orderby querybydunpirority.DunningPirority
                               select new
                               {
                                   Id = querybydunpirority.Id,
                                   ActionDate = querybydunpirority.ActionDate,
                                   Deal = querybydunpirority.Deal,
                                   TaskId = querybydunpirority.TaskId,
                                   ReferenceNo = querybydunpirority.ReferenceNo,
                                   ProcessId = querybydunpirority.ProcessId,
                                   SoaStatus = querybydunpirority.SoaStatus,
                                   CauseObjectNumber = querybydunpirority.CauseObjectNumber,
                                   BatchType = querybydunpirority.BatchType,
                                   FailedReason = querybydunpirority.FailedReason,
                                   PeriodId = querybydunpirority.PeriodId,
                                   AlertType = querybydunpirority.AlertType,
                                   CustomerNum = querybydunpirority.CustomerNum,
                                   CustomerName = querybydunpirority.CustomerName,
                                   BillGroupCode = querybydunpirority.BillGroupCode,
                                   BillGroupName = querybydunpirority.BillGroupName,
                                   Operator = querybydunpirority.Operator,
                                   CreditLimit = querybydunpirority.CreditLimit,
                                   TotalAmt = querybydunpirority.TotalAmt,
                                   CurrentAmt = querybydunpirority.CurrentAmt,
                                   FDueOver90Amt = querybydunpirority.FDueOver90Amt,
                                   PastDueAmt = querybydunpirority.PastDueAmt,
                                   Risk = querybydunpirority.Risk,
                                   Value = querybydunpirority.Value,
                                   Class = querybydunpirority.Class,
                                   SiteUseId = querybydunpirority.SiteUseId,
                                   CreditTrem = querybydunpirority.CreditTrem,
                                   CollectorName = querybydunpirority.CollectorName,
                                   Sales = querybydunpirority.Sales,
                                   Due15Amt = querybydunpirority.Due15Amt,
                                   Due30Amt = querybydunpirority.Due30Amt,
                                   Due45Amt = querybydunpirority.Due45Amt,
                                   Due60Amt = querybydunpirority.Due60Amt,
                                   Due90Amt = querybydunpirority.Due90Amt,
                                   Due120Amt = querybydunpirority.Due120Amt,
                                   TotalFutureDue = querybydunpirority.TotalFutureDue,
                                   CS = querybydunpirority.CS,
                                   overDueAMT = querybydunpirority.overDueAMT,
                                   arBalance = querybydunpirority.arBalance,
                                   PTP_1 = querybydunpirority.PTP_1,
                                   PTP_2 = querybydunpirority.PTP_2,
                                   Remindering = querybydunpirority.Remindering,
                                   Dunning_1 = querybydunpirority.Dunning_1,
                                   Dunning_2 = querybydunpirority.Dunning_2,
                                   EB = querybydunpirority.EB
                               })
                          group res by new
                          {
                              res.Id,
                              res.ActionDate,
                              res.Deal,
                              res.TaskId,
                              res.ReferenceNo,
                              res.ProcessId,
                              res.SoaStatus,
                              res.CauseObjectNumber,
                              res.BatchType,
                              res.FailedReason,
                              res.PeriodId,
                              res.AlertType,
                              res.CustomerNum,
                              res.CustomerName,
                              res.BillGroupCode,
                              res.BillGroupName,
                              res.Operator,
                              res.Risk,
                              res.Value,
                              res.Class,
                              res.SiteUseId,
                              res.CreditTrem,
                              res.CollectorName,
                              res.Sales,
                              res.Due15Amt,
                              res.Due30Amt,
                              res.Due45Amt,
                              res.Due60Amt,
                              res.Due90Amt,
                              res.Due120Amt,
                              res.TotalFutureDue,
                              res.CS,
                              res.overDueAMT,
                              res.arBalance,
                              res.PTP_1,
                              res.PTP_2,
                              res.Remindering,
                              res.Dunning_1,
                              res.Dunning_2,
                              res.EB
                          }
                              into reses
                          select new
                          {
                              Id = reses.Key.Id,
                              ActionDate = reses.Key.ActionDate,
                              Deal = reses.Key.Deal,
                              TaskId = reses.Key.TaskId,
                              ReferenceNo = reses.Key.ReferenceNo,
                              ProcessId = reses.Key.ProcessId,
                              SoaStatus = reses.Key.SoaStatus,
                              CauseObjectNumber = reses.Key.CauseObjectNumber,
                              BatchType = reses.Key.BatchType,
                              FailedReason = reses.Key.FailedReason,
                              PeriodId = reses.Key.PeriodId,
                              AlertType = reses.Key.AlertType,
                              CustomerNum = reses.Key.CustomerNum,
                              CustomerName = reses.Key.CustomerName,
                              BillGroupCode = reses.Key.BillGroupCode,
                              BillGroupName = reses.Key.BillGroupName,
                              Class = reses.Key.Class,
                              Risk = reses.Key.Risk,
                              CreditLimit = reses.Sum(age => age.CreditLimit),
                              TotalAmt = reses.Sum(age => age.TotalAmt),
                              CurrentAmt = reses.Sum(age => age.CurrentAmt),
                              FDueOver90Amt = reses.Sum(age => age.FDueOver90Amt),
                              PastDueAmt = reses.Sum(age => age.PastDueAmt),
                              Operator = reses.Key.Operator,
                              SiteUseId = reses.Key.SiteUseId,
                              CreditTrem = reses.Key.CreditTrem,
                              CollectorName = reses.Key.CollectorName,
                              Sales = reses.Key.Sales,
                              Due15Amt = reses.Key.Due15Amt,
                              Due30Amt = reses.Key.Due30Amt,
                              Due45Amt = reses.Key.Due45Amt,
                              Due60Amt = reses.Key.Due60Amt,
                              Due90Amt = reses.Key.Due90Amt,
                              Due120Amt = reses.Key.Due120Amt,
                              TotalFutureDue = reses.Key.TotalFutureDue,
                              CS = reses.Key.CS,
                              PTP_1 = reses.Key.PTP_1,
                              PTP_2 = reses.Key.PTP_2,
                              Remindering = reses.Key.Remindering,
                              Dunning_1 = reses.Key.Dunning_1,
                              Dunning_2 = reses.Key.Dunning_2,
                              overDueAMT = reses.Key.overDueAMT,
                              arBalance = reses.Key.arBalance,
                              EB = reses.Key.EB
                          })

                     join aging in CommonRep.GetDbSet<CustomerAging>().Where(o => o.Deal == AppContext.Current.User.Deal) on new { final.CustomerNum, final.Deal, final.SiteUseId } equals new { aging.CustomerNum, aging.Deal, aging.SiteUseId }
                        into agings
                     select new SoaDto
                     {
                         Id = final.Id,
                         ActionDate = final.ActionDate,
                         Deal = final.Deal,
                         TaskId = final.TaskId,
                         ReferenceNo = final.ReferenceNo,
                         ProcessId = final.ProcessId,
                         SoaStatus = final.SoaStatus,
                         CauseObjectNumber = final.CauseObjectNumber,
                         BatchType = final.BatchType,
                         FailedReason = final.FailedReason,
                         PeriodId = final.PeriodId,
                         AlertType = final.AlertType,
                         CustomerNum = final.CustomerNum,
                         CustomerName = final.CustomerName,
                         BillGroupCode = string.IsNullOrEmpty(final.BillGroupCode) == true ? final.CustomerName : final.BillGroupCode,
                         BillGroupName = string.IsNullOrEmpty(final.BillGroupName) == true ? final.CustomerName : final.BillGroupName,
                         CreditLimit = final.CreditLimit,
                         TotalAmt = final.TotalAmt,
                         CurrentAmt = final.CurrentAmt,
                         FDueOver90Amt = final.FDueOver90Amt,
                         PastDueAmt = final.PastDueAmt,
                         LegalEntityList = from l in agings
                                           select l.LegalEntity,
                         Class = final.Class,
                         Risk = final.Risk,
                         Operator = final.Operator,
                         SiteUseId = final.SiteUseId,
                         CreditTrem = final.CreditTrem,
                         CollectorName = final.CollectorName,
                         Sales = final.Sales,
                         Due15Amt = final.Due15Amt,
                         Due30Amt = final.Due30Amt,
                         Due45Amt = final.Due45Amt,
                         Due60Amt = final.Due60Amt,
                         Due90Amt = final.Due90Amt,
                         Due120Amt = final.Due120Amt,
                         TotalFutureDue = final.TotalFutureDue,
                         CS = final.CS,
                         PTP_1 = final.PTP_1,
                         PTP_2 = final.PTP_2,
                         Remindering = final.Remindering,
                         Dunning_1 = final.Dunning_1,
                         Dunning_2 = final.Dunning_2,
                         overDueAMT = final.overDueAMT,
                         arBalance = final.arBalance,
                         EB = final.EB
                     });
                #endregion
            }

            return r;
        }

        public IEnumerable<SoaDto> SelectChangePeriod(int PeriodId)
        {

            if (PeriodId > 0)
            {
                IQueryable<SoaDto> r = null;
                List<SysUser> listUser = new List<SysUser>();
                listUser = XccService.GetUserTeamList(AppContext.Current.User.EID);
                string[] userGroup = listUser.Select(t => t.EID).Distinct().ToArray();
                string collecotrList = "," + string.Join(",", userGroup.ToArray()) + ",";

                //&& soa.PeriodId == CurrentPeriod 
                var soas = CommonRep.GetDbSet<CollectorAlert>().Where(o => o.Status == "Finish" && (o.AlertType == 1) && o.PeriodId == PeriodId);
                if (AppContext.Current.User.ActionPermissions.IndexOf("alldataforsupervisor") >= 0)
                {
                }
                else
                {
                    soas = soas.Where(o => collecotrList.Contains("," + o.Eid + ","));
                }

                #region Without InvoiceState and InvoiceTrackState
                var assType = from x in CommonRep.GetDbSet<T_CustomerAssessment>()
                              join y in CommonRep.GetDbSet<T_AssessmentType>()
                              on x.AssessmentType equals y.Id
                              into xy
                              from y in xy.DefaultIfEmpty()
                              select new { CustomerNum = x.CustomerId, DunningPirority = y.DunningPirority, SiteUseId = x.SiteUseId };

                // base table: dunning
                r = (from final in
                         (from res in
                              (from soa in soas
                                   // left join customer level
                               join cust in SpringFactory.GetObjectImpl<CustomerService>("CustomerService").GetCustomerLevel(CurrentPeriod) on new { soa.CustomerNum, soa.Deal, soa.SiteUseId } equals new { cust.CustomerNum, cust.Deal, cust.SiteUseId }
                                  into custs
                               from cust in custs.DefaultIfEmpty()
                               join age in CommonRep.GetQueryable<CustomerAging>().Where(o => o.Deal == AppContext.Current.User.Deal) on new { cust.CustomerNum, cust.Deal, cust.SiteUseId } equals new { age.CustomerNum, age.Deal, age.SiteUseId }
                                  into ages
                               from age in ages.DefaultIfEmpty()
                               join custorder in assType on new { CustomerNum = age.CustomerNum, SiteUseId = age.SiteUseId } equals new { CustomerNum = custorder.CustomerNum, SiteUseId = custorder.SiteUseId }
                               into custorders
                               from custorderss in custorders.DefaultIfEmpty()
                               join ccv in CommonRep.GetQueryable<CustomerContactorView>() on new { Deal = cust.Deal, CustomerNum = cust.CustomerNum, SiteUseId = cust.SiteUseId } equals new { Deal = ccv.Deal, CustomerNum = ccv.CustomerNum, SiteUseId = ccv.SiteUseId }
                               into ccvs
                               from ccvsss in ccvs.DefaultIfEmpty()
                               join duncount in CommonRep.GetQueryable<V_DUN_COUNT>() on new { CustomerNum = cust.CustomerNum, SiteUseId = cust.SiteUseId, LegalEntity = age.LegalEntity } equals new { CustomerNum = duncount.CUSTOMER_NUM, SiteUseId = duncount.SiteUseId, LegalEntity = duncount.LEGAL_ENTITY }
                               into ccdu
                               from ccvsst in ccdu.DefaultIfEmpty()
                               select new
                               {
                                   Id = soa.Id,
                                   ActionDate = soa.ActionDate,
                                   Deal = cust.Deal,
                                   TaskId = soa.TaskId,
                                   ReferenceNo = soa.ReferenceNo,
                                   ProcessId = soa.ProcessId,
                                   SoaStatus = soa.Status,
                                   CauseObjectNumber = soa.CauseObjectNumber,
                                   BatchType = soa.BatchType,
                                   FailedReason = soa.FailedReason,
                                   PeriodId = soa.PeriodId,
                                   AlertType = soa.AlertType,
                                   CustomerNum = cust.CustomerNum,
                                   CustomerName = cust.CustomerName,
                                   BillGroupCode = cust.BillGroupCode,
                                   BillGroupName = cust.BillGroupName,
                                   Operator = cust.Collector,
                                   CreditLimit = age == null ? 0 : age.CreditLimit,
                                   TotalAmt = age == null ? 0 : age.TotalAmt,
                                   CurrentAmt = age == null ? 0 : age.CurrentAmt,
                                   FDueOver90Amt = age == null ? 0 : (age.Due120Amt + age.Due150Amt + age.Due180Amt + age.Due210Amt + age.Due240Amt + age.Due270Amt + age.Due300Amt + age.Due330Amt + age.Due360Amt + age.DueOver360Amt),
                                   PastDueAmt = age == null ? 0 : age.DueoverTotalAmt,
                                   Risk = cust.Risk,
                                   Value = cust.Value,
                                   Class = cust.Class,
                                   SiteUseId = age.SiteUseId,
                                   CreditTrem = age.CreditTrem,
                                   CollectorName = ccvsss.Contactor,
                                   Sales = age.Sales,
                                   Due15Amt = age.DUE15_AMT,
                                   Due30Amt = age.Due30Amt,
                                   Due45Amt = age.DUE45_AMT,
                                   Due60Amt = age.Due60Amt,
                                   Due90Amt = age.Due90Amt,
                                   Due120Amt = age.Due120Amt,
                                   TotalFutureDue = age.TotalFutureDue,
                                   CS = cust.CS,
                                   overDueAMT = age.DueoverTotalAmt,
                                   arBalance = age.TotalAmt,
                                   DunningPirority = custorderss != null ? custorderss.DunningPirority : 3,
                                   PTP_1 = ccvsst != null ? ccvsst.PTP_1 : 0,
                                   PTP_2 = ccvsst != null ? ccvsst.PTP_2 : 0,
                                   Remindering = ccvsst != null ? ccvsst.REMINDING : 0,
                                   Dunning_1 = ccvsst != null ? ccvsst.Dunning_1 : 0,
                                   Dunning_2 = ccvsst != null ? ccvsst.Dunning_2 : 0,
                                   EB = age.Ebname
                               }
                               into querybydunpirority
                               orderby querybydunpirority.DunningPirority
                               select new
                               {
                                   Id = querybydunpirority.Id,
                                   ActionDate = querybydunpirority.ActionDate,
                                   Deal = querybydunpirority.Deal,
                                   TaskId = querybydunpirority.TaskId,
                                   ReferenceNo = querybydunpirority.ReferenceNo,
                                   ProcessId = querybydunpirority.ProcessId,
                                   SoaStatus = querybydunpirority.SoaStatus,
                                   CauseObjectNumber = querybydunpirority.CauseObjectNumber,
                                   BatchType = querybydunpirority.BatchType,
                                   FailedReason = querybydunpirority.FailedReason,
                                   PeriodId = querybydunpirority.PeriodId,
                                   AlertType = querybydunpirority.AlertType,
                                   CustomerNum = querybydunpirority.CustomerNum,
                                   CustomerName = querybydunpirority.CustomerName,
                                   BillGroupCode = querybydunpirority.BillGroupCode,
                                   BillGroupName = querybydunpirority.BillGroupName,
                                   Operator = querybydunpirority.Operator,
                                   CreditLimit = querybydunpirority.CreditLimit,
                                   TotalAmt = querybydunpirority.TotalAmt,
                                   CurrentAmt = querybydunpirority.CurrentAmt,
                                   FDueOver90Amt = querybydunpirority.FDueOver90Amt,
                                   PastDueAmt = querybydunpirority.PastDueAmt,
                                   Risk = querybydunpirority.Risk,
                                   Value = querybydunpirority.Value,
                                   Class = querybydunpirority.Class,
                                   SiteUseId = querybydunpirority.SiteUseId,
                                   CreditTrem = querybydunpirority.CreditTrem,
                                   CollectorName = querybydunpirority.CollectorName,
                                   Sales = querybydunpirority.Sales,
                                   Due15Amt = querybydunpirority.Due15Amt,
                                   Due30Amt = querybydunpirority.Due30Amt,
                                   Due45Amt = querybydunpirority.Due45Amt,
                                   Due60Amt = querybydunpirority.Due60Amt,
                                   Due90Amt = querybydunpirority.Due90Amt,
                                   Due120Amt = querybydunpirority.Due120Amt,
                                   TotalFutureDue = querybydunpirority.TotalFutureDue,
                                   CS = querybydunpirority.CS,
                                   overDueAMT = querybydunpirority.overDueAMT,
                                   arBalance = querybydunpirority.arBalance,
                                   PTP_1 = querybydunpirority.PTP_1,
                                   PTP_2 = querybydunpirority.PTP_2,
                                   Remindering = querybydunpirority.Remindering,
                                   Dunning_1 = querybydunpirority.Dunning_1,
                                   Dunning_2 = querybydunpirority.Dunning_2,
                                   EB = querybydunpirority.EB
                               })
                          group res by new
                          {
                              res.Id,
                              res.ActionDate,
                              res.Deal,
                              res.TaskId,
                              res.ReferenceNo,
                              res.ProcessId,
                              res.SoaStatus,
                              res.CauseObjectNumber,
                              res.BatchType,
                              res.FailedReason,
                              res.PeriodId,
                              res.AlertType,
                              res.CustomerNum,
                              res.CustomerName,
                              res.BillGroupCode,
                              res.BillGroupName,
                              res.Operator,
                              res.Risk,
                              res.Value,
                              res.Class,
                              //Start add by xuan.wu for Arrow adding
                              res.SiteUseId,
                              res.CreditTrem,
                              res.CollectorName,
                              res.Sales,
                              res.Due15Amt,
                              res.Due30Amt,
                              res.Due45Amt,
                              res.Due60Amt,
                              res.Due90Amt,
                              res.Due120Amt,
                              res.TotalFutureDue,
                              res.CS,
                              res.overDueAMT,
                              res.arBalance,
                              res.PTP_1,
                              res.PTP_2,
                              res.Remindering,
                              res.Dunning_1,
                              res.Dunning_2,
                              res.EB
                              //End add by xuan.wu for Arrow adding
                          }
                              into reses
                          select new
                          {
                              Id = reses.Key.Id,
                              ActionDate = reses.Key.ActionDate,
                              Deal = reses.Key.Deal,
                              TaskId = reses.Key.TaskId,
                              ReferenceNo = reses.Key.ReferenceNo,
                              ProcessId = reses.Key.ProcessId,
                              SoaStatus = reses.Key.SoaStatus,
                              CauseObjectNumber = reses.Key.CauseObjectNumber,
                              BatchType = reses.Key.BatchType,
                              FailedReason = reses.Key.FailedReason,
                              PeriodId = reses.Key.PeriodId,
                              AlertType = reses.Key.AlertType,
                              CustomerNum = reses.Key.CustomerNum,
                              CustomerName = reses.Key.CustomerName,
                              BillGroupCode = reses.Key.BillGroupCode,
                              BillGroupName = reses.Key.BillGroupName,
                              Class = reses.Key.Class,
                              Risk = reses.Key.Risk,
                              CreditLimit = reses.Sum(age => age.CreditLimit),
                              TotalAmt = reses.Sum(age => age.TotalAmt),
                              CurrentAmt = reses.Sum(age => age.CurrentAmt),
                              FDueOver90Amt = reses.Sum(age => age.FDueOver90Amt),
                              PastDueAmt = reses.Sum(age => age.PastDueAmt),
                              Operator = reses.Key.Operator,
                              SiteUseId = reses.Key.SiteUseId,
                              CreditTrem = reses.Key.CreditTrem,
                              CollectorName = reses.Key.CollectorName,
                              Sales = reses.Key.Sales,
                              Due15Amt = reses.Key.Due15Amt,
                              Due30Amt = reses.Key.Due30Amt,
                              Due45Amt = reses.Key.Due45Amt,
                              Due60Amt = reses.Key.Due60Amt,
                              Due90Amt = reses.Key.Due90Amt,
                              Due120Amt = reses.Key.Due120Amt,
                              TotalFutureDue = reses.Key.TotalFutureDue,
                              CS = reses.Key.CS,
                              PTP_1 = reses.Key.PTP_1,
                              PTP_2 = reses.Key.PTP_2,
                              Remindering = reses.Key.Remindering,
                              Dunning_1 = reses.Key.Dunning_1,
                              Dunning_2 = reses.Key.Dunning_2,
                              overDueAMT = reses.Key.overDueAMT,
                              arBalance = reses.Key.arBalance,
                              EB = reses.Key.EB
                              //End add by xuan.wu for Arrow adding
                          })

                     join aging in CommonRep.GetDbSet<CustomerAging>().Where(o => o.Deal == AppContext.Current.User.Deal) on new { final.CustomerNum, final.Deal, final.SiteUseId } equals new { aging.CustomerNum, aging.Deal, aging.SiteUseId }
                        into agings
                     select new SoaDto
                     {
                         Id = final.Id,
                         ActionDate = final.ActionDate,
                         Deal = final.Deal,
                         TaskId = final.TaskId,
                         ReferenceNo = final.ReferenceNo,
                         ProcessId = final.ProcessId,
                         SoaStatus = final.SoaStatus,
                         CauseObjectNumber = final.CauseObjectNumber,
                         BatchType = final.BatchType,
                         FailedReason = final.FailedReason,
                         PeriodId = final.PeriodId,
                         AlertType = final.AlertType,
                         CustomerNum = final.CustomerNum,
                         CustomerName = final.CustomerName,
                         BillGroupCode = string.IsNullOrEmpty(final.BillGroupCode) == true ? final.CustomerName : final.BillGroupCode,
                         BillGroupName = string.IsNullOrEmpty(final.BillGroupName) == true ? final.CustomerName : final.BillGroupName,
                         CreditLimit = final.CreditLimit,
                         TotalAmt = final.TotalAmt,
                         CurrentAmt = final.CurrentAmt,
                         FDueOver90Amt = final.FDueOver90Amt,
                         PastDueAmt = final.PastDueAmt,
                         LegalEntityList = from l in agings
                                           select l.LegalEntity,
                         Class = final.Class,
                         Risk = final.Risk,
                         Operator = final.Operator,
                         SiteUseId = final.SiteUseId,
                         CreditTrem = final.CreditTrem,
                         CollectorName = final.CollectorName,
                         Sales = final.Sales,
                         Due15Amt = final.Due15Amt,
                         Due30Amt = final.Due30Amt,
                         Due45Amt = final.Due45Amt,
                         Due60Amt = final.Due60Amt,
                         Due90Amt = final.Due90Amt,
                         Due120Amt = final.Due120Amt,
                         TotalFutureDue = final.TotalFutureDue,
                         CS = final.CS,
                         PTP_1 = final.PTP_1,
                         PTP_2 = final.PTP_2,
                         Remindering = final.Remindering,
                         Dunning_1 = final.Dunning_1,
                         Dunning_2 = final.Dunning_2,
                         overDueAMT = final.overDueAMT,
                         arBalance = final.arBalance,
                         EB = final.EB

                     });
                #endregion

                return r;
            }
            else
            {
                return GetNoPaging("complete");
            }
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

        //get all sendsoa
        public IEnumerable<SendSoaHead> CreateSoa(string ColSoa, string Type)
        {
            //var watch = System.Diagnostics.Stopwatch.StartNew();

            string oper = AppContext.Current.User.Id.ToString();
            string deal = AppContext.Current.User.Deal.ToString();

            IBaseDataService bdSer = SpringFactory.GetObjectImpl<IBaseDataService>("BaseDataService");

            #region createsoalist
            string[] cusGroup = ColSoa.Split(',');
            //cus
            var cusList = CommonRep.GetQueryable<Customer>()
                .Where(o => o.Deal == deal && cusGroup.Contains(o.CustomerNum)).Include<Customer, CustomerGroupCfg>(c => c.CustomerGroupCfg).ToList();
            Customer cus = new Customer();
            //aging
            var cusAgingList = CommonRep.GetQueryable<CustomerAging>()
                .Where(o => o.Deal == deal && cusGroup.Contains(o.CustomerNum)).ToList();
            //sendsoa
            List<SendSoaHead> sendsoaList = new List<SendSoaHead>();
            SendSoaHead sendsoa = new SendSoaHead();
            //customerchangehis=>class
            var classList = CommonRep.GetQueryable<CustomerLevelView>()
                .Where(o => o.Deal == deal && cusGroup.Contains(o.CustomerNum)).ToList();
            CustomerLevelView level = new CustomerLevelView();
            //SpecialNotes
            var SNList = CommonRep.GetQueryable<SpecialNote>().Where(o => o.Deal == deal && cusGroup.Contains(o.CustomerNum)).ToList();
            //Rate
            var rateList = CommonRep.GetQueryable<RateTran>()
                .Where(o => o.Deal == AppContext.Current.User.Deal && o.EffectiveDate <= AppContext.Current.User.Now.Date && o.ExpiredDate >= AppContext.Current.User.Now.Date).ToList();
            //agingDT
            DateTime agingDT = new DateTime();
            PeroidService pservice = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");
            PeriodControl currentP = pservice.getcurrentPeroid();
            agingDT = dataConvertToDT(currentP.PeriodEnd.ToString());
            DateTime agingDT90 = new DateTime();
            agingDT90 = agingDT.AddDays(-90);
            //invoice
            var oldinvoiceList = CommonRep.GetQueryable<InvoiceAging>()
                .Where(o => o.Deal == deal && cusGroup.Contains(o.CustomerNum)).ToList();
            List<InvoiceAging> newinvoiceList = new List<InvoiceAging>();
            newinvoiceList = oldinvoiceList;

            foreach (var item in newinvoiceList)
            {
                if (item.Currency != "USD")
                {
                    item.StandardBalanceAmt = (rateList.Find(m => m.ForeignCurrency == item.Currency).Rate == null ? 1 : rateList.Find(m => m.ForeignCurrency == item.Currency).Rate) * item.BalanceAmt;
                }
                else { item.StandardBalanceAmt = item.BalanceAmt; }
            }

            IDunningService service = SpringFactory.GetObjectImpl<IDunningService>("DunningReminderService");
            List<CollectorAlert> reminders = service.GetEstimatedReminders(cusGroup.ToList(), null);

            foreach (var item in cusGroup)
            {
                sendsoa = new SendSoaHead();
                cus = cusList.Find(m => m.Deal == deal && m.CustomerNum == item);
                level = classList.Find(m => m.Deal == deal && m.CustomerNum == item);
                var newCusAgingList = cusAgingList.FindAll(m => m.Deal == deal && m.CustomerNum == item);
                sendsoa.Deal = deal;
                sendsoa.CustomerCode = item;
                sendsoa.CustomerName = cus.CustomerName;
                sendsoa.TotalBalance = newCusAgingList.Sum(m => m.TotalAmt);
                sendsoa.CustomerClass = (string.IsNullOrEmpty(level.ClassLevel) == true ? "LV" : level.ClassLevel)
                    + (string.IsNullOrEmpty(level.RiskLevel) == true ? "LR" : level.RiskLevel);

                //contactHistory
                List<SubContactHistory> ContactHisList = new List<SubContactHistory>();
                SubContactHistory ContactHis = new SubContactHistory();
                var OldConHisList = CommonRep.GetDbSet<ContactHistory>().Where(o => o.Deal == deal && o.CustomerNum == item);
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
                    var SN = SNList.Find(m => m.CustomerNum == item);
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
                    sublegal.SubTracking = tracking;
                    sublegal.SubInvoice = subinvoiceList;
                    sublegalList.Add(sublegal);
                }
                sendsoa.SubLegal = sublegalList;
                sendsoaList.Add(sendsoa);
            }
            #endregion
            //**********************************WF Start ***********************************
            if (IsWF == "true")
            {
                if (Type == "create")
                {
                    if (GetPStatus(ColSoa, "Processing") == 0)
                    {
                        Wfchange("4", ColSoa, "start");
                    }
                }
            }
            return sendsoaList.AsQueryable<SendSoaHead>();
        }

        //start get all sendsoa by xuan.wu
        public IEnumerable<SendSoaHead> CreateSoaForArrow(string ColSoa, string Type, string SiteUsrId)
        {
            //var watch = System.Diagnostics.Stopwatch.StartNew();

            string oper = AppContext.Current.User.Id.ToString();
            string deal = AppContext.Current.User.Deal.ToString();

            IBaseDataService bdSer = SpringFactory.GetObjectImpl<IBaseDataService>("BaseDataService");

            #region createsoalist
            string[] cusGroup = ColSoa.Split(',');
            List<SendSoaHead> sendsoaList = new List<SendSoaHead>();

            //cus
            var cusList = CommonRep.GetQueryable<Customer>()
                .Where(o => o.Deal == deal && cusGroup.Contains(o.CustomerNum) && o.SiteUseId == SiteUsrId).ToList();
            Customer cus = new Customer();
            //aging
            var cusAgingList = CommonRep.GetQueryable<CustomerAging>()
                .Where(o => o.Deal == deal && o.SiteUseId == SiteUsrId && cusGroup.Contains(o.CustomerNum)).ToList();
            //sendsoa
            SendSoaHead sendsoa = new SendSoaHead();
            var classList = CommonRep.GetQueryable<CustomerLevelView>()
                .Where(o => o.Deal == deal && cusGroup.Contains(o.CustomerNum) && o.SiteUseId == SiteUsrId).ToList();
            CustomerLevelView level = new CustomerLevelView();
            //SpecialNotes
            var SNList = CommonRep.GetQueryable<SpecialNote>().Where(o => o.Deal == deal && cusGroup.Contains(o.CustomerNum) && o.SiteUseId == SiteUsrId).ToList();
            //Rate
            var rateList = CommonRep.GetQueryable<RateTran>()
                .Where(o => o.Deal == AppContext.Current.User.Deal && o.EffectiveDate <= AppContext.Current.User.Now.Date && o.ExpiredDate >= AppContext.Current.User.Now.Date).ToList();
            //agingDT
            DateTime agingDT = new DateTime();
            PeroidService pservice = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");
            PeriodControl currentP = pservice.getcurrentPeroid();
            agingDT = dataConvertToDT(currentP.PeriodEnd.ToString());
            DateTime agingDT90 = new DateTime();
            agingDT90 = agingDT.AddDays(-90);
            //invoice
            var oldinvoiceList = CommonRep.GetQueryable<InvoiceAging>()
                .Where(o => o.Deal == deal && cusGroup.Contains(o.CustomerNum) && o.SiteUseId == SiteUsrId).ToList();
            List<InvoiceAging> newinvoiceList = new List<InvoiceAging>();
            newinvoiceList = oldinvoiceList;

            // IsCostomerContact
            bool isCusContact = CommonRep.GetQueryable<Contactor>().Where(o => cusGroup.Contains(o.CustomerNum) && o.SiteUseId == SiteUsrId && o.IsCostomerContact == true).Count() > 0;

            DateTime nNowTime = DateTime.Now;
            PeriodControl periodCtrl = CommonRep.GetQueryable<PeriodControl>().Where(x => x.PeriodBegin <= nNowTime && x.PeriodEnd > nNowTime && x.SoaFlg == "1").FirstOrDefault();
            if (periodCtrl == null)
            {
                Helper.Log.Error("Period信息未维护！", null);
            }
            DateTime? ReconciliationDay = CommonRep.GetQueryable<CustomerPaymentCircle>().Where(o => cusGroup.Contains(o.CustomerNum) && o.SiteUseId == SiteUsrId && o.Reconciliation_Day != null)
                    .ToList()
                    .Where(o => o.Reconciliation_Day >= periodCtrl.PeriodBegin)
                    .Select(x => x.Reconciliation_Day).FirstOrDefault();

            foreach (var item in newinvoiceList)
            {
                item.StandardBalanceAmt = item.BalanceAmt;
            }

            IDunningService service = SpringFactory.GetObjectImpl<IDunningService>("DunningReminderService");
            List<CollectorAlert> reminders = service.GetEstimatedRemindersForArrow(cusGroup.ToList(), SiteUsrId, null);
            string legalentity = "";
            try
            {
                legalentity = cusAgingList.FindAll(m => m.Deal == deal && m.CustomerNum == cusGroup[0] && m.SiteUseId == SiteUsrId).First().LegalEntity;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            SqlParameter[] para = new SqlParameter[3];
            para[0] = new SqlParameter("@SiteUseId", SiteUsrId);
            para[1] = new SqlParameter("@CustomerNo", cusGroup[0]);
            para[2] = new SqlParameter("@LegalEntity", legalentity);
            DataTable dt = CommonRep.GetDBContext().Database.ExecuteDataTable("P_Dun_STRATEGY", para);
            int dunflag = int.Parse(dt.Rows[0][0].ToString());
            string DunningName = "New Customer";
            string custnum = cusGroup[0];
            IQueryable<T_CustomerAssessment> custass = CommonRep.GetDbSet<T_CustomerAssessment>().Where(i => i.CustomerId == custnum && i.SiteUseId == SiteUsrId);
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
            //DunningName = assType.ToString();
            foreach (var item in cusGroup)
            {
                sendsoa = new SendSoaHead();
                cus = cusList.Find(m => m.Deal == deal && m.CustomerNum == item && m.SiteUseId == SiteUsrId);
                level = classList.Find(m => m.Deal == deal && m.CustomerNum == item && m.SiteUseId == SiteUsrId);
                var newCusAgingList = cusAgingList.FindAll(m => m.Deal == deal && m.CustomerNum == item && m.SiteUseId == SiteUsrId);
                sendsoa.Deal = deal;
                sendsoa.CustomerCode = item;
                sendsoa.CustomerName = cus.CustomerName;
                sendsoa.IsCostomerContact = isCusContact == true ? "Y" : "N";
                sendsoa.ReconciliationDay = ReconciliationDay == null ? new DateTime(2099, 12, 31) : (DateTime)ReconciliationDay;
                sendsoa.TotalBalance = newCusAgingList.Sum(m => m.TotalAmt);
                sendsoa.CustomerClass = string.IsNullOrEmpty(level.ClassLevel) ? "" : level.ClassLevel;
                //Start add by xuan.wu for Arrow adding
                sendsoa.SiteUseId = cus.SiteUseId;
                sendsoa.LegalEntity = newCusAgingList[0].LegalEntity;
                sendsoa.Sales = cus.FSR;
                sendsoa.CreditLimit = newCusAgingList[0].CreditLimit;
                sendsoa.CreditTremDescription = newCusAgingList[0].CreditTrem;
                sendsoa.Total_Balance = newCusAgingList[0].TotalAmt;
                sendsoa.Current_Balance = newCusAgingList[0].CurrentAmt;
                sendsoa.Amount = (newinvoiceList.Where(c => c.TrackStates != "013" && c.TrackStates != "016").Sum(m => m.BalanceAmt));
                sendsoa.Eb = newCusAgingList[0].Ebname;
                sendsoa.DunFlag = dunflag;
                sendsoa.Assessment = DunningName;
                sendsoa.comment = cus.Comment;
                sendsoa.CommentExpirationDate = cus.CommentExpirationDate;
                sendsoa.CommentLastDate = cus.CommentLastDate;
                //End add by xuan.wu for Arrow adding
                //contactHistory
                List<SubContactHistory> ContactHisList = new List<SubContactHistory>();
                SubContactHistory ContactHis = new SubContactHistory();
                var OldConHisList = CommonRep.GetDbSet<ContactHistory>().Where(o => o.Deal == deal && o.CustomerNum == item && o.SiteUseId == SiteUsrId);
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
                    ContactHis.ContacterId = his.ContacterId;
                    ContactHis.Comments = his.Comments;
                    ContactHis.SiteUseId = his.SiteUseId;
                    ContactHisList.Add(ContactHis);
                    ihis++;
                }
                sendsoa.SubContactHistory = ContactHisList;
                //Legal
                List<SoaLegal> sublegalList = new List<SoaLegal>();
                SoaLegal sublegal = new SoaLegal();
                List<string> trackerStatusList = new List<string>() { "000", "001", "002", "003", "004", "005", "006", "007", "008", "009", "010", "011", "012", "015" };

                foreach (var legal in newCusAgingList)
                {

                    var invoice = newinvoiceList
                        .FindAll(m => m.CustomerNum == item && m.LegalEntity == legal.LegalEntity && m.SiteUseId == SiteUsrId);
                    var inv1 = invoice.Where(m => trackerStatusList.Contains(m.TrackStates) && m.Class == "INV").OrderBy(m => m.DueDate)
                       .Union(invoice.Where(m => trackerStatusList.Contains(m.TrackStates) && m.Class != "INV").OrderBy(m => m.DueDate)).ToList();

                    // vat
                    List<string> invNumList = inv1.Select(x => x.InvoiceNum).ToList();
                    List<int> invIdList = inv1.Select(x => x.Id).ToList();
                    var invVatList = CommonRep.GetDbSet<T_INVOICE_VAT>().Where(x => x.LineNumber == 1 && invNumList.Contains(x.Trx_Number)).Select(x => new { Trx_Number = x.Trx_Number, VATInvoiceDate = x.VATInvoiceDate }).ToList();

                    // ptp
                    var ptpQ = from ptp in CommonRep.GetQueryable<T_PTPPayment_Invoice>()
                               join pp in CommonRep.GetQueryable<T_PTPPayment>()
                               on ptp.PTPPaymentId equals pp.Id
                               where pp.PTPPaymentType == "PTP" && invIdList.Contains((int)ptp.InvoiceId)
                               group ptp by ptp.InvoiceId into g
                               select new { Key = g.Key, PTPId = g.Max(s => s.PTPPaymentId) };

                    var pt = (from pp in CommonRep.GetQueryable<T_PTPPayment>()
                              join ptp in ptpQ on pp.Id equals ptp.PTPId
                              select new { ptp.Key, pp.CustomerNum, pp.SiteUseId, pp.CreateTime, pp.PromiseDate, pp.Comments }).ToList();

                    // dispute
                    var disQ = from di in CommonRep.GetQueryable<DisputeInvoice>()
                               where invNumList.Contains(di.InvoiceId)
                               group di by di.InvoiceId into g
                               select new { g.Key, DisputeID = g.Max(s => s.DisputeId) };

                    var dis = (from q in CommonRep.GetQueryable<Dispute>()
                               join di in disQ on q.Id equals di.DisputeID
                               select new { di.Key, q.CustomerNum, q.SiteUseId, q.CreateDate, q.IssueReason, q.Comments, q.ActionOwnerDepartmentCode, q.Status }).ToList();

                    var adpt = CommonRep.GetQueryable<SysTypeDetail>().Where(x => x.TypeCode == "038").Select(x => new { DetailName = x.DetailName, DetailValue = x.DetailValue }).ToList();

                    var disReason = CommonRep.GetQueryable<SysTypeDetail>().Where(x => x.TypeCode == "025").Select(x => new { DetailName = x.DetailName, DetailValue = x.DetailValue }).ToList();

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
                    sublegal.SiteUseId = SiteUsrId;
                    var SN = SNList.Find(m => m.CustomerNum == item && m.SiteUseId == SiteUsrId);
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
                            subinvoice.OverdueReason = inv.OverdueReason;
                            subinvoice.PurchaseOrder = inv.PoNum;
                            subinvoice.SaleOrder = inv.SoNum;
                            subinvoice.RBO = inv.MstCustomer;
                            subinvoice.InvoiceCurrency = inv.Currency;
                            subinvoice.OriginalInvoiceAmount = inv.OriginalAmt.ToString();
                            subinvoice.OutstandingInvoiceAmount = inv.BalanceAmt;
                            subinvoice.DaysLate = (AppContext.Current.User.Now.Date - Convert.ToDateTime(inv.DueDate).Date).Days.ToString();
                            subinvoice.InvoiceTrack = !string.IsNullOrEmpty(inv.TrackStates) == false ? "" : Helper.CodeToEnum<TrackStatus>(inv.TrackStates).ToString().Replace("_", " ");
                            subinvoice.FinishStatus = inv.FinishedStatus;
                            subinvoice.Status = !String.IsNullOrEmpty(inv.States) ? Helper.CodeToEnum<InvoiceStatus>(inv.States).ToString().Replace("_", " ") : "";
                            //added by zhangYu 20151205 start
                            subinvoice.PtpDate = inv.PtpDate;
                            //added by zhangYu 20151205 End
                            subinvoice.DocumentType = inv.Class;
                            subinvoice.BalanceMemo = inv.BalanceMemo;
                            subinvoice.StandardInvoiceAmount = inv.StandardBalanceAmt;
                            //Start add by xuan.wu for Arrow adding
                            subinvoice.SiteUseId = inv.SiteUseId;
                            subinvoice.Sales = inv.Sales;
                            subinvoice.States = inv.States;
                            subinvoice.BALANCE_AMT = inv.BalanceAmt;
                            subinvoice.WoVat_AMT = inv.WoVat_AMT;
                            subinvoice.AgingBucket = inv.AgingBucket;
                            subinvoice.NotClear = (inv.NotClear == true ? "Lock" : "");
                            subinvoice.Ebname = inv.Eb;
                            subinvoice.COLLECTOR_NAME = cus.Collector; //inv.CollectorName;
                            subinvoice.DueDays = (AppContext.Current.User.Now.Date - Convert.ToDateTime(inv.DueDate).Date).Days;
                            subinvoice.InClass = inv.Class;
                            subinvoice.ConsignmentNumber = inv.ConsignmentNumber;
                            subinvoice.MemoExpirationDate = inv.MemoExpirationDate;                            //End add by xuan.wu for Arrow adding
                            subinvoice.IsExp = inv.MemoExpirationDate <= DateTime.Now ? 1 : 0;

                            var iVat = invVatList.Where(x => x.Trx_Number == inv.InvoiceNum).FirstOrDefault();
                            // Start add by albert 
                            if (iVat != null)
                            {
                                subinvoice.VatNum = iVat.Trx_Number;
                                subinvoice.VatDate = iVat.VATInvoiceDate;
                            }
                            subinvoice.TrackDate = inv.TRACK_DATE;

                            if (pt != null && pt.Count > 0)
                            {
                                var iPt = pt.Where(x => x.Key == inv.Id).FirstOrDefault();
                                if (iPt != null)
                                    subinvoice.PtpIdentifiedDate = iPt.CreateTime;
                            }

                            if (dis != null && dis.Count > 0)
                            {
                                var iDis = dis.Where(x => x.Key == inv.InvoiceNum).FirstOrDefault();
                                if (iDis != null)
                                {
                                    if (!string.IsNullOrEmpty(iDis.IssueReason))
                                    {
                                        var iRsn = disReason.Where(x => x.DetailValue == iDis.IssueReason).FirstOrDefault();
                                        if (iRsn != null)
                                            subinvoice.DisputeReason = iRsn.DetailName;
                                    }

                                    subinvoice.DisputeDate = iDis.CreateDate;
                                    if (!string.IsNullOrEmpty(iDis.ActionOwnerDepartmentCode))
                                    {
                                        var idpt = adpt.Where(x => x.DetailValue == iDis.ActionOwnerDepartmentCode).FirstOrDefault();
                                        if (idpt != null)
                                            subinvoice.OwnerDepartment = idpt.DetailName;
                                    }

                                    subinvoice.NextActionDate = iDis.CreateDate.AddDays(7);
                                }
                            }

                            //End add by albert

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
                    var tracking = calendar.GetTracking(reminders.FindAll(a => a.CustomerNum == item && string.IsNullOrEmpty(a.LegalEntity) && a.SiteUseId == SiteUsrId));
                    // 2. Other reminders
                    tracking = calendar.GetTracking(reminders.FindAll(a => a.CustomerNum == item && a.LegalEntity == legal.LegalEntity && a.SiteUseId == SiteUsrId), tracking);
                    // 3. Append other information shown in UI;
                    sublegal.SubTracking = tracking;
                    sublegal.SubInvoice = subinvoiceList.AsQueryable<SoaInvoice>().OrderByDescending(m => m.IsExp).ToList();
                    sublegalList.Add(sublegal);
                }
                sendsoa.SubLegal = sublegalList;
                sendsoaList.Add(sendsoa);
            }
            #endregion

            //**********************************WF Start ***********************************
            if (IsWF == "true")
            {
                if (Type == "create")
                {
                    if (GetPStatus(ColSoa, "Processing") == 0)
                    {
                        Wfchange("4", ColSoa, "start");
                    }
                }
            }
            return sendsoaList.AsQueryable<SendSoaHead>();
        }
        //End get all sendsoa by xuan.wu

        //review sendsoa :get custNums by taskNo
        public CollectorAlert GetSoa(string TaskNo)
        {
            string oper = AppContext.Current.User.Id.ToString();
            string deal = AppContext.Current.User.Deal.ToString();
            var alertList = CommonRep.GetQueryable<CollectorAlert>()
                .Where(m => m.ProcessId == TaskNo && m.PeriodId == CurrentPeriod).OrderByDescending(m => m.CreateDate).FirstOrDefault();
            return alertList;
        }

        //get a soa for a status
        public CollectorAlert GetStatus(string ReferenceNo)
        {
            string oper = AppContext.Current.User.Id.ToString();
            string deal = AppContext.Current.User.Deal.ToString();
            var alertList = CommonRep.GetQueryable<CollectorAlert>()
                .Where(m => m.ReferenceNo == ReferenceNo
                && m.Status != "Cancelled"
                && m.AlertType == 1
                ).OrderByDescending(m => m.CreateDate).FirstOrDefault();
            return alertList;
        }

        //get Invoice Log
        public IEnumerable<InvoiceLog> GetInvLog(string InvNum)
        {
            var invList = CommonRep.GetQueryable<InvoiceLog>()
                .Where(o => o.InvoiceId == InvNum && o.Deal == AppContext.Current.User.Deal).Select(o => o).OrderByDescending(m => m.LogDate).ToList();
            List<InvoiceLog> newLogList = new List<InvoiceLog>();
            InvoiceLog log = new InvoiceLog();
            MailTmp mail = new MailTmp();
            Call call = new Call();
            foreach (var inv in invList)
            {
                log = new InvoiceLog();
                mail = new MailTmp();
                call = new Call();
                string related = "";
                //6:breakPtp 8:HoldCustomer7:changeStatus
                if ((inv.LogType == "1" || inv.LogType == "3" || inv.LogType == "4" || inv.LogType == "5" || inv.LogType == "6" || inv.LogType == "7" || inv.LogType == "8" || inv.LogType == "9") && inv.ProofId != null)
                {
                    mail = CommonRep.GetDbSet<MailTmp>().Where(o => o.MessageId == inv.ProofId).FirstOrDefault();
                    if (mail != null)
                    {
                        related = mail.From + " " + mail.Subject + " " + mail.CreateTime.ToString().Replace("T", " ");
                    }
                    else
                    {
                        related = "";
                    }
                }
                else if (inv.LogType == "2" && inv.ProofId != null)
                {
                    call = CommonRep.GetDbSet<Call>().Where(o => o.ContactId == inv.ProofId).FirstOrDefault();
                    if (call != null)
                    {
                        related = "Detail";
                    }
                    else
                    {
                        related = "";
                    }
                }
                else
                {
                    related = "";
                }
                log.Id = inv.Id;
                log.Deal = inv.Deal;
                log.CustomerNum = inv.CustomerNum;
                log.InvoiceId = inv.InvoiceId;
                log.LogDate = inv.LogDate;
                log.LogPerson = inv.LogPerson;
                log.LogAction = inv.LogAction;
                log.LogType = inv.LogType;
                log.OldStatus = !string.IsNullOrEmpty(inv.OldTrack) == false ? "" : Helper.CodeToEnum<InvoiceStatus>(inv.OldStatus).ToString().Replace("_", " ");
                log.NewStatus = !string.IsNullOrEmpty(inv.OldTrack) == false ? "" : Helper.CodeToEnum<InvoiceStatus>(inv.NewStatus).ToString().Replace("_", " ");
                log.OldTrack = !string.IsNullOrEmpty(inv.OldTrack) == false ? "" : Helper.CodeToEnum<TrackStatus>(inv.OldTrack).ToString().Replace("_", " ");
                log.NewTrack = !string.IsNullOrEmpty(inv.NewTrack) == false ? "" : Helper.CodeToEnum<TrackStatus>(inv.NewTrack).ToString().Replace("_", " ");
                log.ContactPerson = inv.ContactPerson;
                log.ProofId = inv.ProofId;
                log.Discription = inv.Discription;
                log.RelatedEmail = related;
                newLogList.Add(log);
            }

            return newLogList.AsQueryable();
        }

        //get Invoice Detail
        public IEnumerable<T_Invoice_Detail> GetInvoiceDetail(string InvNum)
        {
            string strInvNumNoLine = InvNum.Replace("-", "");
            return CommonRep.GetQueryable<T_Invoice_Detail>()
                .Where(o => o.InvoiceNumber == InvNum || o.InvoiceNumber == strInvNumNoLine).Select(o => o).OrderBy(m => m.InvoiceLineNumber).ToList();
        }

        //get WF status
        /// <summary>
        /// Whether Task Exist in WF DB 
        /// </summary>
        /// <param name="referenceNo">CustNums</param>
        /// <param name="status">status in task</param>
        /// <returns></returns>
        public int GetPStatus(string referenceNo, string status)
        {
            IWorkflowService wfservice = SpringFactory.GetObjectImpl<IWorkflowService>("WorkflowService");
            string deal = AppContext.Current.User.Deal.ToString();
            string oper = AppContext.Current.User.Id.ToString();
            string eid = AppContext.Current.User.EID.ToString();
            string[] cus = referenceNo.Split(',');
            string DefaultCus = cus[0].ToString();
            var alert = CommonRep.GetQueryable<CollectorAlert>()
                        .Where(o => o.Deal == deal && o.CustomerNum == DefaultCus && o.AlertType == 1 && o.Status != "Cancelled"
                        ).ToList()
                        .FirstOrDefault();
            if (string.IsNullOrEmpty(alert.CauseObjectNumber))
            {
                return 0;
            }
            else
            {
                if (wfservice.GetProcessStatus("4", alert.CauseObjectNumber, oper, "Processing").Count > 0)
                {
                    return 1;
                }
                else
                {
                    return 0;
                };
            }
        }

        //WFchange
        public void Wfchange(string processDefinationId, string referenceNo, string type)
        {
            IWorkflowService wfservice = SpringFactory.GetObjectImpl<IWorkflowService>("WorkflowService");
            string oper = AppContext.Current.User.Id.ToString();
            string deal = AppContext.Current.User.Deal.ToString();
            string eid = AppContext.Current.User.EID.ToString();
            string[] cus = referenceNo.Split(',');
            string causeObjectNum = Guid.NewGuid().ToString();
            string DefaultCus = cus[0].ToString();
            var alert = new CollectorAlert();
            if (type != "start")
            {
                alert = CommonRep.GetQueryable<CollectorAlert>()
                        .Where(o => o.Deal == deal && o.CustomerNum == DefaultCus && o.AlertType == 1 && o.Status != "Cancelled"
                        ).ToList()
                        .OrderByDescending(o => o.PeriodId).FirstOrDefault();
            }
            if (type == "start")
            {
                if (IsWF == "true")
                {
                    var task = wfservice.StartProcess(processDefinationId, causeObjectNum, oper);
                    wfservice.AcceptTask(task.TaskId, causeObjectNum, oper);
                    string processid = GetProcessId(task.TaskId);

                    UpdateAlert(cus, task.TaskId.ToString(), processid, causeObjectNum, "Processing", 2);
                }
            }
            else if (type == "restart")
            {
                //2016-01-12 把Finish的Task重新开启
                UpdateAlert(cus, "", "", "", "Restart", 2);
            }
            else if (type == "cancel")
            {
                if (IsWF == "true")
                {
                    wfservice.CancelTask(processDefinationId, alert.CauseObjectNumber, oper, alert.TaskId);
                    UpdateAlert(cus, "", "", "", "Cancel", 2);
                }
            }
            else if (type == "pause")
            {
                if (IsWF == "true")
                {
                    wfservice.PauseProcess(processDefinationId, alert.CauseObjectNumber, oper, alert.TaskId);
                    UpdateAlert(cus, "", "", "", "Pause", 2);
                }
            }
            else if (type == "resume")
            {
                if (IsWF == "true")
                {
                    wfservice.ResumeProcess(processDefinationId, alert.CauseObjectNumber, oper, alert.TaskId);
                    UpdateAlert(cus, "", "", "", "Resume", 2);
                }
            }
            else if (type == "finish")
            {
                if (IsWF == "true")
                {
                    wfservice.FinishProcess(processDefinationId, alert.CauseObjectNumber, oper, alert.TaskId);
                    UpdateAlert(cus, "", "", "", "Finish", 2);
                }
            }
        }

        //Get ProcessId
        public string GetProcessId(long taskid)
        {
            IWorkflowService wfservice = SpringFactory.GetObjectImpl<IWorkflowService>("WorkflowService");
            string oper = AppContext.Current.User.Id.ToString();
            //get processinstanceid
            List<string> status = new List<string>();
            status.Add("Processing");
            var task = wfservice.GetMyTaskList(oper, status).Find(m => m.Id == taskid);
            string processid = task.ProcessInstance_Id.ToString();

            return processid;
        }

        //GetTask
        public int GetTask(string reNo)
        {
            IWorkflowService wfservice = SpringFactory.GetObjectImpl<IWorkflowService>("WorkflowService");
            string oper = AppContext.Current.User.Id.ToString();
            List<string> status = new List<string>();
            status.Add("Waiting");
            if (wfservice.GetMyTaskList(oper, status).Find(m => m.CauseObjectNumber == reNo) != null)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        //save comment
        public void SaveComm(int invid, string comm, string commDate)
        {
            InvoiceAging invoice = CommonRep.GetQueryable<InvoiceAging>().Where(m => m.Id == invid).FirstOrDefault();

            // INSERT T_INVOICE_AGING_ExpirationDateHis
            if (!Convert.ToDateTime(invoice.MemoExpirationDate).ToString("yyyy-MM-dd").Equals(commDate))
            {
                T_INVOICE_AGING_ExpirationDateHis agingExpDateHis = new T_INVOICE_AGING_ExpirationDateHis();

                agingExpDateHis.InvID = invoice.Id;
                agingExpDateHis.OldMemoExpirationDate = invoice.MemoExpirationDate;
                if (string.IsNullOrEmpty(commDate))
                {
                    agingExpDateHis.NewMemoExpirationDate = null;
                }
                else
                {
                    agingExpDateHis.NewMemoExpirationDate = Convert.ToDateTime(commDate);
                }
                agingExpDateHis.UserId = AppContext.Current.User.EID; //当前用户ID
                agingExpDateHis.ChangeDate = DateTime.Now;
                CommonRep.Add(agingExpDateHis);
            }

            // UPDATE InvoiceAging
            invoice.BalanceMemo = comm;
            if (string.IsNullOrEmpty(commDate))
            {
                invoice.MemoExpirationDate = null;
            }
            else
            {
                invoice.MemoExpirationDate = Convert.ToDateTime(commDate);
            }

            //CommonRep.

            CommonRep.Commit();
        }

        //batch save comment
        public void BatchSaveComm(string invids, string comm, string commDate)
        {
            string[] strInvidArray = invids.Split(',');
            List<int> intInvidList = Array.ConvertAll<string, int>(strInvidArray, s => int.Parse(s)).ToList<int>();
            var invoiceList = CommonRep.GetQueryable<InvoiceAging>().Where(m => intInvidList.Contains(m.Id));
            foreach (var id in intInvidList)
            {
                InvoiceAging invoice = invoiceList.Where(m => m.Id == id).FirstOrDefault();

                // INSERT T_INVOICE_AGING_ExpirationDateHis
                if (!Convert.ToDateTime(invoice.MemoExpirationDate).ToString("yyyy-MM-dd").Equals(commDate))
                {
                    T_INVOICE_AGING_ExpirationDateHis agingExpDateHis = new T_INVOICE_AGING_ExpirationDateHis();

                    agingExpDateHis.InvID = invoice.Id;
                    agingExpDateHis.OldMemoExpirationDate = invoice.MemoExpirationDate;
                    if (string.IsNullOrEmpty(commDate))
                    {
                        agingExpDateHis.NewMemoExpirationDate = null;
                    }
                    else
                    {
                        agingExpDateHis.NewMemoExpirationDate = Convert.ToDateTime(commDate);
                    }
                    agingExpDateHis.UserId = AppContext.Current.User.EID; //当前用户ID
                    agingExpDateHis.ChangeDate = DateTime.Now;
                    CommonRep.Add(agingExpDateHis);
                }


                if (!string.IsNullOrEmpty(comm))
                {
                    invoice.BalanceMemo = comm + Environment.NewLine + invoice.BalanceMemo;
                }
                else
                {
                    invoice.BalanceMemo = comm;
                }
                if (string.IsNullOrEmpty(commDate))
                {
                    invoice.MemoExpirationDate = invoice.MemoExpirationDate;
                }
                else
                {
                    invoice.MemoExpirationDate = Convert.ToDateTime(commDate);
                }
            }
            CommonRep.Commit();
        }

        //save notes
        public void SaveNotes(string Cus, string SpNotes)
        {
            Customer cus = CommonRep.GetDbSet<Customer>()
                .Where(m => m.Deal == AppContext.Current.User.Deal && m.CustomerNum == Cus && m.IsActive == true).FirstOrDefault();
            cus.SpecialNotes = SpNotes;
            CommonRep.Commit();
        }

        //batch send soa
        public void BatchSoa(string Cusnums, string siteUseId)
        {
            string deal = AppContext.Current.User.Deal.ToString();
            string eid = AppContext.Current.User.EID.ToString();
            MailService mailservice = SpringFactory.GetObjectImpl<MailService>("MailService");
            ICustomerService custservice = SpringFactory.GetObjectImpl<ICustomerService>("CustomerService");
            ContactService conservice = SpringFactory.GetObjectImpl<ContactService>("ContactService");
            InvoiceService invservice = SpringFactory.GetObjectImpl<InvoiceService>("InvoiceService");
            //1.get batch customer list
            List<string> AllCustNumList = Cusnums.Split(',').ToList();

            //20160406
            var CustList = GetNoPaging("batch").Where(m => AllCustNumList.Contains(m.CustomerNum)).ToList();

            //Rate
            var rateList = CommonRep.GetQueryable<RateTran>()
                .Where(o => o.Deal == AppContext.Current.User.Deal && o.EffectiveDate <= AppContext.Current.User.Now.Date && o.ExpiredDate >= AppContext.Current.User.Now.Date).ToList();
            //all invoice
            List<string> TrackStatesList = new List<string>() { "000", "001", "002", "003", "004", "005", "006", "015" };
            var AllInvList = CommonRep.GetDbSet<InvoiceAging>()
            .Where(o => o.Deal == deal && AllCustNumList.Contains(o.CustomerNum) && TrackStatesList.Contains(o.TrackStates)).ToList();

            //loop customer list by groupcode
            IEnumerable<IGrouping<string, SoaDto>> query = CustList.GroupBy(m => m.BillGroupCode);
            foreach (IGrouping<string, SoaDto> info in query)
            {
                List<SoaDto> CusGroupList = info.ToList<SoaDto>();

                //get cusnums
                string cusnums = "";
                List<int> invids = new List<int>();
                foreach (var item in CusGroupList)
                {
                    cusnums += item.CustomerNum + ",";
                }
                cusnums = cusnums.Substring(0, cusnums.Length - 1);
                //get invoiceids

                //=========updated by alex body中显示附件名+Currency=== $scope.inv 追加 ======

                //2.get mailinstance
                string subject = "SOA";
                int[] invIds = null;
                //3.get contact into mailinstance.to
                string to = string.Empty;
                //########## update by pxc 增加cc ######################## s
                string cc = string.Empty;
                List<string> contactorsTo = conservice.GetContactsByCustomers(cusnums, siteUseId).Where(m => m.ToCc == "1").Select(m => m.EmailAddress).Distinct().ToList();
                List<string> contactorsCC = conservice.GetContactsByCustomers(cusnums, siteUseId).Where(m => m.ToCc == "2").Select(m => m.EmailAddress).Distinct().ToList();
                if (contactorsTo.Count > 0)
                {
                    foreach (var conto in contactorsTo)
                    {
                        to += conto + ";";
                    }
                    to = to.Substring(0, to.Length - 1);
                }
                if (contactorsCC.Count > 0)
                {
                    foreach (var concc in contactorsCC)
                    {
                        cc += concc + ";";
                    }
                    cc = cc.Substring(0, cc.Length - 1);
                }
                //########## update by pxc 增加cc ######################## e
                //=========================================================================

                //4.get attachment into mailinstance.attachment
                string[] cus = cusnums.Split(',');
                decimal blc = 0;
                foreach (var c in cus)
                {
                    blc = 0;
                    //=========updated by alex body中显示附件名+Currency=== $scope.inv 追加 ======
                    //2016-01-07 update
                    subject += "-" + CusGroupList.Find(m => m.CustomerNum == c).CustomerNum + "-" + CusGroupList.Find(m => m.CustomerNum == c).CustomerName;

                    AllInvList.FindAll(m => m.CustomerNum == c).ForEach(m =>
                    {
                        invids.Add(m.Id);
                        if (m.Currency != "USD")
                        {
                            blc += Convert.ToDecimal((rateList.Find(n => n.ForeignCurrency == m.Currency).Rate == null ? 1 : rateList.Find(n => n.ForeignCurrency == m.Currency).Rate) * m.BalanceAmt);
                        }
                        else
                        {
                            blc += Convert.ToDecimal(m.BalanceAmt);
                        }
                    });
                    invIds = invids.ToArray();
                    //2016-01-07 delete
                    //===========================================================================
                }
                //=========updated by alex body中显示附件名+Currency=== $scope.inv 追加 ======
                string attInfo = string.Empty;
                MailTmp instance = GetNewMailInstance(CusGroupList[0].CustomerNum, "", "", siteUseId, invids, "", "", "", "", "");
                instance.Subject = subject;
                //########## update by pxc 增加MessageId ######################## 
                instance.MessageId = Guid.NewGuid().ToString();
                instance.To = to;
                //########## update by pxc 增加cc ######################## s
                instance.Cc = cc;
                //########## update by pxc 增加cc ######################## e
                instance.invoiceIds = invIds;

                //########## update by pxc 新MailTmp 删除 ######################## 
                //instance.Bussiness_Reference = cusnums;
                //########## update by pxc 增加Mail customer ######################## s
                foreach (var c in cus)
                {
                    CustomerMail cm = new CustomerMail();
                    cm.MessageId = instance.MessageId;
                    cm.CustomerNum = c;
                    instance.CustomerMails.Add(cm);
                }
                //########## update by pxc 增加Mail customer ######################## e
                instance.MailType = "001,SOA";

                //==========================================================================

                //5.call mailservice SendMail
                try
                {
                    //6.save mail contacthistory  invoicelog
                    if (sendSoaSaveInfoToDB(instance, invIds.ToList()) == 1)
                    {
                        //alert update 
                        UpdateAlert(cus, "", "", "", "Finish", 1);
                    }
                }
                catch (MailServiceException ex)
                {
                    //alert update 
                    UpdateAlert(cus, ex.Message.ToString(), "", "", "Failed", 1);
                }
                catch (Exception ex)
                {
                    //alert update 
                    UpdateAlert(cus, ex.ToString(), "", "", "Failed", 1);
                }

            }

        }

        //update alert
        /// <summary>
        /// update alert when after send soa or pause/resume/cancel/finish workflow
        /// </summary>
        /// <param name="cusnums">array custnums</param>
        /// <param name="TaskId">task id (if workflow )</param>
        /// <param name="ProcessId">processId ( if workflow )</param>
        /// <param name="status">status in alert</param>
        /// <param name="type">batch or execute: 1:batch;2:execute</param>
        public void UpdateAlert(string[] cusnums, string TaskId, string ProcessId, string causeObjectNum, string status, int type)
        {
            string deal = AppContext.Current.User.Deal.ToString();
            string eid = AppContext.Current.User.EID.ToString();
            // pxc update 20160311
            List<CollectorAlert> alertList = new List<CollectorAlert>();
            //type = 1 : batch
            if (type == 1)
            {
                if (status == "Finish")
                {
                    List<string> strCondition = new List<string>();
                    var DunBatchType = type.ToString();
                    var DunAlertType = "";

                    foreach (var cus in cusnums)
                    {
                        CollectorAlert alert = new CollectorAlert();
                        // pxc update 20160311
                        alert = CommonRep.GetQueryable<CollectorAlert>().Where(m => m.Deal == deal && m.AlertType == 1 && m.Status != "Cancelled" && m.CustomerNum == cus && m.Status != "Finish"
                        ).FirstOrDefault();
                        alert.ReferenceNo = string.Join(",", cusnums);
                        alert.Status = status;
                        alert.ActionDate = AppContext.Current.User.Now.Date;
                        strCondition.Add(cus);
                        if (string.IsNullOrEmpty(DunAlertType))
                        {
                            DunAlertType = alert.AlertType.ToString();
                        }
                    }
                    CreateDunning(strCondition, DunBatchType, DunAlertType);
                }
                else if (status == "Failed")
                {
                    foreach (var cus in cusnums)
                    {
                        CollectorAlert alert = new CollectorAlert();
                        // pxc update 20160311
                        alert = CommonRep.GetQueryable<CollectorAlert>().Where(m => m.Deal == deal && m.AlertType == 1 && m.Status != "Cancelled" && m.CustomerNum == cus && m.Status != "Finish"
                        ).FirstOrDefault();
                        alert.FailedReason = TaskId;
                    }
                }
            }
            //type = 2 : execute
            else if (type == 2)
            {
                if (status == "Processing")
                {
                    foreach (var cus in cusnums)
                    {
                        CollectorAlert alert = new CollectorAlert();
                        // pxc update 20160311
                        alert = CommonRep.GetQueryable<CollectorAlert>().Where(m => m.Deal == deal && m.AlertType == 1 && m.Status != "Cancelled" && m.CustomerNum == cus && m.Status != "Finish"
                        ).FirstOrDefault();
                        alert.TaskId = TaskId;
                        alert.ProcessId = ProcessId;
                        alert.ReferenceNo = string.Join(",", cusnums);
                        alert.CauseObjectNumber = causeObjectNum;
                        alert.Status = status;
                    }
                }
                else if (status == "Pause" || status == "Resume" || status == "Finish")
                {
                    List<string> strCondition = new List<string>();
                    var DunTask = "";
                    var DunBatchType = type.ToString();
                    var DunAlertType = "";
                    foreach (var cus in cusnums)
                    {
                        CollectorAlert alert = new CollectorAlert();
                        // pxc update 20160311
                        alert = CommonRep.GetQueryable<CollectorAlert>().Where(m => m.Deal == deal && m.AlertType == 1 && m.Status != "Cancelled" && m.CustomerNum == cus && m.Status != "Finish"
                        ).FirstOrDefault();
                        alert.Status = status;
                        if (status == "Finish")
                        {
                            alert.ActionDate = AppContext.Current.User.Now.Date;
                        }
                        //Dunning para
                        if (status == "Finish" && string.IsNullOrEmpty(DunTask))
                        {
                            DunTask = alert.TaskId;
                        }
                        if (status == "Finish" && string.IsNullOrEmpty(DunAlertType))
                        {
                            DunAlertType = alert.AlertType.ToString();
                        }
                        //2016-01-12  pxc update
                        WorkflowHistoryService wfhser = SpringFactory.GetObjectImpl<WorkflowHistoryService>("WorkflowHistoryService");
                        wfhser.AddOne(alert);
                    }
                    if (status == "Finish")
                    {
                        //Dunning
                        strCondition.Add(DunTask);
                        CreateDunning(strCondition, DunBatchType, DunAlertType);
                    }
                }
                else if (status == "Cancel" || status == "Restart")
                {
                    foreach (var cus in cusnums)
                    {
                        CollectorAlert alert = new CollectorAlert();
                        if (status == "Cancel")
                        {
                            // pxc update 20160311
                            alert = CommonRep.GetQueryable<CollectorAlert>().Where(m => m.Deal == deal && m.AlertType == 1 && m.Status != "Cancelled" && m.CustomerNum == cus && m.Status != "Finish"
                            ).FirstOrDefault();
                        }
                        else if (status == "Restart")
                        {
                            // pxc update 20160311
                            alert = CommonRep.GetQueryable<CollectorAlert>().Where(m => m.Deal == deal && m.AlertType == 1 && m.Status != "Cancelled" && m.CustomerNum == cus && m.Status == "Finish"
                            ).FirstOrDefault();
                        }
                        alert.TaskId = "";
                        alert.ProcessId = "";
                        alert.ReferenceNo = "";
                        alert.CauseObjectNumber = "";
                        alert.Status = "Initialized";
                    }
                }
            }
            CommonRep.Commit();
        }

        public IEnumerable<CustomerPaymentBank> GetSoaPayment(string CustNumFPb)
        {
            string deal = AppContext.Current.User.Deal;
            var pbList = CommonRep.GetQueryable<CustomerPaymentBank>()
                .Where(o => o.Deal == deal && o.CustomerNum == CustNumFPb).Select(o => o);
            pbList.ToList().ForEach(pb =>
           {
               pb.InUse = pb.Flg == "1" ? "Valid" : "Invalid";
           });
            return pbList.AsQueryable();
        }

        public IEnumerable<CustomerPaymentCircle> GetSoaPaymentCircle(string CustNumFPc, string SiteUseIdFPc)
        {
            string deal = AppContext.Current.User.Deal;
            int i = 1;
            var pcList = CommonRep.GetQueryable<CustomerPaymentCircle>()
                .Where(o => o.Deal == deal && o.CustomerNum == CustNumFPc && o.SiteUseId == SiteUseIdFPc).Select(o => o);
            pcList.ToList().ForEach(pc =>
            {
                pc.sortId = i++;
                pc.weekDay = (pc.PaymentDay.HasValue ? pc.PaymentDay.Value.DayOfWeek.ToString() : "");
            });
            return pcList.AsQueryable();
        }

        //add by jiaxing for get contactdomain list
        public IEnumerable<ContactorDomain> GetSoaContactDomain(string CustNumFPd)
        {
            string deal = AppContext.Current.User.Deal;
            int i = 1;
            var pdList = CommonRep.GetQueryable<ContactorDomain>()
                .Where(o => o.Deal == deal && o.CustomerNum == CustNumFPd).Select(o => o);
            pdList.ToList().ForEach(pd =>
               {
                   pd.sortId = i++;
               });
            return pdList.AsQueryable();
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
                subConHisList = CommonRep.GetDbSet<ContactHistory>().Where(o => o.CustomerNum == item).ToList();
                subConHisList.ForEach(sb =>
                {
                    sb.sortId = i++;
                });
                ConHisList.AddRange(subConHisList);
            }
            return ConHisList.AsQueryable();
        }

        public int sendCaPmtMailSaveInfoToDB(MailTmp mailInstance, string strID)
        {
            try
            {
	            Helper.Log.Info("**************************** 000000 ***************************");
	            int flag = 0;//0:failed;1:succeed
	            MailService mailService = SpringFactory.GetObjectImpl<MailService>("MailService");

	            //2. SAVE MAIL_INFO TO T_Mail for each customer
	            List<MailTmp> mailList = new List<MailTmp>();
	            MailTmp saveMail = new MailTmp();
	            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
	            {
	                try
	                {
	                    //保存TMP邮件
	                    saveMail = mailService.SaveMailAsDraft(mailInstance);

	                    flag = 1;
	                }
	                catch (Exception e)
	                {
	                    if (e.InnerException != null && e.InnerException.InnerException != null && e.InnerException.InnerException.InnerException != null && e.InnerException.InnerException.InnerException.InnerException != null) {
	                        string strErrorMsg = e.InnerException.InnerException.InnerException.InnerException.Message;
	                        throw new OTCServiceException(strErrorMsg);
	                    } else if (e.InnerException != null && e.InnerException.InnerException != null && e.InnerException.InnerException.InnerException != null) {

	                        string strErrorMsg = e.InnerException.InnerException.InnerException.Message;
	                        throw new OTCServiceException(strErrorMsg);
	                    } else if (e.InnerException != null && e.InnerException.InnerException != null)
	                    {
	                    string strErrorMsg = e.InnerException.InnerException.Message;
	                    throw new OTCServiceException(strErrorMsg);
	                    }
	                    else if (e.InnerException != null)
	                    {
	                        string strErrorMsg = e.InnerException.Message;
	                        throw new OTCServiceException(strErrorMsg);
	                    }
	                }
	                finally
	                {
	                    CommonRep.Commit();
	                    scope.Complete();
	                }
	            }
	            if (flag == 1)
	            {
	                //更新 T_CA_MailAlert 及 T_CA_Task表 (此处没有写在邮件保存同一事务中)
	                List<string> listSQL = new List<string>();
	                strID = strID.Replace(",", "','");
	                string strSQLCaPmtMail = string.Format("UPDATE T_CA_MailAlert SET STATUS = 'Processing', SendTime = GETDATE(), MessageId = '{0}' WHERE ID in ('{1}')", saveMail.MessageId, strID);
	                listSQL.Add(strSQLCaPmtMail);
	                SqlHelper.ExcuteListSql(listSQL);
	            }

	            return saveMail.Id;
            }
            catch (Exception ex) {
                throw ex;
            }
        }

        //added by zhangYu
        public int sendSoaSaveInfoToDB(MailTmp mailInstance, List<int> invs, int inputAlertType = -1, string Collector = "", string legalEntity = "",string CustomerNum = "", string SiteUseId = "", string toTitle = "", string toName = "", string ccTitle = "", int periodId = 0, string TempleteLanguage = "")
        {
            int flag = 0;//0:failed;1:succeed

            if (CustomerNum == null) { CustomerNum = ""; }
            string[] CustomerNumList = CustomerNum.Split(',');

            //1.SEND MAIL
            MailService mailService = SpringFactory.GetObjectImpl<MailService>("MailService");

            //2. SAVE MAIL_INFO TO T_Mail for each customer
            List<MailTmp> mailList = new List<MailTmp>();
            MailTmp saveMail = new MailTmp();
            //####################### partials:MailTmp return List #######################

            List<ContactHistory> ConHisList = new List<ContactHistory>();

            //invoice
            var UpdateInvSql = string.Format("");
            List<InvoiceLog> InvLogList = new List<InvoiceLog>();

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
            {
                // Send mail logic.
                string strErrorMsg = "";
                try
                {
                    mailService.SendMail(mailInstance, TempleteLanguage, Collector);

                    List<CollectorAlert> nAlert = new List<CollectorAlert>();
                    if (TempleteLanguage == "006" || TempleteLanguage == "007" || TempleteLanguage == "0071" || TempleteLanguage == "009")
                    {
                        nAlert = CommonRep.GetQueryable<CollectorAlert>().Where(c => c.Eid == Collector &&
                                                                                    c.PeriodId == periodId &&
                                                                                    c.AlertType == inputAlertType &&
                                                                                    CustomerNumList.Contains(c.CustomerNum) &&
                                                                                    c.ToTitle == toTitle &&
                                                                                    c.ToName == toName &&
                                                                                    c.Status == "Initialized").OrderByDescending(o => o.Id).ToList();
                    }
                    else if (TempleteLanguage == "001")
                    {
                        Helper.Log.Info("************* Collector: *************" + Collector);
                        Helper.Log.Info("************* periodId: *************" + periodId);
                        Helper.Log.Info("************* inputAlertType: *************" + inputAlertType);
                        Helper.Log.Info("************* legalEntity: *************" + legalEntity);
                        Helper.Log.Info("************* CustomerNumList: *************" + CustomerNumList);
                        if (string.IsNullOrEmpty(legalEntity) || CustomerNumList == null || CustomerNumList.Count() == 0)
                        {
                            nAlert = CommonRep.GetQueryable<CollectorAlert>().Where(c => c.Eid == Collector &&
                                                                                        c.PeriodId == periodId &&
                                                                                        c.AlertType == inputAlertType &&
                                                                                        c.ToTitle == toTitle &&
                                                                                        c.ToName == toName &&
                                                                                        c.Status == "Initialized").OrderByDescending(o => o.Id).ToList();
                        }
                        else {
                            nAlert = CommonRep.GetQueryable<CollectorAlert>().Where(c => c.Eid == Collector &&
                                                                                        c.PeriodId == periodId &&
                                                                                        c.AlertType == inputAlertType &&
                                                                                        c.LegalEntity == legalEntity &&
                                                                                        CustomerNumList.Contains(c.CustomerNum) &&
                                                                                        c.ToTitle == toTitle &&
                                                                                        //c.ToName == toName &&
                                                                                        c.Status == "Initialized").OrderByDescending(o => o.Id).ToList();
                        }
                        if (nAlert == null || nAlert.Count == 0) {
                            Helper.Log.Info("************* nAlert is null *************");
                        }
                    }
                    else
                    {
                        nAlert = CommonRep.GetQueryable<CollectorAlert>().Where(c => c.Eid == Collector &&
                                                                                c.PeriodId == periodId &&
                                                                                c.AlertType == inputAlertType &&
                                                                                c.ToTitle == toTitle &&
                                                                                (c.SiteUseId == SiteUseId || SiteUseId == "") &&
                                                                                c.ToName == toName &&
                                                                                c.Status == "Initialized").OrderByDescending(o => o.Id).ToList();
                    }

                    if (nAlert != null)
                    {
                        foreach (CollectorAlert alert in nAlert)
                        {
                            ContactHistory his = new ContactHistory();
                            his.LegalEntity = alert.LegalEntity;
                            his.CustomerNum = alert.CustomerNum;
                            his.ContactType = "Mail";
                            his.CollectorId = "BATCH_USER";
                            his.ContacterId = mailInstance.To;
                            his.ContactDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:00"));
                            his.Comments = "Wave" + alert.AlertType;
                            his.Deal = alert.Deal;
                            his.AlertId = alert.Id;
                            his.ContactId = mailInstance.MessageId;
                            his.LastUpdatePerson = "BATCH_USER";
                            his.LastUpdateTime = DateTime.Now;
                            his.SiteUseId = alert.SiteUseId;
                            his.Region = alert.Region;
                            his.ToTitle = alert.ToTitle;
                            his.ToName = alert.ToName;
                            his.CCTitle = alert.CCTitle;
                            ConHisList.Add(his);
                        }
                        CommonRep.AddRange(ConHisList);
                    }


                    if (inputAlertType != -1)
                    {
                        if (inputAlertType == 5)
                        {
                            if (invs != null && invs.Count > 0)
                            {
                                var notPMT = CommonRep.GetQueryable<SysTypeDetail>().Where(o => o.TypeCode == "048").Select(o => o.DetailName).DefaultIfEmpty().ToList();

                                List<InvoiceAging> aging = CommonRep.GetQueryable<InvoiceAging>().Where(c => invs.Contains(c.Id) && c.Class == "PMT" && !notPMT.Contains(c.CreditTrem)).ToList();
                                if (aging != null)
                                {
                                    foreach (InvoiceAging a in aging)
                                    {
                                        a.TRACK_DATE = DateTime.Now;
                                        a.hasPmt = "1";
                                        a.PERIOD_ID = Convert.ToInt32(DateTime.Now.ToString("yyyyMMdd"));
                                        CommonRep.Save(a);
                                    }
                                }
                            }

                            if (nAlert != null)
                            {
                                foreach (CollectorAlert alert in nAlert)
                                {
                                    alert.Status = "Finish";
                                    CommonRep.Save(alert);
                                }
                            }
                        }
                        else
                        {
                            if (invs != null)
                            {
                                if (inputAlertType == 0 || inputAlertType == 1 || inputAlertType == 2)
                                {
                                    List<InvoiceAging> aging = CommonRep.GetQueryable<InvoiceAging>().Where(c => invs.Contains(c.Id)).ToList();
                                    foreach (InvoiceAging a in aging)
                                    {
                                        a.PERIOD_ID = periodId;
                                        CommonRep.Save(a);
                                    }
                                }

                                foreach (CollectorAlert alert in nAlert)
                                {
                                    Customer nCustomer = CommonRep.GetQueryable<Customer>().Where(c => c.CustomerNum == alert.CustomerNum && c.SiteUseId == alert.SiteUseId).FirstOrDefault();
                                    if (nCustomer != null)
                                    {
                                        nCustomer.LastSendDate = DateTime.Now;
                                        CommonRep.Save(nCustomer);
                                    }
                                    else
                                    {
                                        Helper.Log.Info("not find customer: siteuseid-" + SiteUseId + ", customerNo-" + CustomerNum);
                                    }
                                    alert.MessageId = mailInstance.MessageId;
                                    alert.Status = "Finish";
                                    CommonRep.Save(alert);
                                }
                            }
                        }
                    }
                    flag = 1;
                }
                catch (Exception e)
                {
                    if (e.InnerException != null && e.InnerException.InnerException != null && e.InnerException.InnerException.InnerException != null && e.InnerException.InnerException.InnerException.InnerException != null)
                    {
                        strErrorMsg = e.InnerException.InnerException.InnerException.InnerException.Message;
                    }
                    else if (e.InnerException != null && e.InnerException.InnerException.InnerException != null)
                    {
                        strErrorMsg = e.InnerException.InnerException.InnerException.Message;
                    } else if (e.InnerException != null && e.InnerException.InnerException != null)
                    {
                        strErrorMsg = e.InnerException.InnerException.Message;
                    }  else if (e.InnerException != null)
                    {
                        strErrorMsg = e.InnerException.Message;
                    }
                    List<CollectorAlert> nAlert = CommonRep.GetQueryable<CollectorAlert>().Where(c => c.Eid == Collector &&
                                                                                                    c.AlertType == inputAlertType &&
                                                                                                    c.ToTitle == toTitle &&
                                                                                                    c.ToName == toName &&
                                                                                                    c.Status == "Initialized").OrderByDescending(o => o.Id).ToList();
                    foreach (CollectorAlert alert in nAlert)
                    {
                        if (strErrorMsg.Length > 2000)
                        {
                            alert.Comment = strErrorMsg.Substring(2000);
                        }
                        else
                        {
                            alert.Comment = strErrorMsg;
                        }
                        CommonRep.Save(alert);
                    }
                    throw new OTCServiceException(strErrorMsg);
                }
                finally
                {
                    CommonRep.Commit();
                    scope.Complete();
                }
            }

            return flag;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="language"></param>
        /// <param name="type"></param>
        /// <param name="collectorEID"></param>
        /// <returns></returns>
        public MailTmp GetNewMailInstance(string customerNums, string siteUseId, string templateType, string templatelang, List<int> intIds, string Collector = "", string ToTitle = "", string ToName = "", string CCTitle = "", string ResponseDate = "", string Region = "", string indexFile = "", string fileType = "XLS")
        {
            if (string.IsNullOrEmpty(customerNums)) { customerNums = ""; }
            if (string.IsNullOrEmpty(siteUseId)) { siteUseId = ""; }

            //根据某一张发票获得LegalEntity(补丁)
            int invIdOne = Convert.ToInt32(intIds[0]);
            string strLegalEntity = (from inv in CommonRep.GetQueryable<InvoiceAging>()
                                     where inv.Id == invIdOne
                                     select inv.LegalEntity).FirstOrDefault();

            List<string> siteUseIdList = siteUseId.Split(',').ToList();
            List<string> customerNumList = customerNums.Split(',').ToList();

            MailTmp res = null;
            string attachment = "";

            if (string.IsNullOrEmpty(Collector))
            {
                Collector = (from c in CommonRep.GetQueryable<Customer>()
                             where (customerNumList.Contains(c.CustomerNum) || customerNums == "") && (siteUseIdList.Contains(c.SiteUseId) || c.SiteUseId == siteUseId)
                             select c.Collector).FirstOrDefault();
            }
            string CustomerNameNoSite = (from c in CommonRep.GetQueryable<Customer>()
                                         where (customerNumList.Contains(c.CustomerNum) || customerNums == "") && ( siteUseId == "" || siteUseIdList.Contains(c.SiteUseId))
                                         select c.CustomerName).FirstOrDefault();
            string CustomerGroupName = (from c in CommonRep.GetQueryable<Customer>()
                                        where (customerNumList.Contains(c.CustomerNum) || customerNums == "") && (siteUseId == "" || siteUseIdList.Contains(c.SiteUseId))
                                        select c.GroupName).FirstOrDefault();
            if (string.IsNullOrEmpty(CustomerGroupName)) { CustomerGroupName = ""; }
            string CustomerName = (from c in CommonRep.GetQueryable<Customer>()
                                   where (customerNumList.Contains(c.CustomerNum) || customerNums == "") &&  (siteUseId == "" || siteUseIdList.Contains(c.SiteUseId))
                                   select c.CustomerName + "(" + c.CustomerNum + "&" + c.SiteUseId + ")").FirstOrDefault();


            if (!string.IsNullOrEmpty(templateType) || !string.IsNullOrEmpty(templatelang))
            {
                Helper.Log.Info("-----------------------------------------------templateType:" + templateType);
                Helper.Log.Info("-----------------------------------------------templatelang:" + templatelang);
            }
            else
            {

                Helper.Log.Info("-----------------------------------------------templateType & templatelang all null");
            }

            DateTime PeriodEndDate = CommonRep.GetDbSet<PeriodControl>().Where(o => o.Deal == AppContext.Current.User.Deal && o.PeriodBegin <= CurrentTime
                                    && o.PeriodEnd >= CurrentTime && o.SoaFlg == "1").Select(o => o.PeriodEnd).FirstOrDefault();
            DateTime PeriodEndStart = CommonRep.GetDbSet<PeriodControl>().Where(o => o.Deal == AppContext.Current.User.Deal && o.PeriodBegin <= CurrentTime
                                    && o.PeriodEnd >= CurrentTime && o.SoaFlg == "1").Select(o => o.PeriodBegin).FirstOrDefault();
            string strPeriodEndDate = PeriodEndDate.ToString("MM/dd/yyyy");
            string strPeriodStartDateEN = PeriodEndStart.ToString("MMM", new System.Globalization.CultureInfo("en-us")) + " " + PeriodEndStart.ToString("yyyy");
            string strPeriodEndDateMMDDYYYY = PeriodEndDate.ToString("MM/dd/yyyy");
            string strPeriodEndDateYYYYMMDD = PeriodEndDate.ToString("yyyy-MM-dd");
            string strprePeriodEndDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM") + "-01").AddDays(-1).ToString("MM/dd/yyyy");

            Helper.Log.Info("------------------------------------PeriodEndDate: " + strPeriodEndDate);

            // 2, retrieve template based on customer information and hint.
            IMailService ms = SpringFactory.GetObjectImpl<IMailService>("MailService");
            MailTemplate tpl = null;
            if (templateType != "" || templateType != null)
            {
                string templatelangNow = templatelang;
                if (templatelang == "008") { templatelangNow = "006"; }
                tpl = ms.GetMailTemplatebytype(templateType, templatelangNow);

                if (tpl != null)
                {
                    List<string> toList = new List<string>();
                    List<string> ccList = new List<string>();
                    try
                    {
                        res = ms.GetInstanceFromTemplate(tpl, (parser) =>
                        {
                            ContactService cs = SpringFactory.GetObjectImpl<ContactService>("ContactService");

                            string contactNames = "";
                            string[] toNameArray = ToName.Split(';');
                            foreach (string CurrentToName in toNameArray)
                            {
                                var sname = CommonRep.GetQueryable<Contactor>()
                                            .Where(o => o.Title == ToTitle && (customerNumList.Contains(o.CustomerNum) || customerNums == "") && (siteUseId == "" || siteUseIdList.Contains(o.SiteUseId)) && (o.EmailAddress == CurrentToName || o.Name == CurrentToName))
                                            .Select(o => o.Name).FirstOrDefault();

                                if (!string.IsNullOrEmpty(sname))
                                {
                                    if (!string.IsNullOrEmpty(contactNames))
                                    {
                                        contactNames += " & " + sname;
                                    }
                                    else
                                    {
                                        contactNames += sname;
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(siteUseId))
                            {
                                contactNames = "";
                                //手动单个SiteUseId发
                                IList<Contactor> contactors = cs.GetContactsByCustomers(customerNums, siteUseId);
                                foreach (Contactor cont in contactors)
                                {
                                    if (cont.Title == ToTitle)
                                    {
                                        if (!toList.Contains(cont.EmailAddress))
                                        {
                                            toList.Add(cont.EmailAddress);
                                            if (contactNames.IndexOf(cont.Name) < 0)
                                            {
                                                contactNames += (cont.Name + ", ");
                                            }
                                        }
                                    }
                                    else if (CCTitle.IndexOf(cont.Title) >= 0)
                                    {
                                        if (!ccList.Contains(cont.EmailAddress))
                                        {
                                            ccList.Add(cont.EmailAddress);
                                        }
                                    }
                                }
                            }
                            contactNames = contactNames.Trim();
                            contactNames = contactNames.TrimEnd(',');
                            //生成附件并取得附近名和币种的合计值
                            System.Data.DataTable[] reportItemList;
                            InvoiceService invServ = SpringFactory.GetObjectImpl<InvoiceService>("InvoiceService");
                            Helper.Log.Info("**************************indexFile:" + indexFile);
                            string[] attachPathList = invServ.setContent(intIds, templateType, out reportItemList, customerNums, siteUseId, templatelang, Collector, ToTitle, ToName, CCTitle, indexFile, fileType).ToArray();
                            attachment = string.Join(",", attachPathList);

                            //正文表格
                            string reportStr = "";
                            if (reportItemList != null)
                            {
                                int ii = 0;
                                foreach (System.Data.DataTable dt in reportItemList)
                                {
                                    ii++;
                                    //ASEAN, WAVE1&WAVE2邮件正文暂时不要币制合计
                                    if ((templatelang == "006" || templatelang == "009") && (templateType == "001" || templateType == "002") && ii == 1) { continue; }
                                    if (dt.Rows.Count > 0)
                                    {
                                        if (!string.IsNullOrEmpty(reportStr))
                                        {
                                            reportStr += "<br>" + invServ.GetHTMLTableByDataTable(dt);
                                        }
                                        else
                                        {
                                            reportStr += invServ.GetHTMLTableByDataTable(dt);
                                        }
                                    }
                                }
                            }
                            parser.RegistContext("attachmentInfo", reportStr);
                            parser.RegistContext("templatelang", templatelang);

                            //====================================================================
                            //Start test by albert on 2017-11-02
                            parser.RegistContext("ContactName", contactNames);
                            //End test by albert on 2017-11-02
                            // 2, collector
                            parser.RegistContext("collector", Collector);
                            parser.RegistContext("prePeriodEndDate", strprePeriodEndDate);
                            parser.RegistContext("periodEndDate", strPeriodEndDate);
                            parser.RegistContext("periodStartDateEN", strPeriodStartDateEN);
                            parser.RegistContext("periodEndDateMMDDYYYY", strPeriodEndDateMMDDYYYY);
                            parser.RegistContext("periodEndDateYYYYMMDD", strPeriodEndDateYYYYMMDD);

                            string CurrentDate = DateTime.Now.ToString("yyyy-MM-dd");
                            if (templatelang == "001" || templatelang == "002")
                            {
                                CurrentDate = DateTime.Now.ToString("yyyy年MM月dd日");
                            }

                            if (templatelang == "007" || templatelang == "0071")
                            {
                                if (templateType == "001")
                                {
                                    string strHKOverDueBody = "";
                                    //判断INV是否有Due的
                                    List<InvoiceAging> invoicelist = new List<InvoiceAging>();
                                    invoicelist = (from inv in CommonRep.GetQueryable<InvoiceAging>()
                                                   where intIds.Contains(inv.Id) && inv.DueDate <= AppContext.Current.User.Now
                                                   select inv).ToList();
                                    if (invoicelist == null || invoicelist.Count == 0)
                                    {
                                        //没有Due
                                        strHKOverDueBody = (from dic in CommonRep.GetQueryable<SysTypeDetail>()
                                                            where dic.TypeCode == "055" && dic.DetailName == "NotOverDue"
                                                            select dic.Description).FirstOrDefault();
                                        parser.RegistContext("HKOverDueBody", strHKOverDueBody);
                                        parser.RegistContext("ASAP", "");
                                    }
                                    else
                                    {
                                        //有Due的
                                        strHKOverDueBody = (from dic in CommonRep.GetQueryable<SysTypeDetail>()
                                                            where dic.TypeCode == "055" && dic.DetailName == "HasOverDue"
                                                            select dic.Description).FirstOrDefault();
                                        parser.RegistContext("HKOverDueBody", strHKOverDueBody);
                                        parser.RegistContext("ASAP", "/ASAP");
                                    }
                                }
                            }

                            if (templatelang == "006" || templatelang == "008" || templatelang == "009")
                            {
                                if (templateType == "000" || templateType == "001" || templateType == "002")
                                {
                                    string strASEANOverDueBody = "";
                                    //判断INV是否有Due的
                                    List<InvoiceAging> invoicelist = new List<InvoiceAging>();
                                    invoicelist = (from inv in CommonRep.GetQueryable<InvoiceAging>()
                                                   where intIds.Contains(inv.Id) && inv.DueDate <= PeriodEndDate
                                                   select inv).ToList();
                                    if (invoicelist == null || invoicelist.Count == 0)
                                    {
                                        //没有Due
                                        strASEANOverDueBody = "";
                                        parser.RegistContext("ASEANOverDueBody", strASEANOverDueBody);
                                    }
                                    else
                                    {
                                        //有Due的
                                        strASEANOverDueBody = (from dic in CommonRep.GetQueryable<SysTypeDetail>()
                                                               where dic.TypeCode == "056" && dic.DetailName == "HasOverDue"
                                                               select dic.Description).FirstOrDefault();
                                        parser.RegistContext("ASEANOverDueBody", strASEANOverDueBody);
                                    }
                                }
                            }
                            if (string.IsNullOrEmpty(CustomerName)) { CustomerName = ""; }
                            parser.RegistContext("CustomerNames", CustomerName);
                            parser.RegistContext("customerName", CustomerNameNoSite);
                            parser.RegistContext("CurrentDate", CurrentDate);
                            parser.RegistContext("ResponseDate", (string.IsNullOrEmpty(ResponseDate) ? CurrentDate : ResponseDate));
                            
                        }, Region);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    if (toList.Count > 0)
                    {
                        string to = string.Join(";", toList);
                        res.To = to;
                    }
                    if (ccList.Count > 0)
                    {
                        string cc = string.Join(";", ccList);
                        res.Cc = cc;
                    }
                    //added by alex body中显示附件名+Currency
                    //附件的id
                    res.Attachment = attachment;
                }
                else
                {
                    Helper.Log.Info("------------------------- tpl is null");
                }
            }
            else
            {
                Exception ex = new OTCServiceException("No matching template was found!", System.Net.HttpStatusCode.NotFound);
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }

            //根据Collector获得Collector邮件标题标识
            string strCollectorMailId = "";
            string strCollector = "";
            if (String.IsNullOrEmpty(Collector))
            {
                strCollector = CommonRep.GetQueryable<Customer>().Where(x => (customerNumList.Contains(x.CustomerNum) || customerNums == "") && (siteUseIdList.Contains(x.SiteUseId) || siteUseId == "")).Select(x => x.Collector).FirstOrDefault();
            }
            else
            {
                strCollector = Collector;
            }

            //根据Collector获得发送的组邮箱
            if (!string.IsNullOrEmpty(strCollector))
            {

                Helper.Log.Info("------------------------------- strCollector:" + strCollector);
                var groupMailBox = (from ca in CommonRep.GetDbSet<SysTypeDetail>()
                                    where ca.TypeCode == "045" && ca.DetailName == strCollector
                                    select ca.DetailValue2).FirstOrDefault().ToString();
                if (groupMailBox == null)
                {
                    Helper.Log.Info("------------------------------- groupMailBox is null:" + strCollector);
                }
                res.From = groupMailBox;
            }

            //CC组邮箱
            if (!string.IsNullOrEmpty(res.Cc))
            {
                res.Cc += ";" + res.From;
            }
            else
            {
                res.Cc = res.From;
            }

            if (res == null || res.Subject == null)
            {
                Helper.Log.Info("------------------------------- res is null");
            }
            if (tpl == null || tpl.Subject == null)
            {
                Helper.Log.Info("------------------------------- tpl is null");
            }

            // 设置邮件标题
            res.Subject = tpl.Subject;


            var TypeTem = "";

            if (templateType == "000")
            {
                TypeTem = "Daily";
            }
            if (templateType == "001")
            {
                TypeTem = "Wave1";
            }
            else if (templateType == "002")
            {
                TypeTem = "Wave2";
            }
            else if (templateType == "003")
            {
                TypeTem = "Wave3";
            }
            else if (templateType == "004")
            {
                TypeTem = "Wave4";
            }
            else if (templateType == "005")
            {
                TypeTem = "PMT";
            }

            if (templatelang == "006" || templatelang == "009")
            {
                res.Subject = "Arrow Asia Electronics (S) Pte Ltd: " + (string.IsNullOrEmpty(CustomerGroupName) ? CustomerNameNoSite : CustomerGroupName) + " (" + (templateType == "000" ? DateTime.Today.ToString("yyyy/MM/dd") : res.Subject) + ")";
            }
            else if (templatelang == "011")
            {
                string SubRegion = "";
                switch (strLegalEntity) { 
                    case "308":
                        SubRegion = "(AU) : " + CustomerNameNoSite;
                        break;
                    case "309":
                        SubRegion = "(NZ) : " + CustomerNameNoSite;
                        break;
                }
                res.Subject = res.Subject.Replace("XXX", SubRegion);
            }
            else if (templatelang == "008")
            {
                res.Subject = res.Subject;
            }
            else if (templatelang == "007" || templatelang == "0071")
            {
                if (templateType == "001")
                {
                    //判断INV是否有Due的
                    List<InvoiceAging> invoicelist = new List<InvoiceAging>();
                    invoicelist = (from inv in CommonRep.GetQueryable<InvoiceAging>()
                                   where intIds.Contains(inv.Id) && inv.DueDate <= AppContext.Current.User.Now
                                   select inv).ToList();
                    if (invoicelist == null || invoicelist.Count == 0)
                    {
                        //没有Due
                        res.Subject = (CustomerGroupName == "" ? CustomerNameNoSite : CustomerGroupName) + " - (Pre-Alert) - attached AR statement!";
                    }
                    else
                    {
                        //有Due的
                        res.Subject = (CustomerGroupName == "" ? CustomerNameNoSite : CustomerGroupName) + " - (Reminder&Pre-Alert)";
                    }
                }
                else if (templateType == "005")
                {
                    res.Subject = (CustomerGroupName == "" ? CustomerNameNoSite : CustomerGroupName) + " - (payment details)";
                }
                else
                {
                    res.Subject = (CustomerGroupName == "" ? CustomerNameNoSite : CustomerGroupName) + " (" + res.Subject + ")";
                }
            }
            else if (templatelang == "001")
            {
                res.Subject += " (" + DateTime.Now.ToString("yyyyMMdd") + "-" + TypeTem + "-" + strLegalEntity + "-" + strCollector + "-" + customerNums + ")";
            }
            else
            {
                res.Subject += " (" + DateTime.Now.ToString("yyyyMMdd") + "-" + TypeTem + "-" + strLegalEntity + "-" + strCollector;
                if (!string.IsNullOrEmpty(ToName))
                {
                    res.Subject += "-To-" + ToName + ")";
                }
                else {
                    res.Subject += ")";
                }
            }
            return res;
        }

        public MailTmp GetPmtMailInstance(string customerNums, string siteUseId, string templateType, string templatelang)
        {

            List<string> customerNumList = customerNums.Split(',').ToList();
            List<string> siteUseIdList = siteUseId.Split(',').ToList();

            MailTmp res = null;
            string attachment = "";

            // 2, retrieve template based on customer information and hint.
            IMailService ms = SpringFactory.GetObjectImpl<IMailService>("MailService");
            MailTemplate tpl = null;
            tpl = ms.GetMailTemplatebytype(templateType, templatelang);

            if (tpl != null)
            {
                try
                {
                    res = ms.GetInstanceFromTemplate(tpl, (parser) =>
                    {
                        // 1, contactNames used in SOA template
                        ContactService cs = SpringFactory.GetObjectImpl<ContactService>("ContactService");
                        //Start test by albert on 2017-11-02

                        IList<Contactor> contactors = cs.GetContactsByCustomers(customerNums, siteUseId);
                        string contactorName = string.Join(",", contactors.Select(x => x.Name).ToList());

                        //End test by albert on 2017-11-02
                        List<string> toList = new List<string>();
                        List<string> ccList = new List<string>();
                        string contactNames = string.Empty;
                        foreach (Contactor cont in contactors)
                        {
                            if (cont.ToCc == "1")
                            {
                                if (!toList.Contains(cont.EmailAddress))
                                {
                                    toList.Add(cont.EmailAddress);
                                    contactNames += (cont.Name + ", ");
                                }
                            }
                            else
                            {
                                if (!ccList.Contains(cont.EmailAddress))
                                {
                                    ccList.Add(cont.EmailAddress);
                                }
                            }
                        }
                        contactNames = contactNames.TrimEnd(',');
                        //生成附件并取得附近名和币种的合计值
                        parser.RegistContext("siteUseId", siteUseId);

                        //====================================================================
                        //Start test by albert on 2017-11-02
                        parser.RegistContext("contactNames", contactNames);
                        //End test by albert on 2017-11-02
                        // 2, collector
                        parser.RegistContext("collector", AppContext.Current.User.Name);

                        string[] custList = customerNums.Split(',');
                        string[] siteUseIdList1 = siteUseId.Split(',');

                        List<CustomerAging> customerList = CommonRep.GetQueryable<CustomerAging>().Where(x => custList.Contains(x.CustomerNum) && siteUseIdList1.Contains(x.SiteUseId)).ToList();
                        string[] legalList = customerList.Select(x => x.LegalEntity).ToArray();
                        List<Sites> siteList = CommonRep.GetQueryable<Sites>().Where(x => x.Deal == AppContext.Current.User.Deal && legalList.Contains(x.LegalEntity)).ToList();


                        parser.RegistContext("company", string.Join(",", siteList.Select(x => x.SiteNameSys)));
                        parser.RegistContext("customerName", string.Join(",", customerList.Select(x => x.CustomerName)));
                    });
                }
                catch (Exception ex)
                {
                    Helper.Log.Error(ex.Message, ex);
                    throw ex;
                }
                //added by alex body中显示附件名+Currency
                //附件的id
                res.Attachment = attachment;
            }
            else
            {
                throw new OTCServiceException("No matching template was found!", System.Net.HttpStatusCode.NotFound);
            }

            //根据Collector获得Collector邮件标题标识
            string strCollectorMailId = "";
            string strCollector = CommonRep.GetQueryable<Customer>().Where(x => siteUseIdList.Contains(x.SiteUseId)).Select(x => x.Collector).FirstOrDefault();
            if (!string.IsNullOrEmpty(strCollector))
            {
                strCollectorMailId = CommonRep.GetQueryable<SysTypeDetail>().Where(x => x.TypeCode == "045" && x.DetailName == strCollector).Select(x => x.DetailValue).FirstOrDefault();
            }

            // 2018-02-26 Added by Albert 设置邮件标题
            List<string> nCusNames = CommonRep.GetQueryable<Customer>().Where(x => siteUseIdList.Contains(x.SiteUseId)).Select(x => x.CustomerName).ToList();
            if (nCusNames != null && nCusNames.Count > 0)
                res.Subject = "ARROW艾睿电子 - " + string.Join(",", nCusNames) + " - " + DateTime.Now.Year + "年" + DateTime.Now.Month.ToString().PadLeft(2, '0') + "月" + "对账单";
            else
                res.Subject = "ARROW艾睿电子 - " + DateTime.Now.Year + "年" + DateTime.Now.Month.ToString().PadLeft(2, '0') + "月" + "对账单";

            if (!string.IsNullOrEmpty(strCollectorMailId))
            {
                res.Subject += " (" + strCollectorMailId + "-" + siteUseId + ")";
            }

            return res;
        }
        public MailTmp GetCaPmtMailInstance(string EID, string strId, string strBsId, string strLegalEntity, string strCustomerNum, string strSiteUseId, string templateType, string templatelang, string strIndexFile = "", string fileType = "XLS")
        {
            Helper.Log.Info("************** Pmt Detail mail start **************");
            if (string.IsNullOrEmpty(strLegalEntity)) { strLegalEntity = ""; }
            if (string.IsNullOrEmpty(strCustomerNum)) { strCustomerNum = ""; }
            
            List<string> listIds = strId.Split(',').ToList();

            MailTmp res = null;
            List<int> intIds = new List<int>();
            intIds = (from a in CommonRep.GetQueryable<InvoiceAging>()
                      where a.TrackStates != "014"
                             && a.LegalEntity == strLegalEntity
                             && a.CustomerNum == strCustomerNum
                            && (a.Class == "INV" || a.Class == "CM")
                      orderby a.LegalEntity ascending, a.CustomerNum ascending, a.SiteUseId, a.InvoiceDate ascending
                      select a.Id).ToList();
            string strCustomerName = "";
            string strTRANSACTION_NUMBER = "";
            DateTime? strVALUE_DATE = null;
            string strCURRENCY = "";
            decimal decCURRENT_AMOUNT = 0;
            string strDescription = "";
            //string strSql = string.Format(@"SELECT TRANSACTION_NUMBER,
            //                                        VALUE_DATE,
            //                                        CURRENCY,
            //                                        CURRENT_AMOUNT,
            //                                        CUSTOMER_NAME,
            //                                        Description
            //                   FROM t_ca_bankstatement 
            //                  WHERE id = '{0}'", strBsId);
            //if (lb_MulitRecord) {
            Helper.Log.Info("****************** id: *******************" + strId);
            string strBSIds = "'" + strBsId.Replace(",", "','") + "'";
                string strSql = string.Format(@"SELECT TRANSACTION_NUMBER,
                                                    VALUE_DATE,
                                                    CURRENCY,
                                                    CURRENT_AMOUNT,
                                                    CUSTOMER_NAME,
                                                    Description,
                                                    SiteUseId,
                                                    TRANSACTION_AMOUNT,
                                                    BankChargeTo,
                                                    CUSTOMER_NUM
                               FROM t_ca_bankstatement 
                              WHERE id in ({0})", strBSIds);
            //}
            List<CaBankStatementDto> list = SqlHelper.GetList<CaBankStatementDto>(SqlHelper.ExcuteTable(strSql, System.Data.CommandType.Text, null));
            int transNumberCount = 0;
            foreach (CaBankStatementDto b in list)
            {
                transNumberCount++;
                if (string.IsNullOrEmpty(strTRANSACTION_NUMBER))
                {
                    strTRANSACTION_NUMBER += b.TRANSACTION_NUMBER;
                }
                else
                {
                    strTRANSACTION_NUMBER += "&" + b.TRANSACTION_NUMBER;
                }
                if (transNumberCount >= 5 && transNumberCount < list.Count)
                {
                    strTRANSACTION_NUMBER += "...";
                    break;
                }
            }

            Helper.Log.Info("************* 11111111111 *************");
            if (list.Count == 0)
            {
                Helper.Log.Info("************* bs not exists *************");
                return res; }
            strCustomerName = list[0].CUSTOMER_NAME == null ? "" : list[0].CUSTOMER_NAME;
            if (list.Count > 0)
            {
                //strTRANSACTION_NUMBER = list[0].TRANSACTION_NUMBER;
                strVALUE_DATE = list[0].VALUE_DATE;
                strCURRENCY = list[0].CURRENCY;
                decCURRENT_AMOUNT = list[0].CURRENT_AMOUNT == null ? 0 : Convert.ToDecimal(list[0].CURRENT_AMOUNT);
                strCustomerName = list[0].CUSTOMER_NAME;
                strDescription = list[0].Description;
            }

            Helper.Log.Info("************* 2222222222 *************");
            var strlegalEntityName = (from site in CommonRep.GetQueryable<Sites>()
                                      where site.LegalEntity == strLegalEntity
                                      select site.LegalEntity + " " + site.SiteNameSys
                               ).FirstOrDefault();

            string attachment = "";
            string attachPhysicsPathString = "";
            if (!string.IsNullOrEmpty(templateType) || !string.IsNullOrEmpty(templatelang))
            {
                Helper.Log.Info("-----------------------------------------------templateType:" + templateType);
                Helper.Log.Info("-----------------------------------------------templatelang:" + templatelang);
            }
            else
            {

                Helper.Log.Info("-----------------------------------------------templateType & templatelang all null");
            }
            Helper.Log.Info("************* 3333333333 *************");
            // 2, retrieve template based on customer information and hint.
            IMailService ms = SpringFactory.GetObjectImpl<IMailService>("MailService");
            MailTemplate tpl = null;
            if (templateType != "" || templateType != null)
            {
                Helper.Log.Info("************* templete is not null *************");
                string templatelangNow = templatelang;
                tpl = ms.GetMailTemplatebytype(templateType, templatelangNow);

                if (tpl != null)
                {
                    try
                    {
                        res = ms.GetInstanceFromTemplate(tpl, (parser) =>
                        {
                            //生成附件并取得附近名和币种的合计值
                            System.Data.DataTable[] reportItemList;
                            InvoiceService invServ = SpringFactory.GetObjectImpl<InvoiceService>("InvoiceService");
                            string[] attachPathList = null;
                            List<string> attachPhysicsPathList = new List<string>();
                            if (templatelang == "001")
                            {
                                //string strSiteUseIdLink = "";
                                //foreach (CaBankStatementDto bs in list) {
                                //    Helper.Log.Info("************************************* siteuseid:" + bs.SiteUseId);
                                //    if (!string.IsNullOrEmpty(bs.SiteUseId) && strSiteUseIdLink.IndexOf(bs.SiteUseId) < 0) {
                                //        if (!string.IsNullOrEmpty(strSiteUseIdLink)) {
                                //            strSiteUseIdLink += "&" + bs.SiteUseId;
                                //        }
                                //        else {
                                //            strSiteUseIdLink += bs.SiteUseId;
                                //        }
                                //    }
                                //}
                                strCustomerName = strCustomerName.Replace("/", " ");
                                strCustomerName = strCustomerName.Replace("\\", " ");
                                strCustomerName = strCustomerName.Replace(":", " ");
                                strCustomerName = strCustomerName.Replace("?", " ");
                                strCustomerName = strCustomerName.Replace("\"", " ");
                                strCustomerName = strCustomerName.Replace("<", " ");
                                strCustomerName = strCustomerName.Replace(">", " ");
                                strCustomerName = strCustomerName.Replace("?", " ");
                                if (strCustomerName.Length > 50) { strCustomerName = strCustomerName.Substring(0, 50); }
                                string path = strLegalEntity + " RV#" + strTRANSACTION_NUMBER + " Acc#" + strCustomerNum + " " + strCustomerName + ".xlsx";
                                attachPathList = invServ.setCaPmtMailContentCN(path, strId, list, intIds, strLegalEntity, ref decCURRENT_AMOUNT, ref attachPhysicsPathList, out reportItemList, strIndexFile, fileType).ToArray();
                            }
                            else
                            {
                                attachPathList = invServ.setCaPmtMailContent(strId, intIds, strLegalEntity, strCustomerNum, strCustomerName, strTRANSACTION_NUMBER, strVALUE_DATE, strCURRENCY, decCURRENT_AMOUNT, strDescription, out reportItemList, fileType).ToArray();
                            }
                            attachment = string.Join(",", attachPathList);
                            attachPhysicsPathString = string.Join(";", attachPhysicsPathList);

                            //正文表格
                            string reportStr = "";
                            if (reportItemList != null)
                            {
                                int ii = 0;
                                foreach (System.Data.DataTable dt in reportItemList)
                                {
                                    ii++;
                                    if (dt.Rows.Count > 0)
                                    {
                                        if (!string.IsNullOrEmpty(reportStr))
                                        {
                                            reportStr += "<br>" + invServ.GetHTMLTableByDataTableCa(dt);
                                        }
                                        else
                                        {
                                            reportStr += invServ.GetHTMLTableByDataTableCa(dt);
                                        }
                                    }
                                }
                            }
                            DateTime dtValueDate = strVALUE_DATE == null ? DateTime.Now : Convert.ToDateTime(strVALUE_DATE);
                            parser.RegistContext("collector", EID);
                            parser.RegistContext("templatelang", templatelangNow);
                            parser.RegistContext("attachmentInfo", reportStr);
                            parser.RegistContext("CurrentDate", dtValueDate.ToString("MM/dd/yyyy"));
                            parser.RegistContext("Currency", strCURRENCY);
                            parser.RegistContext("Amount", Math.Round(decCURRENT_AMOUNT, 2).ToString("#,###.00"));
                            parser.RegistContext("ContactName", strCustomerName);
                            // 付款客户名称、实体名称、签名
                            parser.RegistContext("PaymentCustomerName", strCustomerName + "/" + strCustomerNum + "/" + strSiteUseId);
                            parser.RegistContext("EntityName", strlegalEntityName);
                            //parser.RegistContext("SignatureEn", EID);
                        });
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    //附件的id
                    res.Attachment = attachment;
                    res.AttachmentPath = attachPhysicsPathString;
                }
                else
                {
                    Helper.Log.Info("------------------------- tpl is null");
                }
            }
            else
            {
                Exception ex = new OTCServiceException("No matching template was found!", System.Net.HttpStatusCode.NotFound);
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
            // 设置邮件标题
            if (templatelang == "001")
            {
                // 销账明细需求 / 3641 / DBG (JIAXING) TECHNOLOGY ELECTRONICS CO., LTD / 1106787 / 2358531 - RV:18431568
                res.Subject = "销账明细需求 / " + strLegalEntity + " / " + strCustomerName + " / " + strCustomerNum ;
            }
            else
            {
                res.Subject = tpl.Subject + " - LegalEntity:" + strLegalEntity + " - RV:" + strTRANSACTION_NUMBER + "(" + strCustomerName + ")";
            }

            Helper.Log.Info("--------------------------------Subject:" + res.Subject);
            return res;
        }
        public MailTmp GetCaClearMailInstance(string EID, string strId, string strBsId, string strLegalEntity, string strCustomerNum, string strSiteUseId, string templateType, string templatelang, string strIndexFile = "", string fileType = "XLS")
        {
            Helper.Log.Info("************** Clear confirm mail start **************");
            if (string.IsNullOrEmpty(strLegalEntity)) { strLegalEntity = ""; }
            if (string.IsNullOrEmpty(strCustomerNum)) { strCustomerNum = ""; }

            MailTmp res = null;
            List<int> intIds = new List<int>();
            intIds = (from a in CommonRep.GetQueryable<InvoiceAging>()
                      where  a.CustomerNum == strCustomerNum
                            && (a.Class == "INV" || a.Class == "CM")
                      orderby a.LegalEntity ascending, a.CustomerNum ascending, a.SiteUseId, a.InvoiceDate ascending
                      select a.Id).ToList();

            Helper.Log.Info("************** strCustomerNum:" + strCustomerNum);
            Helper.Log.Info("********************* ar count: " + intIds.Count());

            string strCustomerName = "";
            string strTRANSACTION_NUMBER = "";
            DateTime? strVALUE_DATE = null;
            string strCURRENCY = "";
            decimal decCURRENT_AMOUNT = 0;
            string strDescription = "";
            // 获取bsid信息
            //string strIdLike = "'" + strId.Replace(",", "','") + "'";
            List<string> listBsId = strBsId.Split(',').ToList();
            List<CaBankStatementDto> list = new List<CaBankStatementDto>();
            foreach (string id in listBsId) {
                string strSql = string.Format(@"select bsid.*, m.*, (select 
                                                    max(T_CA_PMT.ReceiveDate)
                                                    from T_CA_PMTBS join T_CA_PMT on T_CA_PMTBS.ReconId = T_CA_PMT.id
                                                    where T_CA_PMTBS.BANK_STATEMENT_ID = m.id ) PMTReceiveDate
                                            from
                                            (
                                             SELECT T_CA_Recon.id as reconId, BANK_STATEMENT_ID,Amount ReconBS_Amount, 
                                            (case T_CA_Recon.GroupType when 'AR' then 'Based on AR' when 'PTP' then 'Based on Promise to Pay' when 'PMT' then 'Based on PMT' else '' end) as reconType FROM T_CA_ReconBS
											 join T_CA_Recon on T_CA_Recon.id = T_CA_ReconBS.ReconId
                                             WHERE ReconId IN (
                                              select TOP 1 t_ca_recon.ID 
                                              from t_ca_recon 
                                              left join t_ca_reconbs on t_ca_recon.id = t_ca_reconbs.ReconId
                                              where t_ca_reconbs.BANK_STATEMENT_ID = '{0}'
                                              and t_ca_recon.GroupType not like 'UN%'
                                                    and t_ca_recon.GroupType not like 'NM%'
                                              ORDER BY t_ca_recon.CREATE_DATE DESC
                                             )
                                            ) as bsid
                                            left join T_CA_BankStatement m on m.id=bsid.BANK_STATEMENT_ID", id);
                List<CaBankStatementDto> listOne = SqlHelper.GetList<CaBankStatementDto>(SqlHelper.ExcuteTable(strSql, System.Data.CommandType.Text, null));
                if (listOne != null) {
                    list.AddRange(listOne);
                }
            }
            int transNumberCount = 0;
            foreach (CaBankStatementDto b in list)
            {
                transNumberCount++;
                if (string.IsNullOrEmpty(strTRANSACTION_NUMBER))
                {
                    strTRANSACTION_NUMBER += b.TRANSACTION_NUMBER;
                }
                else
                {
                    strTRANSACTION_NUMBER += "&" + b.TRANSACTION_NUMBER;
                }
                if (transNumberCount >= 5 && transNumberCount < list.Count)
                {
                    strTRANSACTION_NUMBER += "...";
                    break;
                }
            }
            decimal ldec_ClearAmount = 0;
            if (list.Count > 0)
            {
                //strTRANSACTION_NUMBER = list[0].TRANSACTION_NUMBER;
                strVALUE_DATE = list[0].VALUE_DATE;
                strCURRENCY = list[0].CURRENCY;
                decCURRENT_AMOUNT = list.Sum(o => o.CURRENT_AMOUNT == null ? 0 : Convert.ToDecimal(o.CURRENT_AMOUNT));
                strCustomerName = list[0].CUSTOMER_NAME;
                strDescription = list[0].Description;
                ldec_ClearAmount += list.Sum(o => o.ReconBS_Amount == null ? 0 : Convert.ToDecimal(o.ReconBS_Amount));
            }

            var strlegalEntityName = (from site in CommonRep.GetQueryable<Sites>()
                                      where site.LegalEntity == strLegalEntity
                                      select site.LegalEntity + " " + site.SiteNameSys
                               ).FirstOrDefault();

            string attachment = "";
            string attachPhysicsPathString = "";
            if (!string.IsNullOrEmpty(templateType) || !string.IsNullOrEmpty(templatelang))
            {
                Helper.Log.Info("-----------------------------------------------templateType:" + templateType);
                Helper.Log.Info("-----------------------------------------------templatelang:" + templatelang);
            }
            else
            {

                Helper.Log.Info("-----------------------------------------------templateType & templatelang all null");
            }
            // 2, retrieve template based on customer information and hint.
            IMailService ms = SpringFactory.GetObjectImpl<IMailService>("MailService");
            MailTemplate tpl = null;
            if (templateType != "" || templateType != null)
            {
                string templatelangNow = templatelang;
                tpl = ms.GetMailTemplatebytype(templateType, templatelangNow);

                if (tpl != null)
                {
                    Helper.Log.Info("****************************templete finded***************************");
                    try
                    {
                        res = ms.GetInstanceFromTemplate(tpl, (parser) =>
                        {
                            
                            //生成附件并取得附近名和币种的合计值
                            System.Data.DataTable[] reportItemList;
                            InvoiceService invServ = SpringFactory.GetObjectImpl<InvoiceService>("InvoiceService");
                            string[] attachPathList = null;
                            List<string> attachPhysicsPathList = new List<string>();

                            strCustomerName = strCustomerName.Replace("/", " ");
                            strCustomerName = strCustomerName.Replace("\\", " ");
                            strCustomerName = strCustomerName.Replace(":", " ");
                            strCustomerName = strCustomerName.Replace("?", " ");
                            strCustomerName = strCustomerName.Replace("\"", " ");
                            strCustomerName = strCustomerName.Replace("<", " ");
                            strCustomerName = strCustomerName.Replace(">", " ");
                            strCustomerName = strCustomerName.Replace("?", " ");
                            if (strCustomerName.Length > 50) { strCustomerName = strCustomerName.Substring(0, 50); }
                            string path = strLegalEntity + " RV#" + strTRANSACTION_NUMBER + " Acc#" + strCustomerNum + "" + strCustomerName + ".xlsx";

                            Helper.Log.Info("****************************setCaPmtMailContentCNClear start***************************");
                            attachPathList = invServ.setCaPmtMailContentCNClear(path, list, intIds, strLegalEntity, ref decCURRENT_AMOUNT, ref attachPhysicsPathList, out reportItemList, strIndexFile,  fileType).ToArray();
                            Helper.Log.Info("****************************setCaPmtMailContentCNClear end***************************");
                            attachment = string.Join(",", attachPathList);
                            attachPhysicsPathString = string.Join(";", attachPhysicsPathList);
                            Helper.Log.Info("****************************attachment：" + attachment);
                            Helper.Log.Info("****************************attachPhysicsPathString：" + attachPhysicsPathString);

                            //正文表格
                            string reportStr = "";
                            if (reportItemList != null)
                            {
                                int ii = 0;
                                foreach (System.Data.DataTable dt in reportItemList)
                                {
                                    ii++;
                                    if (dt.Rows.Count > 0)
                                    {
                                        if (!string.IsNullOrEmpty(reportStr))
                                        {
                                            reportStr += "<br>" + invServ.GetHTMLTableByDataTableCa(dt);
                                        }
                                        else
                                        {
                                            reportStr += invServ.GetHTMLTableByDataTableCa(dt);
                                        }
                                    }
                                }
                            }
                            Helper.Log.Info("****************************reportStr end***************************");

                            DataTable dtnotes = createCAMailNotesTable();
                            string strNotesStr = invServ.GetHTMLTableByDataTableCa(dtnotes);

                            Helper.Log.Info("****************************strNotesStr end***************************");

                            DateTime dtValueDate = strVALUE_DATE == null ? DateTime.Now : Convert.ToDateTime(strVALUE_DATE);
                            parser.RegistContext("collector", EID);
                            parser.RegistContext("templatelang", templatelangNow);
                            parser.RegistContext("attachmentInfo", reportStr);
                            parser.RegistContext("Notestable", strNotesStr);
                            parser.RegistContext("CurrentDate", dtValueDate.ToString("MM/dd/yyyy"));
                            parser.RegistContext("Currency", strCURRENCY);
                            parser.RegistContext("Amount", Math.Round(ldec_ClearAmount, 2).ToString("#,###.00"));
                            parser.RegistContext("ContactName", strCustomerName);
                            // 付款客户名称、实体名称、签名
                            //parser.RegistContext("PaymentCustomerName", strCustomerName + "/" + strCustomerNum + "/" + strSiteUseId);
                            parser.RegistContext("EntityName", strlegalEntityName);
                            //parser.RegistContext("SignatureEn", EID);
                        });
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    //附件的id
                    res.Attachment = attachment;
                    res.AttachmentPath = attachPhysicsPathString;
                }
                else
                {
                    Helper.Log.Info("------------------------- tpl is null");
                }
            }
            else
            {
                Exception ex = new OTCServiceException("No matching template was found!", System.Net.HttpStatusCode.NotFound);
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
            // 设置邮件标题
            if (templatelang == "001")
            {
                // 销账结果确认 / 3641 / DBG (JIAXING) TECHNOLOGY ELECTRONICS CO., LTD / 1106787 / 2358531 - RV:18431568
                res.Subject = "销账结果确认 / " + strLegalEntity + " / " + strCustomerName + " / " + strCustomerNum + " - RV:" + strTRANSACTION_NUMBER;
            }
            else
            {
                res.Subject = tpl.Subject + " - LegalEntity:" + strLegalEntity + " - RV:" + strTRANSACTION_NUMBER + "(" + strCustomerName + ")";
            }

            return res;
        }

        private DataTable createCAMailNotesTable()
        {
            DataTable notesData = new DataTable();
            notesData = new System.Data.DataTable("Table_Notes");
            DataColumn dc1 = new DataColumn("Category", System.Type.GetType("System.String"));
            DataColumn dc2 = new DataColumn("Comment", System.Type.GetType("System.String"));
            notesData.Columns.Add(dc1);
            notesData.Columns.Add(dc2);
            //Based on PMT
            DataRow drpmt = notesData.NewRow();
            drpmt["Category"] = "Based on PMT";
            drpmt["Comment"] = "基于销售团队的指单";
            notesData.Rows.Add(drpmt);
            //Based on AR
            DataRow drar = notesData.NewRow();
            drar["Category"] = "Based on AR";
            drar["Comment"] = "AR中唯一销账组合";
            notesData.Rows.Add(drar);
            //Based on PTP
            DataRow drptp = notesData.NewRow();
            drptp["Category"] = "Based on Promise to Pay";
            drptp["Comment"] = "销售团队提供的基于发票层级的预计付款日期和金额";
            notesData.Rows.Add(drptp);
            return notesData;
        }


        public List<int> GetAlertAutoSendInvoice(string strCollector, string strdeal, string strLegalEntity, string strCustomerNum, string strSiteUseId, string alertType, string strToTitle, string strToName, string strTempleteLanguage)
        {
            List<int> listInvs = new List<int>();
            List<InvoiceAging> invs = new List<InvoiceAging>();

            string[] strToTitleList = strToTitle.Split(',');
            if (strCustomerNum == null) { strCustomerNum = ""; }
            string[] strCustomerNumList = strCustomerNum.Split(',');
            if (strToName == null) { strToName = ""; }
            string[] strToNameList = strToName.Split(';');

            if (alertType != "5")
            {
                if (strTempleteLanguage == "006")
                {
                    List<string> noterm = (from c in CommonRep.GetQueryable<SysTypeDetail>()
                                           where c.TypeCode == "058"
                                           select c.DetailValue).ToList();
                    if (alertType == "0")
                    {
                        DateTime dtPrePeriodEndDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM") + "-02 00:00:00");
                        invs = (from a in CommonRep.GetQueryable<InvoiceAging>()
                                join b in CommonRep.GetQueryable<CollectorAlert>() on new { SiteUseId = a.SiteUseId } equals new { SiteUseId = b.SiteUseId }
                                where (a.TrackStates != "014"
                                       && a.TrackStates != "016"
                                      )
                                   && !noterm.Contains(a.CreditTremDescription)
                                   && b.Eid.Equals(strCollector)
                                   && b.Deal == strdeal
                                   && b.AlertType.ToString() == alertType
                                   && strCustomerNumList.Contains(b.CustomerNum)
                                   && b.TempleteLanguage == strTempleteLanguage
                                   && strToTitleList.Contains(b.ToTitle)
                                   && strToName == b.ToName
                                   && (b.Status == "Initialized" || (b.Status == "Cancelled" && b.isLasted == true))
                                orderby a.LegalEntity ascending, a.CustomerNum ascending, a.SiteUseId, a.InvoiceDate ascending
                                select a).ToList();
                    }
                    else if (alertType == "1")
                    {
                        DateTime dtPrePeriodEndDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM") + "-02 00:00:00");
                        invs = (from a in CommonRep.GetQueryable<InvoiceAging>()
                                join b in CommonRep.GetQueryable<CollectorAlert>() on new { SiteUseId = a.SiteUseId } equals new { SiteUseId = b.SiteUseId }
                                where ((a.TrackStates != "014"
                                       && a.TrackStates != "016"
                                       && a.CreateDate < dtPrePeriodEndDate
                                        ) ||
                                        (
                                            a.TrackStates == "014" && a.CloseDate > dtPrePeriodEndDate
                                        )
                                   )
                                   && !noterm.Contains(a.CreditTremDescription)
                                   && b.Eid.Equals(strCollector)
                                   && b.Deal == strdeal
                                   && b.AlertType.ToString() == alertType
                                   && strCustomerNumList.Contains(b.CustomerNum)
                                   && b.TempleteLanguage == strTempleteLanguage
                                   && strToTitleList.Contains(b.ToTitle)
                                   && strToName == b.ToName
                                   && (b.Status == "Initialized" || (b.Status == "Cancelled" && b.isLasted == true))
                                orderby a.LegalEntity ascending, a.CustomerNum ascending, a.SiteUseId, a.InvoiceDate ascending
                                select a).ToList();
                    }
                    else
                    {
                        invs = (from a in CommonRep.GetQueryable<InvoiceAging>()
                                join b in CommonRep.GetQueryable<CollectorAlert>() on new { SiteUseId = a.SiteUseId } equals new { SiteUseId = b.SiteUseId }
                                where (a.TrackStates != "014"
                                       && a.TrackStates != "016"
                                   )
                                   && !noterm.Contains(a.CreditTremDescription)
                                   && b.Eid.Equals(strCollector)
                                   && b.Deal == strdeal
                                   && b.AlertType.ToString() == alertType
                                   && strCustomerNumList.Contains(b.CustomerNum)
                                   && b.TempleteLanguage == strTempleteLanguage
                                   && strToTitleList.Contains(b.ToTitle)
                                   && strToName == b.ToName
                                   && b.Status == "Initialized"
                                orderby a.LegalEntity ascending, a.CustomerNum ascending, a.SiteUseId, a.InvoiceDate ascending
                                select a).ToList();
                    }
                }
                if (strTempleteLanguage == "011") //ANZ
                {
                    List<string> noterm = (from c in CommonRep.GetQueryable<SysTypeDetail>()
                                           where c.TypeCode == "058"
                                           select c.DetailValue).ToList();
                    if (alertType == "1")
                    {
                        DateTime dtPrePeriodEndDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM") + "-02 00:00:00");
                        invs = (from a in CommonRep.GetQueryable<InvoiceAging>()
                                join b in CommonRep.GetQueryable<CollectorAlert>() on new { SiteUseId = a.SiteUseId } equals new { SiteUseId = b.SiteUseId }
                                where (a.TrackStates != "014"
                                       && a.TrackStates != "016"
                                       && a.CreateDate < dtPrePeriodEndDate
                                   )
                                   && !noterm.Contains(a.CreditTremDescription)
                                   && b.Eid.Equals(strCollector)
                                   && b.Deal == strdeal
                                   && b.AlertType.ToString() == alertType
                                   && strCustomerNumList.Contains(b.CustomerNum)
                                   && b.TempleteLanguage == strTempleteLanguage
                                   && strToTitleList.Contains(b.ToTitle)
                                   && strToName == b.ToName
                                   && (b.Status == "Initialized" || (b.Status == "Cancelled" && b.isLasted == true))
                                orderby a.LegalEntity ascending, a.CustomerNum ascending, a.SiteUseId, a.InvoiceDate ascending
                                select a).ToList();
                    }
                    else
                    {
                        DateTime dtPrePeriodEndDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM") + "-02 00:00:00");
                        invs = (from a in CommonRep.GetQueryable<InvoiceAging>()
                                join b in CommonRep.GetQueryable<CollectorAlert>() on new { SiteUseId = a.SiteUseId } equals new { SiteUseId = b.SiteUseId }
                                where (a.TrackStates != "014"
                                       && a.TrackStates != "016"
                                       && a.CreateDate < dtPrePeriodEndDate
                                   )
                                   && !noterm.Contains(a.CreditTremDescription)
                                   && b.Eid.Equals(strCollector)
                                   && b.Deal == strdeal
                                   && b.AlertType.ToString() == alertType
                                   && strCustomerNumList.Contains(b.CustomerNum)
                                   && b.TempleteLanguage == strTempleteLanguage
                                   && strToTitleList.Contains(b.ToTitle)
                                   && strToName == b.ToName
                                   && b.Status == "Initialized"
                                orderby a.LegalEntity ascending, a.CustomerNum ascending, a.SiteUseId, a.InvoiceDate ascending
                                select a).ToList();
                    }
                }
                else if (strTempleteLanguage == "007" || strTempleteLanguage == "0071")
                {
                    invs = (from a in CommonRep.GetQueryable<InvoiceAging>()
                            join b in CommonRep.GetQueryable<CollectorAlert>() on new { SiteUseId = a.SiteUseId } equals new { SiteUseId = b.SiteUseId }
                            where a.TrackStates != "014"
                               && a.TrackStates != "016"
                               && b.Eid.Equals(strCollector)
                               && b.Deal == strdeal
                               && b.AlertType.ToString() == alertType
                               && strCustomerNumList.Contains(b.CustomerNum)
                               && b.TempleteLanguage == strTempleteLanguage
                               && strToTitleList.Contains(b.ToTitle)
                               && strToName == b.ToName
                               && b.Status == "Initialized"
                            orderby a.LegalEntity ascending, a.CustomerNum ascending, a.SiteUseId, a.InvoiceDate ascending
                            select a).ToList();
                }
                else
                {
                    invs = (from a in CommonRep.GetQueryable<InvoiceAging>()
                            join b in CommonRep.GetQueryable<CollectorAlert>() on new { SiteUseId = a.SiteUseId } equals new { SiteUseId = b.SiteUseId }
                            where a.TrackStates != "014"
                               && a.TrackStates != "016"
                               && b.Eid.Equals(strCollector)
                               && b.Deal == strdeal
                               && (strCustomerNumList.Contains(b.CustomerNum) || strCustomerNum == "" )
                               && (a.SiteUseId == strSiteUseId || strSiteUseId == "")
                               && b.AlertType.ToString() == alertType
                               && strToTitleList.Contains(b.ToTitle)
                               && (strToNameList.Contains(b.ToName) || strToName == "")
                               && b.Status == "Initialized"
                            orderby a.LegalEntity ascending, a.CustomerNum ascending, a.SiteUseId, a.InvoiceDate ascending
                            select a).ToList();
                }
                if (invs == null)
                {
                    return new List<int>();
                }

                if (alertType == "1" || alertType == "2")
                {
                    if (strToTitle == "CS")
                    {
                        invs = (from inv in invs
                                where (strToNameList.Contains(inv.LsrNameHist) || strToName == "")
                                select inv).ToList();
                    }
                    else if (strToTitle == "Sales")
                    {
                        invs = (from inv in invs
                                where (strToNameList.Contains(inv.FsrNameHist) || strToName == "")
                                select inv).ToList();
                    }
                }
            }
            else
            {
                //PMT
                if (strTempleteLanguage == "006" || strTempleteLanguage == "009" || strTempleteLanguage == "007" || strTempleteLanguage == "0071" || strTempleteLanguage == "011")
                {
                    List<string> noterm = (from c in CommonRep.GetQueryable<SysTypeDetail>()
                                           where c.TypeCode == "058"
                                           select c.DetailValue).ToList();
                    invs = (from a in CommonRep.GetQueryable<InvoiceAging>()
                            join b in CommonRep.GetQueryable<CollectorAlert>() on new { SiteUseId = a.SiteUseId } equals new { SiteUseId = b.SiteUseId }
                            where a.TrackStates != "014"
                               && a.TrackStates != "016"
                               && !noterm.Contains(a.CreditTremDescription)
                               && b.Eid.Equals(strCollector)
                               && b.Deal == strdeal
                               && strCustomerNumList.Contains(b.CustomerNum)
                               && b.TempleteLanguage == strTempleteLanguage
                               && b.AlertType.ToString() == alertType
                               && strToTitleList.Contains(b.ToTitle)
                               && strToName == b.ToName
                               && b.Status == "Initialized"
                            orderby a.LegalEntity ascending, a.CustomerNum ascending, a.SiteUseId, a.InvoiceDate ascending
                            select a).ToList();
                }
                else
                {
                    invs = (from a in CommonRep.GetQueryable<InvoiceAging>()
                            join b in CommonRep.GetQueryable<CollectorAlert>() on new { SiteUseId = a.SiteUseId } equals new { SiteUseId = b.SiteUseId }
                            where a.TrackStates != "014"
                               && a.TrackStates != "016"
                               && b.Eid.Equals(strCollector)
                               && b.Deal == strdeal
                               && b.AlertType.ToString() == alertType
                               && strToTitleList.Contains(b.ToTitle)
                               && strToName == b.ToName
                               && b.Status == "Initialized"
                            orderby a.LegalEntity ascending, a.CustomerNum ascending, a.SiteUseId, a.InvoiceDate ascending
                            select a).ToList();
                }
            }

            if (invs == null)
            {
                return new List<int>();
            }

            int intPeriodEndDays = 0;
            DateTime dt_now = Convert.ToDateTime(AppContext.Current.User.Now.ToString("yyyy-MM-dd"));
            PeriodControl period = CommonRep.GetQueryable<PeriodControl>().Where(O => dt_now >= O.PeriodBegin && dt_now <= O.PeriodEnd).FirstOrDefault();
            DateTime dt_PeriodEndDate = period.EndDate;
            if (dt_PeriodEndDate == null)
            {
                throw new OTCServiceException("Period error !");
            }
            else
            {
                intPeriodEndDays = (Convert.ToDateTime(dt_PeriodEndDate.ToString("yyyy-MM-dd")) - AppContext.Current.User.Now).Days + 1;
            }
            dt_PeriodEndDate = Convert.ToDateTime(dt_PeriodEndDate.ToString("yyyy-MM-dd"));

            CollectorAlert cAlert = CommonRep.GetQueryable<CollectorAlert>().Where(o => o.PeriodId == period.Id && o.AlertType == 1 && o.Status == "Finish").OrderBy(o => o.ActionDate).Select(o => o).FirstOrDefault();
            DateTime dt_MinAlertDate = period.PeriodBegin; ;
            if (cAlert != null && cAlert.ActionDate != null)
            {
                dt_MinAlertDate = cAlert.ActionDate;
            }
            switch (alertType)
            {
                case "0":
                case "1":   //Wave1(All)
                    if (strTempleteLanguage == "007" || strTempleteLanguage == "0071")
                    {
                        listInvs = (from inv in invs
                                    where inv.Class == "INV"
                                    select inv.Id).ToList();
                    }
                    else
                    {
                        listInvs = (from inv in invs
                                    select inv.Id).ToList();
                    }
                    break;
                case "2":   //Wave2(all)
                    if (strTempleteLanguage == "006" || strTempleteLanguage == "008")
                    {
                        listInvs = (from inv in invs
                                    where inv.Class == "INV"
                                    select inv.Id).ToList();
                    }
                    if (strTempleteLanguage == "011")
                    {
                        listInvs = (from inv in invs
                                    where
                                     (inv.PERIOD_ID == period.Id)
                                     && (SqlMethods.DateDiffDay(inv.DueDate, dt_PeriodEndDate) >= 0)
                                     && (inv.OverdueReason == null ? "" : inv.OverdueReason) == ""
                                     && (inv.PtpDate == null ? "" : inv.PtpDate.ToString()) == ""
                                     && (inv.Comments == null ? "" : inv.Comments.ToString()) == ""
                                    select inv.Id).ToList();
                    }
                    else if (strTempleteLanguage == "007" || strTempleteLanguage == "0071")
                    {
                        listInvs = (from inv in invs
                                    where inv.Class == "INV"
                                    select inv.Id).ToList();
                    }
                    else
                    {
                        listInvs = (from inv in invs
                                    where SqlMethods.DateDiffDay(inv.DueDate, dt_PeriodEndDate) >= 0
                                    select inv.Id).ToList();
                    }
                    break;
                case "3":   //Wave3(60+未响应)
                case "4":   //Wave4
                    if (strTempleteLanguage == "006" || strTempleteLanguage == "008")
                    {
                        listInvs = (from inv in invs
                                    where inv.Class == "INV"
                                    select inv.Id).ToList();
                    }
                    else if (strTempleteLanguage == "007" || strTempleteLanguage == "0071")
                    {
                        listInvs = (from inv in invs
                                    where inv.Class == "INV"
                                    && SqlMethods.DateDiffDay(inv.DueDate, dt_PeriodEndDate) >= 0
                                    select inv.Id).ToList();
                    }
                    if (strTempleteLanguage == "011")
                    {
                        listInvs = (from inv in invs
                                    where
                                     (inv.PERIOD_ID == period.Id)
                                     && (SqlMethods.DateDiffDay(inv.DueDate, dt_PeriodEndDate) >= 0)
                                     && (inv.OverdueReason == null ? "" : inv.OverdueReason) == ""
                                     && (inv.PtpDate == null ? "" : inv.PtpDate.ToString()) == ""
                                     && (inv.Comments == null ? "" : inv.Comments.ToString()) == ""
                                    select inv.Id).ToList();
                    }
                    else
                    {
                        listInvs = (from inv in invs
                                    where
                                     (inv.PERIOD_ID == period.Id)
                                     && (SqlMethods.DateDiffDay(inv.DueDate, dt_PeriodEndDate) >= 60)
                                     && (inv.OverdueReason == null ? "" : inv.OverdueReason) == ""
                                     && (inv.PtpDate == null ? "" : inv.PtpDate.ToString()) == ""
                                     && (inv.Comments == null ? "" : inv.Comments.ToString()) == ""
                                    select inv.Id).ToList();
                    }
                    break;
                case "5":   //PMT
                    var notPMT = CommonRep.GetQueryable<SysTypeDetail>().Where(o => o.TypeCode == "048").Select(o => o.DetailName).DefaultIfEmpty().ToList();
                    listInvs = (from inv in invs
                                join c in CommonRep.GetQueryable<CustomerAging>() on new { SiteUseId = inv.SiteUseId } equals new { SiteUseId = c.SiteUseId }
                                where ((inv.Class == "PMT" && !notPMT.Contains(c.CreditTrem == null ? "" : c.CreditTrem)
                                && (inv.hasPmt == null ? "0" : inv.hasPmt) == "0"
                                ) || inv.Class != "PMT")
                                select inv.Id).DefaultIfEmpty().ToList();
                    var listInvsINV = (from inv in invs
                                       where inv.Class == "INV"
                                       select inv.Id).ToList();
                    var listInvsPMT = (from inv in invs
                                       join c in CommonRep.GetQueryable<CustomerAging>() on new { SiteUseId = inv.SiteUseId } equals new { SiteUseId = c.SiteUseId }
                                       where (inv.Class == "PMT" && !notPMT.Contains(c.CreditTrem == null ? "" : c.CreditTrem)
                                       && (inv.hasPmt == null ? "0" : inv.hasPmt) == "0"
                                       )
                                       select inv.Id).ToList();
                    if (listInvsINV == null || listInvsPMT == null || listInvsINV.Count == 0 || listInvsPMT.Count == 0)
                    {
                        listInvs = new List<int>();
                    }
                    break;
            }

            return listInvs;
        }

        public List<int> GetCaPmtMailSendInvoice(string strLegalEntity, string strCustomerNum)
        {
            List<int> listInvs = new List<int>();
            List<InvoiceAging> invs = new List<InvoiceAging>();
            invs = (from a in CommonRep.GetQueryable<InvoiceAging>()
                    where a.TrackStates != "014"
                       && a.Class == "INV"
                       && a.CustomerNum == strCustomerNum
                    orderby a.LegalEntity ascending, a.CustomerNum ascending, a.SiteUseId, a.DueDate ascending
                    select a).ToList();

            listInvs = (from inv in invs
                        select inv.Id).DefaultIfEmpty().ToList();
            if (listInvs.Count >= 0)
            {
                //如果有INV的发票，可以销账时才发PMT Mail
                invs = (from a in CommonRep.GetQueryable<InvoiceAging>()
                        where a.TrackStates != "014"
                           && a.CustomerNum == strCustomerNum
                        orderby a.LegalEntity ascending, a.CustomerNum ascending, a.SiteUseId, a.DueDate ascending
                        select a).ToList();
                listInvs = (from inv in invs
                            select inv.Id).DefaultIfEmpty().ToList();
                return listInvs;
            }
            else
            {
                //没有INV，或只有PMT & CM
                return new List<int>();
            }
        }

        //get all period
        public IEnumerable<PeriodControl> GetAllPeriod()
        {
            var periodQuery = CommonRep.GetDbSet<PeriodControl>().Where(o => o.Deal == AppContext.Current.User.Deal);
            foreach (var p in periodQuery)
            {
                if (p.PeriodBegin <= AppContext.Current.User.Now && AppContext.Current.User.Now <= p.PeriodEnd)
                {
                    p.IsCurrentFlg = "1";
                }
                else
                {
                    p.IsCurrentFlg = "0";
                }
                p.Period = p.PeriodBegin.ToShortDateString() + " ~ " + p.PeriodEnd.ToShortDateString();
            }
            return periodQuery.OrderByDescending(o => o.Id);
        }

        //Create Dunning
        public void CreateDunning(List<string> strCondition, string DunBatchType, string DunAlertType)
        {
            IDunningService service = SpringFactory.GetObjectImpl<IDunningService>("DunningReminderService");
            service.insertDunningReminder(strCondition, DunBatchType, DunAlertType);
        }

        public int CheckPermission(string ColSoa)
        {
            int Check = 0;
            string CurrentUser = AppContext.Current.User.EID.ToString();
            string[] cusGroup = ColSoa.Split(',');
            List<string> collectors = new List<string>();
            collectors = CommonRep.GetDbSet<CustomerTeam>()
                .Where(o => o.Deal == AppContext.Current.User.Deal && cusGroup.Contains(o.CustomerNum))
                .Select(o => o.Collector).ToList();
            foreach (var item in collectors)
            {
                if (item != CurrentUser)
                {
                    Check = 1;
                }
            }
            return Check;
        }

        public IEnumerable<InvoicesStatusDto> GetInvoicesStatusList()
        {
            IQueryable<T_INVOICE_STATUS_STAGING> invoiceStatusList = null;
            IQueryable<InvoiceAging> invoiceList = null;
            IQueryable<SysTypeDetail> sysType = null;
            //List<DisputeDto> groupDisp = new List<DisputeDto>();
            invoiceStatusList = CommonRep.GetDbSet<T_INVOICE_STATUS_STAGING>().Where(o => o.CREATE_USER == AppContext.Current.User.EID);
            invoiceList = CommonRep.GetDbSet<InvoiceAging>().Where(o => o.TrackStates != "014" && o.TrackStates != "016");
            sysType = CommonRep.GetDbSet<SysTypeDetail>().Where(o => o.TypeCode == "025");
            var r = from invstatus in invoiceStatusList
                    join inv in invoiceList on new { SITEUSEID = invstatus.SiteUseId, INVOICE_NO = invstatus.INVOICE_NO } equals new { SITEUSEID = inv.SiteUseId, INVOICE_NO = inv.InvoiceNum }
                    into grps
                    from grp in grps.DefaultIfEmpty()
                    join systype_New in sysType on new { TypeName = invstatus.INVOICE_DISPUTE } equals new { TypeName = systype_New.DetailName }
                    into systype_News
                    from systype_Newss in systype_News.DefaultIfEmpty()
                    select new InvoicesStatusDto
                    {
                        SiteUseId = invstatus.SiteUseId,
                        INVOICE_DATE = invstatus.INVOICE_DATE,
                        INVOICE_NO = invstatus.INVOICE_NO,
                        INVOICE_AMOUNT = invstatus.INVOICE_AMOUNT,
                        INVOICE_Class = invstatus.INVOICE_CLASS,
                        INVOICE_CurrencyCode = invstatus.INVOICE_CurrencyCode == null ? grp.Currency : invstatus.INVOICE_CurrencyCode,
                        INVOICE_LineNo = invstatus.INVOICE_LineNo,
                        INVOICE_MaterialNo = invstatus.INVOICE_MaterialNo,
                        INVOICE_MaterialAmount = invstatus.INVOICE_MaterialAmount,
                        INVOICE_BalanceStatus = invstatus.INVOICE_BalanceStatus,
                        INVOICE_BalanceMemo = invstatus.INVOICE_BalanceMemo,
                        INVOICE_Comments = grp.Comments,
                        INVOICE_DUEDATE = invstatus.INVOICE_DUEDATE,
                        INVOICE_IsForwarder = invstatus.IsForwarder,
                        INVOICE_Forwarder = invstatus.Forwarder,
                        INVOICE_PTPDATE = invstatus.INVOICE_PTPDATE,
                        INVOICE_PTPDATE_OLD = grp.PtpDate,
                        INVOICE_DueReason = invstatus.DueReason,
                        INVOICE_DueReason_OLD = grp.OverdueReason,
                        INVOICE_OTHER = invstatus.INVOICE_OTHER,
                        INVOICE_Status = (grp.TrackStates == "014" ? "Closed" : (grp.TrackStates == "016" ? "Canceled" : "")),
                        MemoExpirationDate = invstatus.MemoExpirationDate
                    };
            return r;
        }

        public List<CustomerCommentStatusDto> GetCustomerCommentStatusData() {
            List<T_Customer_Comments> listNewComments = new List<T_Customer_Comments>();
            List<T_Customer_Comments> listOldComments = new List<T_Customer_Comments>();
            List<CustomerCommentStatusDto> listMergeComments = new List<CustomerCommentStatusDto>();
            string sqlNew = string.Format(@"select SiteUseId,AgingBucket,PTPAmount,PTPDate,ODReason as OverdueReason,Comments,CommentsFrom from T_INVOICE_STATUS_CUSTOMER_STAGING where create_user = '{0}'", AppContext.Current.User.EID);
            listNewComments = SqlHelper.GetList<T_Customer_Comments>(SqlHelper.ExcuteTable(sqlNew,CommandType.Text,null));
            string sqlOld = string.Format(@"select SiteUseId,AgingBucket,PTPAmount,PTPDate,OverdueReason,Comments,CommentsFrom from T_Customer_Comments where isNull(isDeleted,0) = 0 and SiteUseId in (select distinct SiteUseId from T_INVOICE_STATUS_CUSTOMER_STAGING where create_user = '{0}')", AppContext.Current.User.EID);
            listOldComments = SqlHelper.GetList<T_Customer_Comments>(SqlHelper.ExcuteTable(sqlOld, CommandType.Text, null));
            foreach(T_Customer_Comments newItem in listNewComments){
                CustomerCommentStatusDto rowItem = new CustomerCommentStatusDto();
                rowItem.SiteUseId = newItem.SiteUseId;
                rowItem.AgingBucket = newItem.AgingBucket;
                rowItem.PTPAmount = newItem.PTPAmount;
                rowItem.PTPDate = newItem.PTPDATE;
                rowItem.ODReason = newItem.OverdueReason;
                rowItem.Comments = newItem.Comments;
                rowItem.CommentsFrom = newItem.CommentsFrom;
                listMergeComments.Add(rowItem);
            }
            foreach (T_Customer_Comments oldItem in listOldComments)
            {
                CustomerCommentStatusDto find = listMergeComments.Find(o=>o.SiteUseId == oldItem.SiteUseId && o.AgingBucket == oldItem.AgingBucket);
                if (find == null)
                {
                    CustomerCommentStatusDto rowItem = new CustomerCommentStatusDto();
                    rowItem.SiteUseId = oldItem.SiteUseId;
                    rowItem.AgingBucket = oldItem.AgingBucket;
                    rowItem.PTPAmountOld = oldItem.PTPAmount;
                    rowItem.PTPDateOld = oldItem.PTPDATE;
                    rowItem.ODReasonOld = oldItem.OverdueReason;
                    rowItem.CommentsOld = oldItem.Comments;
                    rowItem.CommentsFromOld = oldItem.CommentsFrom;
                    listMergeComments.Add(rowItem);
                }
                else
                {
                    find.PTPAmountOld = oldItem.PTPAmount;
                    find.PTPDateOld = oldItem.PTPDATE;
                    find.ODReasonOld = oldItem.OverdueReason;
                    find.CommentsOld = oldItem.Comments;
                    find.CommentsFromOld = oldItem.CommentsFrom;
                }
            }
            return listMergeComments;
        }
        public string SetInvoicesStatusList()
        {
            try
            {
                DateTime dt_Now = DateTime.Now;
                //先处理Customer Level Comments(批量导入时，滚动删除之前的历史记录，先假删，再插)
                SqlHelper.ExcuteSql(string.Format("delete from T_Customer_Comments where isDeleted = 1 and siteuseid in (select distinct siteuseid from T_INVOICE_STATUS_CUSTOMER_STAGING where CREATE_USER = '{0}')", AppContext.Current.User.EID));
                SqlHelper.ExcuteSql(string.Format("Update T_Customer_Comments set isDeleted = 1 where siteuseid in (select distinct siteuseid from T_INVOICE_STATUS_CUSTOMER_STAGING where CREATE_USER = '{0}')", AppContext.Current.User.EID));
                SqlHelper.ExcuteSql(string.Format(@"Insert into T_Customer_Comments (ID, CUSTOMER_NUM,SiteUseId,AgingBucket, PTPDATE, PTPAmount, OverdueReason, Comments, CommentsFrom, CreateUser, CreateDate)" +
                    "                              (select NEWID(), t_customer.CUSTOMER_NUM, T_INVOICE_STATUS_CUSTOMER_STAGING.siteuseid, T_INVOICE_STATUS_CUSTOMER_STAGING.AgingBucket," +
                    "                                      T_INVOICE_STATUS_CUSTOMER_STAGING.PTPDATE, T_INVOICE_STATUS_CUSTOMER_STAGING.PTPAmount, T_INVOICE_STATUS_CUSTOMER_STAGING.ODReason, T_INVOICE_STATUS_CUSTOMER_STAGING.Comments, T_INVOICE_STATUS_CUSTOMER_STAGING.CommentsFrom, T_INVOICE_STATUS_CUSTOMER_STAGING.CREATE_USER, '{1}' " + 
                    "                                 from T_INVOICE_STATUS_CUSTOMER_STAGING join t_customer on T_INVOICE_STATUS_CUSTOMER_STAGING.siteuseid = t_customer.siteuseid" +
                    "                                where T_INVOICE_STATUS_CUSTOMER_STAGING.CREATE_USER = '{0}' and " +
                    "                                     (T_INVOICE_STATUS_CUSTOMER_STAGING.PTPDATE is not null or " +
                    "                                      T_INVOICE_STATUS_CUSTOMER_STAGING.PTPAmount is not null or " +
                    "                                      T_INVOICE_STATUS_CUSTOMER_STAGING.ODReason <> '' or " +
                    "                                      T_INVOICE_STATUS_CUSTOMER_STAGING.Comments <> '' ) )", AppContext.Current.User.EID, dt_Now));
                List<T_INVOICE_STATUS_CUSTOMER_STAGING> listStatusCustomerStaging = SqlHelper.GetList<T_INVOICE_STATUS_CUSTOMER_STAGING>(SqlHelper.ExcuteTable(string.Format("select distinct SiteUseId from T_INVOICE_STATUS_CUSTOMER_STAGING where CREATE_USER = '{0}'", AppContext.Current.User.EID), CommandType.Text));
                List<string> listSiteUseId = new List<string>();
                foreach (T_INVOICE_STATUS_CUSTOMER_STAGING item in listStatusCustomerStaging) {
                    listSiteUseId.Add(item.SiteUseId);
                }
                CustomerService custService = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
                List<string> listRebuildComments = custService.reBuildComments(listSiteUseId);
                if (listRebuildComments.Count > 0) {
                    SqlHelper.ExcuteListSql(listRebuildComments);
                }
                List<T_INVOICE_STATUS_STAGING> invoiceStatusList = new List<T_INVOICE_STATUS_STAGING>();
                List<InvoiceAging> invoiceList = new List<InvoiceAging>();
                invoiceStatusList = (from stag in CommonRep.GetDbSet<T_INVOICE_STATUS_STAGING>()
                                     join invs in CommonRep.GetDbSet<InvoiceAging>()
                                     on new { SiteUseId = stag.SiteUseId, invno = stag.INVOICE_NO } equals new { SiteUseId = invs.SiteUseId, invno = invs.InvoiceNum }
                                     where stag.CREATE_USER == AppContext.Current.User.EID && (stag.INVOICE_PTPDATE != invs.PtpDate || (stag.DueReason == null ? "" : stag.DueReason) != (invs.OverdueReason == null ? "" : invs.OverdueReason) || (stag.INVOICE_BalanceMemo == null ? "" : stag.INVOICE_BalanceMemo) != (invs.Comments == null ? "" : invs.Comments))
                                     select stag).ToList();

                List<T_PTPPayment> paymentInsertList = new List<T_PTPPayment>();
                List<Dispute> disputeInsertList = new List<Dispute>();
                List<T_Invoice_Detail> invoiceDetailList = new List<T_Invoice_Detail>();

                if (invoiceStatusList.Count == 0) { return "true"; }

                string strInvoiceNo = "", strInvoiceNo_Pre = "";

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    Helper.Log.Info("Start:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    List<InvoiceAging> invoiceAgingChanges = new List<InvoiceAging>();
                    List<T_PTPPayment> ptpPaymentRemoves = new List<T_PTPPayment>();
                    List<T_PTPPayment_Invoice> ptpPaymentInvoiceRemoves = new List<T_PTPPayment_Invoice>();
                    List<InvoiceLog> invLogs = new List<InvoiceLog>();
                    foreach (T_INVOICE_STATUS_STAGING item in invoiceStatusList)
                    {
                        strInvoiceNo = item.INVOICE_NO;
                        string invoiceNoNoLine = item.INVOICE_NO.Replace("-", "");
                        if (item.FILETYPE == "SOA" || item.FILETYPE == "SOA-CN" || item.FILETYPE == "SOA-SAP" || item.FILETYPE == "SOA-India|Asean" || item.FILETYPE == "SOA-HK" || item.FILETYPE == "ANZ")
                        {
                            string strOldValue = "";
                            string strNewValue = "";
                            invoiceList = CommonRep.GetDbSet<InvoiceAging>().Where(o => o.TrackStates != "014" && o.TrackStates != "016" && o.InvoiceNum == item.INVOICE_NO && o.SiteUseId == item.SiteUseId).ToList();
                            foreach (InvoiceAging ageing in invoiceList)
                            {
                                try
                                {

                                    InvoiceLog invLog = new InvoiceLog();
                                    if ((ageing.OverdueReason == null ? "" : ageing.OverdueReason) != (item.DueReason == null ? "" : item.DueReason))
                                    {
                                        strOldValue = ageing.OverdueReason;
                                        strNewValue = item.DueReason;
                                        ageing.OverdueReason = item.DueReason;
                                        if (string.IsNullOrEmpty(ageing.OverdueReason))
                                        {
                                            if (ageing.TrackStates == "001")
                                            {
                                                ageing.TrackStates = "000";
                                            }
                                            invLogs.Add(createInvLog("Clear OverDueReason", strOldValue, strNewValue, ageing));
                                        }
                                        else
                                        {
                                            ageing.TrackStates = "001";
                                            invLogs.Add(createInvLog("Add OverDueReason", strOldValue, strNewValue, ageing));
                                        }
                                    }
                                    if ((ageing.Comments == null ? "" : ageing.Comments) != (item.INVOICE_BalanceMemo == null ? "" : item.INVOICE_BalanceMemo))
                                    {
                                        // INSERT T_INVOICE_AGING_ExpirationDateHis
                                        if (!Convert.ToDateTime(ageing.MemoExpirationDate).Equals(item.MemoExpirationDate))
                                        {
                                            T_INVOICE_AGING_ExpirationDateHis agingExpDateHis = new T_INVOICE_AGING_ExpirationDateHis();

                                            agingExpDateHis.InvID = ageing.Id;
                                            agingExpDateHis.OldMemoExpirationDate = ageing.MemoExpirationDate;
                                            if (item.MemoExpirationDate == null)
                                            {
                                                agingExpDateHis.NewMemoExpirationDate = null;
                                            }
                                            else
                                            {
                                                agingExpDateHis.NewMemoExpirationDate = Convert.ToDateTime(item.MemoExpirationDate);
                                            }
                                            agingExpDateHis.UserId = AppContext.Current.User.EID; //当前用户ID
                                            agingExpDateHis.ChangeDate = DateTime.Now;
                                            CommonRep.Add(agingExpDateHis);
                                        }
                                        strOldValue = ageing.Comments;
                                        strNewValue = item.INVOICE_BalanceMemo;
                                        if (string.IsNullOrEmpty(ageing.Comments))
                                        {
                                            invLogs.Add(createInvLog("Add Comment", strOldValue, strNewValue, ageing));
                                        }
                                        else
                                        {
                                            invLogs.Add(createInvLog("Change Comment", strOldValue, strNewValue, ageing));
                                        }
                                        ageing.BalanceMemo = item.INVOICE_BalanceMemo;
                                        if (item.MemoExpirationDate == null)
                                        {
                                            ageing.MemoExpirationDate = null;
                                        }
                                        else
                                        {
                                            ageing.MemoExpirationDate = Convert.ToDateTime(item.MemoExpirationDate);
                                        }
                                        ageing.Comments = item.INVOICE_BalanceMemo;
                                    }
                                    ageing.TRACK_DATE = AppContext.Current.User.Now;

                                    T_PTPPayment_Invoice ptpInvoiceInvoice = CommonRep.GetDbSet<T_PTPPayment_Invoice>().FirstOrDefault(o => (int)o.InvoiceId == ageing.Id);

                                    if (ptpInvoiceInvoice == null && item.INVOICE_PTPDATE.HasValue == false)
                                    {
                                        //之前没有PTP， 导入的也没有， 不需要任何处理

                                    }
                                    else if (ptpInvoiceInvoice == null && item.INVOICE_PTPDATE.HasValue == true)
                                    {
                                        //之前没有PTP， 导入的有PTP， 添加
                                        T_PTPPayment ptpAdd = new T_PTPPayment
                                        {
                                            Deal = ageing.Deal,
                                            CustomerNum = ageing.CustomerNum,
                                            SiteUseId = ageing.SiteUseId,
                                            PromiseDate = item.INVOICE_PTPDATE,
                                            IsPartialPay = false,
                                            Payer = "",
                                            PromissAmount = item.INVOICE_AMOUNT,
                                            PaymentMethod = "",
                                            PaymentBank = "",
                                            MailId = "",
                                            Contact = "",
                                            Tracker = "",
                                            PTPPaymentType = "PTP",
                                            PTPStatus = "001",
                                            CollectorId = AppContext.Current.User.EID,
                                            CreateTime = AppContext.Current.User.Now
                                        };

                                        invLogs.Add(createInvLog("Add PTP", "", Convert.ToDateTime(item.INVOICE_PTPDATE).ToString("yyyy-MM-dd"), ageing));
                                        CommonRep.GetDbSet<T_PTPPayment>().Add(ptpAdd);
                                        CommonRep.Commit();

                                        ptpInvoiceInvoice = new T_PTPPayment_Invoice();
                                        ptpInvoiceInvoice.InvoiceId = ageing.Id;
                                        ptpInvoiceInvoice.PTPPaymentId = ptpAdd.Id;
                                        CommonRep.GetDbSet<T_PTPPayment_Invoice>().Add(ptpInvoiceInvoice);
                                        CommonRep.Commit();

                                        ageing.PtpDate = item.INVOICE_PTPDATE;
                                        ageing.TrackStates = Helper.EnumToCode<TrackStatus>(TrackStatus.PTP_Confirmed);

                                    }
                                    else
                                    {
                                        //之前有PTP
                                        var ptpPayment = CommonRep.GetDbSet<T_PTPPayment>().FirstOrDefault(o => o.Id == ptpInvoiceInvoice.PTPPaymentId);

                                        if (ptpPayment != null)
                                        {
                                            //如果导入的PTP日期是空，或者和之前的日期不相同，先清空之前的PTP
                                            if (item.INVOICE_PTPDATE.HasValue == false || item.INVOICE_PTPDATE.Value != ptpPayment.PromiseDate)
                                            {
                                                var ptp_payment_invoice_count = CommonRep.GetDbSet<T_PTPPayment_Invoice>().Count(o => o.PTPPaymentId == ptpInvoiceInvoice.PTPPaymentId);

                                                if (ptp_payment_invoice_count == 1)
                                                {
                                                    T_PTPPayment PTPFind = ptpPaymentRemoves.Find(o => o.Id == ptpPayment.Id);
                                                    if (PTPFind == null)
                                                    {
                                                        ptpPaymentRemoves.Add(ptpPayment);
                                                    }
                                                }

                                                T_PTPPayment_Invoice PTPDetailFind = ptpPaymentInvoiceRemoves.Find(o => o.Id == ptpInvoiceInvoice.Id);
                                                if (PTPDetailFind == null)
                                                {
                                                    ptpPaymentInvoiceRemoves.Add(ptpInvoiceInvoice);
                                                }
                                            }
                                        }

                                        if (item.INVOICE_PTPDATE.HasValue)
                                        {
                                            strOldValue = ageing.PtpDate == null ? "" : Convert.ToDateTime(ageing.PtpDate).ToString("yyyy-MM-dd");
                                            strNewValue = item.INVOICE_PTPDATE == null ? "" : Convert.ToDateTime(item.INVOICE_PTPDATE).ToString("yyyy-MM-dd");
                                            invLogs.Add(createInvLog("Change PTP", strOldValue, strNewValue, ageing));
                                            //如果导入 PTP有值， 新添加PTP， 并修改invoice状态
                                            T_PTPPayment ptpAdd = new T_PTPPayment
                                            {
                                                Deal = ageing.Deal,
                                                CustomerNum = ageing.CustomerNum,
                                                SiteUseId = ageing.SiteUseId,
                                                PromiseDate = item.INVOICE_PTPDATE,
                                                IsPartialPay = false,
                                                Payer = "",
                                                PromissAmount = item.INVOICE_AMOUNT,
                                                PaymentMethod = "",
                                                PaymentBank = "",
                                                MailId = "",
                                                Contact = "",
                                                Tracker = "",
                                                PTPPaymentType = "PTP",
                                                PTPStatus = "001",
                                                CollectorId = AppContext.Current.User.EID,
                                                CreateTime = AppContext.Current.User.Now
                                            };
                                            CommonRep.GetDbSet<T_PTPPayment>().Add(ptpAdd);
                                            CommonRep.Commit();

                                            ptpInvoiceInvoice = new T_PTPPayment_Invoice();
                                            ptpInvoiceInvoice.InvoiceId = ageing.Id;
                                            ptpInvoiceInvoice.PTPPaymentId = ptpAdd.Id;
                                            CommonRep.GetDbSet<T_PTPPayment_Invoice>().Add(ptpInvoiceInvoice);
                                            CommonRep.Commit();

                                            ageing.PtpDate = item.INVOICE_PTPDATE;
                                            ageing.TrackStates = Helper.EnumToCode<TrackStatus>(TrackStatus.PTP_Confirmed);

                                        }
                                        else
                                        {
                                            invLogs.Add(createInvLog("Clear PTP", (ageing.PtpDate == null ? "" : Convert.ToDateTime(ageing.PtpDate).ToString("yyyy-MM-dd")), "", ageing));
                                            //如果导入 PTP没有值， 只修改invoice状态
                                            ageing.PtpDate = new Nullable<DateTime>();
                                            ageing.TrackStates = Helper.EnumToCode<TrackStatus>(TrackStatus.Open);

                                        }
                                    }
                                }
                                catch (DbEntityValidationException ex)
                                {
                                    throw ex;
                                }
                                InvoiceAging agingFind = invoiceAgingChanges.Find(o => o.Id == ageing.Id);
                                if (agingFind == null)
                                {
                                    invoiceAgingChanges.Add(ageing);
                                }
                            }
                        }
                    }

                    Helper.Log.Info("Start save:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    CommonRep.RemoveRange(invoiceStatusList);
                    Helper.Log.Info("Start remove ptpPaymentRemoves:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    CommonRep.RemoveRange(ptpPaymentRemoves);
                    Helper.Log.Info("Start remove ptpPaymentInvoiceRemoves:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    CommonRep.RemoveRange(ptpPaymentInvoiceRemoves);
                    Helper.Log.Info("Start save invoiceAgingChanges:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    CommonRep.BulkUpdate(invoiceAgingChanges);
                    CommonRep.BulkInsert(invLogs);
                    Helper.Log.Info("End save:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                    CommonRep.Commit();
                    scope.Complete();

                    Helper.Log.Info("End:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                }
                return "true";
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.InnerException, ex);
                Helper.Log.Error(ex.Message, ex);
                return ex.Message;
            }
        }

        public InvoiceLog createInvLog(string strAction, string oldValue, string newValue, InvoiceAging ageing)
        {
            InvoiceLog invLog = new InvoiceLog();
            invLog.Deal = ageing.Deal;
            invLog.CustomerNum = ageing.CustomerNum;
            invLog.SiteUseId = ageing.SiteUseId;
            invLog.InvoiceId = ageing.Id.ToString();
            invLog.LogDate = AppContext.Current.User.Now;
            invLog.LogPerson = AppContext.Current.User.EID;
            invLog.LogAction = strAction;
            invLog.LogType = "0";
            invLog.OldStatus = ageing.States == null ? "004001" : ageing.States;
            invLog.NewStatus = ageing.States == null ? "004001" : ageing.States;
            invLog.ContactPerson = AppContext.Current.User.EID;
            invLog.NewTrack = ageing.TrackStates;
            invLog.OldTrack = ageing.TrackStates;
            invLog.OldValue = oldValue;
            invLog.NewValue = newValue;
            return invLog;
        }

        public string DelInvoicesStatusData()
        {

            try
            {
                List<T_INVOICE_STATUS_STAGING> invoiceStatusList = new List<T_INVOICE_STATUS_STAGING>();
                invoiceStatusList = CommonRep.GetDbSet<T_INVOICE_STATUS_STAGING>().Where(o => o.CREATE_USER == AppContext.Current.User.EID).ToList();

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    CommonRep.BulkDelete(invoiceStatusList);
                    CommonRep.Commit();

                    scope.Complete();
                }
                return "true";
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                return ex.Message;
            }
        }

        public string saveCustomerAgingComments(string LegalEntity, string CustomerNo, string SiteUseId, string Comments)
        {
            try
            {
                List<CustomerAging> customerAgingList = new List<CustomerAging>();
                customerAgingList = CommonRep.GetDbSet<CustomerAging>().Where(o => o.LegalEntity == LegalEntity && o.CustomerNum == CustomerNo && o.SiteUseId == SiteUseId).ToList();
                foreach (CustomerAging item in customerAgingList)
                {
                    item.Comments = Comments;
                }

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    CommonRep.BulkUpdate(customerAgingList);
                    CommonRep.Commit();

                    scope.Complete();
                }
                return "true";
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                return ex.Message;
            }
        }

        public List<T_LSRFSR_CHANGE> getLSRFSRList()
        {
            DateTime today = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00");
            var lsrfsrList = (from ptp in CommonRep.GetQueryable<T_LSRFSR_CHANGE>()
                              where ptp.CREATEDATE >= today
                              orderby ptp.Collector, ptp.LSRFSRNAME, ptp.SITEUSEID, ptp.LSRFSRTYPE
                              select ptp).ToList();
            return lsrfsrList;
        }

        public List<NoContactorSummary> getNoContactorSummary()
        {
            StringBuilder sbselect = new StringBuilder();
            sbselect.Append(@" SELECT collector, count FROM V_Check_NoCsSales_Summary
                                ORDER BY collector");
            DataTable dt = SqlHelper.ExcuteTable(sbselect.ToString(), System.Data.CommandType.Text, null);
            List<NoContactorSummary> List = SqlHelper.GetList<NoContactorSummary>(dt);
            return List;
        }
        public List<NoContactorSiteUseId> getNoContactorSiteUseId()
        {
            StringBuilder sbselect = new StringBuilder();
            sbselect.Append(@" SELECT collector, title, name, siteuseidlist FROM V_Check_NoCsSales_SiteUseId
                                ORDER BY collector, title,name");
            DataTable dt = SqlHelper.ExcuteTable(sbselect.ToString(), System.Data.CommandType.Text, null);
            List<NoContactorSiteUseId> List = SqlHelper.GetList<NoContactorSiteUseId>(dt);
            return List;
        }
        public List<NoContactor> getNoContactorDetail()
        {
            StringBuilder sbselect = new StringBuilder();
            sbselect.Append(@"SELECT Collector, EbName, CreditTerm, CustomerName, CustomerNum, SiteUseId, Title, Name FROM V_CHECK_NOCSSALES
                                ORDER BY Collector, Title, Name, SiteUseId");
            DataTable dt = SqlHelper.ExcuteTable(sbselect.ToString(), System.Data.CommandType.Text, null);
            List<NoContactor> List = SqlHelper.GetList<NoContactor>(dt);
            return List;
        }

        public List<NewCustomerRemdindingSum> getNewCustomerRemindingSum()
        {
            StringBuilder sbselect = new StringBuilder();
            sbselect.Append(@" SELECT collector, count FROM [dbo].[V_NewCustomerReminding_Summary]
                                ORDER BY collector ");
            DataTable dt = SqlHelper.ExcuteTable(sbselect.ToString(), System.Data.CommandType.Text, null);
            List<NewCustomerRemdindingSum> List = SqlHelper.GetList<NewCustomerRemdindingSum>(dt);
            return List;
        }
        public List<NewCustomerRemdindingDetail> getNewCustomerRemindingDetail()
        {
            StringBuilder sbselect = new StringBuilder();
            sbselect.Append(@" SELECT collector, region, organization,ebname,creditterm, customername, 
                                    customernum,siteuseid,sales, createdate FROM [dbo].[V_NewCustomerReminding_Detail]
                                ORDER BY collector, region, organization, ebname,creditterm,customername,customernum,siteuseid");
            DataTable dt = SqlHelper.ExcuteTable(sbselect.ToString(), System.Data.CommandType.Text, null);
            List<NewCustomerRemdindingDetail> List = SqlHelper.GetList<NewCustomerRemdindingDetail>(dt);
            return List;
        }

        public List<SendPMTSummary> GetPmtSendSummaryList()
        {
            try
            {
                StringBuilder sbselect = new StringBuilder();
                sbselect.Append(@" SELECT collector, success, failed FROM V_SendPMT_Sum ORDER BY collector");
                SqlParameter[] paramForSQL = null;
                DataTable dt = SqlHelper.ExcuteTable(sbselect.ToString(), System.Data.CommandType.Text, paramForSQL);
                List<SendPMTSummary> taskPmtList = SqlHelper.GetList<SendPMTSummary>(dt);
                return taskPmtList;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        public List<TaskPmtDto> GetPmtSendDetailList()
        {

            try
            {
                StringBuilder sbselect = new StringBuilder();
                sbselect.Append(@"SELECT * FROM V_SendPMT_Detail
                                  ORDER BY status, collector, invoicenum ");

                SqlParameter[] paramForSQL = null;
                DataTable dt = SqlHelper.ExcuteTable(sbselect.ToString(), System.Data.CommandType.Text, paramForSQL);
                List<TaskPmtDto> taskPmtList = SqlHelper.GetList<TaskPmtDto>(dt);

                return taskPmtList;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        public List<SendSoaRemindingDetail> GetSoaSendWarningDetail()
        {

            try
            {
                StringBuilder sbselect = new StringBuilder();
                sbselect.Append(@" SELECT status,
                                           eid,
                                           region, 
                                           totitle,
                                           toname,
                                           cctitle,
                                           comment,
                                           siteuseidlist,
                                           mailfrom,
                                           mailto,
                                           mailcc,
                                           mailsubject
                                    FROM [dbo].[V_SendSoaReminding_Detail]
                                   WHERE (region not like 'ASEAN%')
                                    ORDER BY STATUS ASC, eid ASC  ");
                SqlParameter[] paramForSQL = null;
                DataTable dt = SqlHelper.ExcuteTable(sbselect.ToString(), System.Data.CommandType.Text, paramForSQL);
                List<SendSoaRemindingDetail> warningList = SqlHelper.GetList<SendSoaRemindingDetail>(dt);

                return warningList;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        public List<SendSoaRemindingSum> GetSoaSendWarningSum()
        {

            try
            {
                StringBuilder sbselect = new StringBuilder();
                sbselect.Append(@" SELECT status, region, eid, count FROM V_SendSoaReminding_Sum
                                   WHERE (region not like 'ASEAN%') AND REGION <> 'HK'
                                    ORDER BY STATUS ASC, eid ASC; ");
                SqlParameter[] paramForSQL = null;
                DataTable dt = SqlHelper.ExcuteTable(sbselect.ToString(), System.Data.CommandType.Text, paramForSQL);
                List<SendSoaRemindingSum> warningList = SqlHelper.GetList<SendSoaRemindingSum>(dt);

                return warningList;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }


        public List<SendSoaRemindingDetail> GetSoaSendWarningDetailASEAN()
        {

            try
            {
                StringBuilder sbselect = new StringBuilder();
                sbselect.Append(@" SELECT status,
                                           eid,
                                           region,
                                           totitle,
                                           toname,
                                           cctitle,
                                           comment,
                                           siteuseidlist,
                                           mailfrom,
                                           mailto,
                                           mailcc,
                                           mailsubject
                                    FROM [dbo].[V_SendSoaReminding_Detail]
                                   WHERE (region like 'ASEAN%') or (region = 'HK')
                                    ORDER BY STATUS ASC, eid ASC  ");
                SqlParameter[] paramForSQL = null;
                DataTable dt = SqlHelper.ExcuteTable(sbselect.ToString(), System.Data.CommandType.Text, paramForSQL);
                List<SendSoaRemindingDetail> warningList = SqlHelper.GetList<SendSoaRemindingDetail>(dt);

                return warningList;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        public List<SendSoaRemindingDetail> GetSoaSendWarningDetailANZ()
        {

            try
            {
                StringBuilder sbselect = new StringBuilder();
                sbselect.Append(@" SELECT status,
                                           eid,
                                           region,
                                           totitle,
                                           toname,
                                           cctitle,
                                           comment,
                                           siteuseidlist,
                                           mailfrom,
                                           mailto,
                                           mailcc,
                                           mailsubject
                                    FROM [dbo].[V_SendSoaReminding_Detail]
                                   WHERE region = 'ANZ'
                                    ORDER BY STATUS ASC, eid ASC  ");
                SqlParameter[] paramForSQL = null;
                DataTable dt = SqlHelper.ExcuteTable(sbselect.ToString(), System.Data.CommandType.Text, paramForSQL);
                List<SendSoaRemindingDetail> warningList = SqlHelper.GetList<SendSoaRemindingDetail>(dt);

                return warningList;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }
        public List<SendSoaRemindingSum> GetSoaSendWarningSumASEAN()
        {

            try
            {
                StringBuilder sbselect = new StringBuilder();
                sbselect.Append(@" SELECT status, region, eid, count FROM V_SendSoaReminding_Sum
                                   WHERE (region like 'ASEAN%') or (region = 'HK')
                                    ORDER BY STATUS ASC, eid ASC; ");
                SqlParameter[] paramForSQL = null;
                DataTable dt = SqlHelper.ExcuteTable(sbselect.ToString(), System.Data.CommandType.Text, paramForSQL);
                List<SendSoaRemindingSum> warningList = SqlHelper.GetList<SendSoaRemindingSum>(dt);

                return warningList;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        public List<SendSoaRemindingSum> GetSoaSendWarningSumANZ()
        {

            try
            {
                StringBuilder sbselect = new StringBuilder();
                sbselect.Append(@" SELECT status, region, eid, count FROM V_SendSoaReminding_Sum
                                   WHERE region = 'ANZ'
                                    ORDER BY STATUS ASC, eid ASC; ");
                SqlParameter[] paramForSQL = null;
                DataTable dt = SqlHelper.ExcuteTable(sbselect.ToString(), System.Data.CommandType.Text, paramForSQL);
                List<SendSoaRemindingSum> warningList = SqlHelper.GetList<SendSoaRemindingSum>(dt);

                return warningList;
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        public List<MyinvoicesDto> GetInvoiceByIds(List<int> ids)
        {
            try
            {
                var invList = (from x in CommonRep.GetQueryable<InvoiceAging>()
                               join y in CommonRep.GetQueryable<Customer>()
                   on x.SiteUseId equals y.SiteUseId
                   into xy
                               where ids.Contains(x.Id)
                               from y in xy.DefaultIfEmpty()
                               orderby x.Class, x.DaysLateSys descending, x.InvoiceDate, x.InvoiceNum
                               select new MyinvoicesDto
                               {
                                   CustomerName = y.CustomerName,
                                   CustomerNum = y.CustomerNum,
                                   SiteUseId = y.SiteUseId,
                                   Class = x.Class,
                                   InvoiceNum = x.InvoiceNum,
                                   InvoiceDate = x.InvoiceDate,
                                   DueDate = x.DueDate,
                                   Currency = x.Currency,
                                   FuncCurrCode = x.FuncCurrCode,
                                   BalanceAmt = x.BalanceAmt,
                                   CreditTrem = y.CreditTrem,
                                   Ebname = y.Ebname,
                                   LegalEntity = y.Organization
                               }).ToList();
                return invList;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public string CreateSendPMTAttachment(List<TaskPmtDto> sendDetail)
        {
            string tempFileName = "";
            string fileName = "";
            string templateFile = "";
            string lstReportPath = "";
            fileName = "PMT Reminding-" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx";
            templateFile = ConfigurationManager.AppSettings["TemplateSendPMTReminding"].ToString().TrimStart('~').Replace("/", "\\").TrimStart('\\');
            templateFile = Path.Combine(HttpRuntime.AppDomainAppPath, templateFile);
            //按模板生成临时文件
            tempFileName = Path.Combine(Path.GetTempPath(), "PMT Reminding-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xlsx");
            NpoiHelper helper = new NpoiHelper(templateFile);
            helper.Save(tempFileName, true);
            helper = new NpoiHelper(tempFileName);
            helper.ActiveSheet = 0;
            ISheet sheet0 = helper.Book.GetSheetAt(0);

            ICellStyle cellStyles = helper.Book.CreateCellStyle();
            cellStyles.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyles.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyles.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyles.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;

            ICellStyle cellStylesfailed = helper.Book.CreateCellStyle();
            cellStylesfailed.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStylesfailed.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStylesfailed.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStylesfailed.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStylesfailed.FillBackgroundColor = HSSFColor.Yellow.Index;
            cellStylesfailed.FillPattern = FillPattern.SolidForeground;
            cellStylesfailed.FillForegroundColor = HSSFColor.Yellow.Index;

            ISheet sheet = helper.Book.GetSheetAt(0);
            int rowNumber = 1;
            foreach (TaskPmtDto rowData in sendDetail)
            {
                ICellStyle cellStylesCurrent = helper.Book.CreateCellStyle();
                //Result
                if (string.IsNullOrEmpty(rowData.status)) { rowData.status = ""; }

                if (rowData.status == "失败")
                {
                    cellStylesCurrent = cellStylesfailed;
                }
                else
                {
                    cellStylesCurrent = cellStyles;
                }

                helper.SetData(rowNumber, 0, rowData.status);
                ICell cell0 = helper.GetCell(rowNumber, 0);
                cell0.CellStyle = cellStylesCurrent;
                //Collector
                if (string.IsNullOrEmpty(rowData.Collector)) { rowData.Collector = ""; }
                helper.SetData(rowNumber, 1, rowData.Collector);
                ICell cell1 = helper.GetCell(rowNumber, 1);
                cell1.CellStyle = cellStylesCurrent;
                //OrgId
                if (string.IsNullOrEmpty(rowData.LegalEntity)) { rowData.LegalEntity = ""; }
                helper.SetData(rowNumber, 2, rowData.LegalEntity);
                ICell cell2 = helper.GetCell(rowNumber, 2);
                cell2.CellStyle = cellStylesCurrent;
                //CustomerName
                if (string.IsNullOrEmpty(rowData.CustomerName)) { rowData.CustomerName = ""; }
                helper.SetData(rowNumber, 3, rowData.CustomerName);
                ICell cell3 = helper.GetCell(rowNumber, 3);
                cell3.CellStyle = cellStylesCurrent;
                //CustomerNum
                if (string.IsNullOrEmpty(rowData.CustomerNum)) { rowData.CustomerNum = ""; }
                helper.SetData(rowNumber, 4, rowData.CustomerNum);
                ICell cell4 = helper.GetCell(rowNumber, 4);
                cell4.CellStyle = cellStylesCurrent;
                //SiteUseId
                if (string.IsNullOrEmpty(rowData.SiteUseId)) { rowData.SiteUseId = ""; }
                helper.SetData(rowNumber, 5, rowData.SiteUseId);
                ICell cell5 = helper.GetCell(rowNumber, 5);
                cell5.CellStyle = cellStylesCurrent;
                //Class
                if (string.IsNullOrEmpty(rowData.Class)) { rowData.Class = ""; }
                helper.SetData(rowNumber, 6, rowData.Class);
                ICell cell6 = helper.GetCell(rowNumber, 6);
                cell6.CellStyle = cellStylesCurrent;
                //CustomerNum
                if (string.IsNullOrEmpty(rowData.InvoiceNum)) { rowData.InvoiceNum = ""; }
                helper.SetData(rowNumber, 7, rowData.InvoiceNum);
                ICell cell7 = helper.GetCell(rowNumber, 7);
                cell7.CellStyle = cellStylesCurrent;
                //InvoiceDate
                helper.SetData(rowNumber, 8, rowData.InvoiceDate);
                ICell cell8 = helper.GetCell(rowNumber, 8);
                cell8.CellStyle = cellStylesCurrent;
                //Currency
                if (string.IsNullOrEmpty(rowData.Currency)) { rowData.Currency = ""; }
                helper.SetData(rowNumber, 9, rowData.Currency);
                ICell cell9 = helper.GetCell(rowNumber, 9);
                cell9.CellStyle = cellStylesCurrent;
                //BalanceAmt
                if (rowData.BalanceAmt == null) { rowData.BalanceAmt = 0; }
                helper.SetData(rowNumber, 10, rowData.BalanceAmt);
                ICell cell10 = helper.GetCell(rowNumber, 10);
                cell10.CellStyle = cellStylesCurrent;
                //CS
                if (string.IsNullOrEmpty(rowData.LsrNameHist)) { rowData.LsrNameHist = ""; }
                helper.SetData(rowNumber, 11, rowData.LsrNameHist);
                ICell cell11 = helper.GetCell(rowNumber, 11);
                cell11.CellStyle = cellStylesCurrent;
                //Sales
                if (string.IsNullOrEmpty(rowData.FsrNameHist)) { rowData.FsrNameHist = ""; }
                helper.SetData(rowNumber, 12, rowData.FsrNameHist);
                ICell cell12 = helper.GetCell(rowNumber, 12);
                cell12.CellStyle = cellStylesCurrent;
                //TrackStatus
                helper.SetData(rowNumber, 13, rowData.TrackStatusName);
                ICell cell13 = helper.GetCell(rowNumber, 13);
                cell13.CellStyle = cellStylesCurrent;
                //TrackDate
                helper.SetData(rowNumber, 14, rowData.TrackDate);
                ICell cell14 = helper.GetCell(rowNumber, 14);
                cell14.CellStyle = cellStylesCurrent;
                //Comment
                if (string.IsNullOrEmpty(rowData.Comments)) { rowData.Comments = ""; }
                helper.SetData(rowNumber, 15, rowData.Comments);
                ICell cell15 = helper.GetCell(rowNumber, 15);
                cell15.CellStyle = cellStylesCurrent;
                rowNumber++;
            }
            helper.Save(tempFileName, true);
            FileService fs = SpringFactory.GetObjectImpl<FileService>("FileService");
            using (FileStream stream = File.OpenRead(tempFileName))
            {
                lstReportPath = fs.AddAppFile(fileName, stream, FileType.SOA).FileId;
            }
            if (File.Exists(tempFileName))
            {
                File.Delete(tempFileName);
            }
            return lstReportPath;
        }

        public string CreateSendSoaRemindingAttachment(List<SendSoaRemindingDetail> sendDetail)
        {
            string tempFileName = "";
            string fileName = "";
            string templateFile = "";
            string lstReportPath = "";
            fileName = "Send Soa Reminding-" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx";
            templateFile = ConfigurationManager.AppSettings["TemplateSendSOAReminding"].ToString().TrimStart('~').Replace("/", "\\").TrimStart('\\');
            templateFile = Path.Combine(HttpRuntime.AppDomainAppPath, templateFile);
            //按模板生成临时文件
            tempFileName = Path.Combine(Path.GetTempPath(), "Send Soa Reminding-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xlsx");
            NpoiHelper helper = new NpoiHelper(templateFile);
            helper.Save(tempFileName, true);
            helper = new NpoiHelper(tempFileName);
            helper.ActiveSheet = 0;
            ISheet sheet0 = helper.Book.GetSheetAt(0);

            ICellStyle cellStyles = helper.Book.CreateCellStyle();
            cellStyles.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyles.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyles.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyles.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;

            ICellStyle cellStylesfailed = helper.Book.CreateCellStyle();
            cellStylesfailed.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStylesfailed.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStylesfailed.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStylesfailed.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStylesfailed.FillBackgroundColor = HSSFColor.Yellow.Index;
            cellStylesfailed.FillPattern = FillPattern.SolidForeground;
            cellStylesfailed.FillForegroundColor = HSSFColor.Yellow.Index;

            ISheet sheet = helper.Book.GetSheetAt(0);
            int rowNumber = 1;
            foreach (SendSoaRemindingDetail rowData in sendDetail)
            {
                ICellStyle cellStylesCurrent = helper.Book.CreateCellStyle();
                //Result
                if (string.IsNullOrEmpty(rowData.status)) { rowData.status = ""; }

                if (rowData.status == "失败")
                {
                    cellStylesCurrent = cellStylesfailed;
                }
                else
                {
                    cellStylesCurrent = cellStyles;
                }

                helper.SetData(rowNumber, 0, rowData.status);
                ICell cell0 = helper.GetCell(rowNumber, 0);
                cell0.CellStyle = cellStylesCurrent;
                //Eid
                if (string.IsNullOrEmpty(rowData.eid)) { rowData.eid = ""; }
                helper.SetData(rowNumber, 1, rowData.eid);
                ICell cell1 = helper.GetCell(rowNumber, 1);
                cell1.CellStyle = cellStylesCurrent;
                //Region
                if (string.IsNullOrEmpty(rowData.region)) { rowData.region = ""; }
                helper.SetData(rowNumber, 2, rowData.region);
                ICell cell2 = helper.GetCell(rowNumber, 2);
                cell2.CellStyle = cellStylesCurrent;
                //To Title
                if (string.IsNullOrEmpty(rowData.totitle)) { rowData.totitle = ""; }
                helper.SetData(rowNumber, 3, rowData.totitle);
                ICell cell3 = helper.GetCell(rowNumber, 3);
                cell3.CellStyle = cellStylesCurrent;
                //To Name
                if (string.IsNullOrEmpty(rowData.toname)) { rowData.toname = ""; }
                helper.SetData(rowNumber, 4, rowData.toname);
                ICell cell4 = helper.GetCell(rowNumber, 4);
                cell4.CellStyle = cellStylesCurrent;
                //CC Title
                if (string.IsNullOrEmpty(rowData.cctitle)) { rowData.cctitle = ""; }
                helper.SetData(rowNumber, 5, rowData.cctitle);
                ICell cell5 = helper.GetCell(rowNumber, 5);
                cell5.CellStyle = cellStylesCurrent;
                //Comment
                if (string.IsNullOrEmpty(rowData.comment)) { rowData.comment = ""; }
                helper.SetData(rowNumber, 6, rowData.comment);
                ICell cell6 = helper.GetCell(rowNumber, 6);
                cell6.CellStyle = cellStylesCurrent;
                //SiteUseId
                if (string.IsNullOrEmpty(rowData.siteuseidlist)) { rowData.siteuseidlist = ""; }
                helper.SetData(rowNumber, 7, rowData.siteuseidlist);
                ICell cell7 = helper.GetCell(rowNumber, 7);
                cell7.CellStyle = cellStylesCurrent;
                //Mail From
                if (string.IsNullOrEmpty(rowData.mailfrom)) { rowData.mailfrom = ""; }
                helper.SetData(rowNumber, 8, rowData.mailfrom);
                ICell cell8 = helper.GetCell(rowNumber, 8);
                cell8.CellStyle = cellStylesCurrent;
                //Mail To
                if (string.IsNullOrEmpty(rowData.mailto)) { rowData.mailto = ""; }
                helper.SetData(rowNumber, 9, rowData.mailto);
                ICell cell9 = helper.GetCell(rowNumber, 9);
                cell9.CellStyle = cellStylesCurrent;
                //Mail CC
                if (string.IsNullOrEmpty(rowData.mailcc)) { rowData.mailcc = ""; }
                helper.SetData(rowNumber, 10, rowData.mailcc);
                ICell cell10 = helper.GetCell(rowNumber, 10);
                cell10.CellStyle = cellStylesCurrent;
                //Mail Subject
                if (string.IsNullOrEmpty(rowData.mailsubject)) { rowData.mailsubject = ""; }
                helper.SetData(rowNumber, 11, rowData.mailsubject);
                ICell cell11 = helper.GetCell(rowNumber, 11);
                cell11.CellStyle = cellStylesCurrent;
                rowNumber++;
            }
            helper.Save(tempFileName, true);
            FileService fs = SpringFactory.GetObjectImpl<FileService>("FileService");
            using (FileStream stream = File.OpenRead(tempFileName))
            {
                lstReportPath = fs.AddAppFile(fileName, stream, FileType.SOA).FileId;
            }
            File.Delete(tempFileName);
            return lstReportPath;
        }

        public string CreateNoCsSalesRemindingAttachment(List<NoContactorSiteUseId> NoContactorSiteUseIdList, List<NoContactor> NoContactorDetailList)
        {
            string tempFileName = "";
            string fileName = "";
            string templateFile = "";
            string lstReportPath = "";
            fileName = "No Cs&Sales Reminding-" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx";
            templateFile = ConfigurationManager.AppSettings["TemplateNoCsSalesReminding"].ToString().TrimStart('~').Replace("/", "\\").TrimStart('\\');
            templateFile = Path.Combine(HttpRuntime.AppDomainAppPath, templateFile);
            //按模板生成临时文件
            tempFileName = Path.Combine(Path.GetTempPath(), "No Cs&Sales Reminding-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xlsx");
            NpoiHelper helper = new NpoiHelper(templateFile);
            helper.Save(tempFileName, true);
            helper = new NpoiHelper(tempFileName);
            helper.ActiveSheet = 0;
            ISheet sheet0 = helper.Book.GetSheetAt(0);

            ICellStyle cellStyles = helper.Book.CreateCellStyle();
            cellStyles.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyles.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyles.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyles.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;

            int rowNumber = 1;

            helper.ActiveSheet = 0;
            foreach (NoContactorSiteUseId rowData in NoContactorSiteUseIdList)
            {

                //Collector
                if (string.IsNullOrEmpty(rowData.Collector)) { rowData.Collector = ""; }
                helper.SetData(rowNumber, 0, rowData.Collector);
                ICell cell0 = helper.GetCell(rowNumber, 0);
                cell0.CellStyle = cellStyles;
                //Title
                if (string.IsNullOrEmpty(rowData.Title)) { rowData.Title = ""; }
                helper.SetData(rowNumber, 1, rowData.Title);
                ICell cell1 = helper.GetCell(rowNumber, 1);
                cell1.CellStyle = cellStyles;
                //Name
                if (string.IsNullOrEmpty(rowData.Name)) { rowData.Name = ""; }
                helper.SetData(rowNumber, 2, rowData.Name);
                ICell cell2 = helper.GetCell(rowNumber, 2);
                cell2.CellStyle = cellStyles;
                //SiteUseId List
                if (string.IsNullOrEmpty(rowData.SiteUseIdList)) { rowData.SiteUseIdList = ""; }
                helper.SetData(rowNumber, 3, rowData.SiteUseIdList);
                ICell cell3 = helper.GetCell(rowNumber, 3);
                cell3.CellStyle = cellStyles;

                rowNumber++;
            }

            helper.ActiveSheet = 1;
            rowNumber = 1;
            foreach (NoContactor rowData in NoContactorDetailList)
            {
                //Collector
                if (string.IsNullOrEmpty(rowData.Collector)) { rowData.Collector = ""; }
                helper.SetData(rowNumber, 0, rowData.Collector);
                ICell cell0 = helper.GetCell(rowNumber, 0);
                cell0.CellStyle = cellStyles;
                //EBName
                if (string.IsNullOrEmpty(rowData.EbName)) { rowData.EbName = ""; }
                helper.SetData(rowNumber, 1, rowData.EbName);
                ICell cell1 = helper.GetCell(rowNumber, 1);
                cell1.CellStyle = cellStyles;
                //CreditTerm
                if (string.IsNullOrEmpty(rowData.CreditTerm)) { rowData.CreditTerm = ""; }
                helper.SetData(rowNumber, 2, rowData.CreditTerm);
                ICell cell2 = helper.GetCell(rowNumber, 2);
                cell2.CellStyle = cellStyles;
                //CustomerName
                if (string.IsNullOrEmpty(rowData.CustomerName)) { rowData.CustomerName = ""; }
                helper.SetData(rowNumber, 3, rowData.CustomerName);
                ICell cell3 = helper.GetCell(rowNumber, 3);
                cell3.CellStyle = cellStyles;
                //CustomerNum
                if (string.IsNullOrEmpty(rowData.CustomerNum)) { rowData.CustomerNum = ""; }
                helper.SetData(rowNumber, 4, rowData.CustomerNum);
                ICell cell4 = helper.GetCell(rowNumber, 4);
                cell4.CellStyle = cellStyles;
                //SiteUseId
                if (string.IsNullOrEmpty(rowData.SiteUseId)) { rowData.SiteUseId = ""; }
                helper.SetData(rowNumber, 5, rowData.SiteUseId);
                ICell cell5 = helper.GetCell(rowNumber, 5);
                cell5.CellStyle = cellStyles;
                //Title
                if (string.IsNullOrEmpty(rowData.Title)) { rowData.Title = ""; }
                helper.SetData(rowNumber, 6, rowData.Title);
                ICell cell6 = helper.GetCell(rowNumber, 6);
                cell6.CellStyle = cellStyles;
                //Name
                if (string.IsNullOrEmpty(rowData.Name)) { rowData.Name = ""; }
                helper.SetData(rowNumber, 7, rowData.Name);
                ICell cell7 = helper.GetCell(rowNumber, 7);
                cell7.CellStyle = cellStyles;

                rowNumber++;
            }

            helper.Book.SetActiveSheet(0);
            helper.Save(tempFileName, true);
            FileService fs = SpringFactory.GetObjectImpl<FileService>("FileService");
            using (FileStream stream = File.OpenRead(tempFileName))
            {
                lstReportPath = fs.AddAppFile(fileName, stream, FileType.SOA).FileId;
            }
            File.Delete(tempFileName);
            return lstReportPath;
        }

        public string CreateNewCustomerRemindingAttachment(List<NewCustomerRemdindingDetail> newCustomerDetail)
        {
            string tempFileName = "";
            string fileName = "";
            string templateFile = "";
            string lstReportPath = "";
            fileName = "NewCustomer Reminding-" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx";
            templateFile = ConfigurationManager.AppSettings["TemplateNewCustomerReminding"].ToString().TrimStart('~').Replace("/", "\\").TrimStart('\\');
            templateFile = Path.Combine(HttpRuntime.AppDomainAppPath, templateFile);
            //按模板生成临时文件
            tempFileName = Path.Combine(Path.GetTempPath(), "NewCustomer Reminding-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xlsx");
            NpoiHelper helper = new NpoiHelper(templateFile);
            helper.Save(tempFileName, true);
            helper = new NpoiHelper(tempFileName);
            helper.ActiveSheet = 0;
            ISheet sheet0 = helper.Book.GetSheetAt(0);

            ICellStyle cellStyles = helper.Book.CreateCellStyle();
            cellStyles.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyles.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyles.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyles.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;

            ICellStyle cellStylesfailed = helper.Book.CreateCellStyle();
            cellStylesfailed.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStylesfailed.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStylesfailed.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStylesfailed.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStylesfailed.FillBackgroundColor = HSSFColor.Yellow.Index;
            cellStylesfailed.FillPattern = FillPattern.SolidForeground;
            cellStylesfailed.FillForegroundColor = HSSFColor.Yellow.Index;

            ISheet sheet = helper.Book.GetSheetAt(0);
            int rowNumber = 1;
            foreach (NewCustomerRemdindingDetail rowData in newCustomerDetail)
            {
                ICellStyle cellStylesCurrent = helper.Book.CreateCellStyle();
                //Collector
                if (string.IsNullOrEmpty(rowData.Collector)) { rowData.Collector = ""; }

                if (rowData.Collector == "***未分配***")
                {
                    cellStylesCurrent = cellStylesfailed;
                }
                else
                {
                    cellStylesCurrent = cellStyles;
                }

                helper.SetData(rowNumber, 0, rowData.Collector);
                ICell cell0 = helper.GetCell(rowNumber, 0);
                cell0.CellStyle = cellStylesCurrent;
                //Region
                if (string.IsNullOrEmpty(rowData.Region)) { rowData.Region = ""; }
                helper.SetData(rowNumber, 1, rowData.Region);
                ICell cell1 = helper.GetCell(rowNumber, 1);
                cell1.CellStyle = cellStylesCurrent;
                //Organization
                if (string.IsNullOrEmpty(rowData.Organization)) { rowData.Organization = ""; }
                helper.SetData(rowNumber, 2, rowData.Organization);
                ICell cell2 = helper.GetCell(rowNumber, 2);
                cell2.CellStyle = cellStylesCurrent;
                //Ebname
                if (string.IsNullOrEmpty(rowData.Ebname)) { rowData.Ebname = ""; }
                helper.SetData(rowNumber, 3, rowData.Ebname);
                ICell cell3 = helper.GetCell(rowNumber, 3);
                cell3.CellStyle = cellStylesCurrent;
                //CreditTerm
                if (string.IsNullOrEmpty(rowData.CreditTerm)) { rowData.CreditTerm = ""; }
                helper.SetData(rowNumber, 4, rowData.CreditTerm);
                ICell cell4 = helper.GetCell(rowNumber, 4);
                cell4.CellStyle = cellStylesCurrent;
                //CustomerName
                if (string.IsNullOrEmpty(rowData.CustomerName)) { rowData.CustomerName = ""; }
                helper.SetData(rowNumber, 5, rowData.CustomerName);
                ICell cell5 = helper.GetCell(rowNumber, 5);
                cell5.CellStyle = cellStylesCurrent;
                //CustomerNum
                if (string.IsNullOrEmpty(rowData.CustomerNum)) { rowData.CustomerNum = ""; }
                helper.SetData(rowNumber, 6, rowData.CustomerNum);
                ICell cell6 = helper.GetCell(rowNumber, 6);
                cell6.CellStyle = cellStylesCurrent;
                //SiteUseId
                if (string.IsNullOrEmpty(rowData.SiteUseId)) { rowData.SiteUseId = ""; }
                helper.SetData(rowNumber, 7, rowData.SiteUseId);
                ICell cell7 = helper.GetCell(rowNumber, 7);
                cell7.CellStyle = cellStylesCurrent;
                //Sales
                if (string.IsNullOrEmpty(rowData.Sales)) { rowData.Sales = ""; }
                helper.SetData(rowNumber, 8, rowData.Sales);
                ICell cell8 = helper.GetCell(rowNumber, 8);
                cell8.CellStyle = cellStylesCurrent;
                //CreateDate
                helper.SetData(rowNumber, 9, rowData.CreateDate);
                ICell cell9 = helper.GetCell(rowNumber, 9);
                cell9.CellStyle = cellStylesCurrent;
                rowNumber++;
            }
            helper.Save(tempFileName, true);
            FileService fs = SpringFactory.GetObjectImpl<FileService>("FileService");
            using (FileStream stream = File.OpenRead(tempFileName))
            {
                lstReportPath = fs.AddAppFile(fileName, stream, FileType.SOA).FileId;
            }
            File.Delete(tempFileName);
            return lstReportPath;
        }


        public int BuildContactorByAlert(string deal, string region, string eid, string templeteLanguage, int periodId, int alertType, string customerNum, string toTitle, string toName, string ccTitle)
        {

            List<Exception> listException = new List<Exception>();
            try
            {
                //DEAL Parameter
                var paramBuildDEAL = new SqlParameter
                {
                    ParameterName = "@DEAL",
                    Value = deal,
                    Direction = ParameterDirection.Input
                };
                //Region Parameter
                var paramBuildRegion = new SqlParameter
                {
                    ParameterName = "@Region",
                    Value = region,
                    Direction = ParameterDirection.Input
                };
                //Collector Parameter
                var paramBuildeid = new SqlParameter
                {
                    ParameterName = "@Eid",
                    Value = eid,
                    Direction = ParameterDirection.Input
                };
                //TempleteLanguage Parameter
                var paramBuildTempleteLanguage = new SqlParameter
                {
                    ParameterName = "@TempleteLanguage",
                    Value = templeteLanguage,
                    Direction = ParameterDirection.Input
                };
                //PeriodId Parameter
                var paramBuildPeriodId = new SqlParameter
                {
                    ParameterName = "@PeriodId",
                    Value = periodId,
                    Direction = ParameterDirection.Input
                };
                //AlertType Parameter
                var paramBuildAlertType = new SqlParameter
                {
                    ParameterName = "@AlertType",
                    Value = alertType,
                    Direction = ParameterDirection.Input
                };
                //CustomerNum Parameter
                var paramBuildCustomerNum = new SqlParameter
                {
                    ParameterName = "@CustomerNum",
                    Value = customerNum,
                    Direction = ParameterDirection.Input
                };
                //ToTitle Parameter
                var paramBuildToTitle = new SqlParameter
                {
                    ParameterName = "@ToTitle",
                    Value = toTitle,
                    Direction = ParameterDirection.Input
                };
                //ToName Parameter
                var paramBuildToName = new SqlParameter
                {
                    ParameterName = "@ToName",
                    Value = toName,
                    Direction = ParameterDirection.Input
                };
                //CcTitle Parameter
                var paramBuildCcTitle = new SqlParameter
                {
                    ParameterName = "@CcTitle",
                    Value = ccTitle,
                    Direction = ParameterDirection.Input
                };
                //Reuslt Parameter(0:NG; 1:OK)
                var paramBuildResultStatus = new SqlParameter
                {
                    ParameterName = "@ResultStatus",
                    Value = 0,
                    Direction = ParameterDirection.Output
                };

                object[] paramBuildList = new object[8];
                paramBuildList[0] = paramBuildDEAL;
                paramBuildList[1] = paramBuildRegion;
                paramBuildList[2] = paramBuildeid;
                paramBuildList[3] = paramBuildTempleteLanguage;
                paramBuildList[4] = paramBuildPeriodId;
                paramBuildList[5] = paramBuildAlertType;
                paramBuildList[6] = paramBuildCustomerNum;
                paramBuildList[7] = paramBuildToTitle;
                paramBuildList[8] = paramBuildToName;
                paramBuildList[9] = paramBuildCcTitle;
                paramBuildList[10] = paramBuildResultStatus;

                Helper.Log.Info("Start: call spBuildContactorByAlert(procedure):@DEAL" + deal);

                CommonRep.GetDBContext().Database.ExecuteSqlCommand("spBuildContactorByAlert @DEAL,@Region,@Eid,@TempleteLanguage,@PeriodId,@AlertType,@CustomerNum,@ToTitle,@ToName,@CcTitle,@ResultStatus OUTPUT", paramBuildList.ToArray());

                Helper.Log.Info("End: call spBuildContactorByAlert(procedure):@DEAL" + deal + ",RETURN:" + paramBuildResultStatus.Value.ToString());
            }
            catch (Exception exLegal)
            {
                return 0;
            }
            return 1;
        }

        public string getCustomerName(string strCustomerNum, string strSiteUseId)
        {
            return (from c in CommonRep.GetQueryable<Customer>()
                    where c.CustomerNum == strCustomerNum && c.SiteUseId == strSiteUseId
                    select c.CustomerName).FirstOrDefault();
        }

        public IEnumerable<CusExpDateHisDto> getCommDateHistory(string CustomerCode, string SiteUseId)
        {
            var lsExpDateHis = from x in CommonRep.GetDbSet<T_Customer_ExpirationDateHis>()
                .Where(o => o.CustomerNum == CustomerCode && o.SiteUseId == SiteUseId).OrderByDescending(o => o.ChangeDate)
                               select new CusExpDateHisDto
                               {
                                   ID = x.ID,
                                   ChangeDate = x.ChangeDate,
                                   CustomerNum = x.CustomerNum,
                                   SiteUseId = x.SiteUseId,
                                   NewCommentExpirationDate = x.NewCommentExpirationDate,
                                   OldCommentExpirationDate = x.OldCommentExpirationDate,
                                   UserId = x.UserId

                               };

            return lsExpDateHis.AsQueryable<CusExpDateHisDto>(); ;
        }

        public IEnumerable<AgingExpDateHisDto> getAgingDateHistory(int invId)
        {
            var lsExpDateHis = from x in CommonRep.GetDbSet<T_INVOICE_AGING_ExpirationDateHis>()
                .Where(o => o.InvID == invId).OrderBy(o => o.ID)
                               select new AgingExpDateHisDto
                               {
                                   ID = x.ID,
                                   ChangeDate = x.ChangeDate,
                                   InvID = x.InvID,
                                   NewMemoExpirationDate = x.NewMemoExpirationDate,
                                   OldMemoExpirationDate = x.OldMemoExpirationDate,
                                   UserId = x.UserId

                               }; ;

            return lsExpDateHis;
        }
    }
}
