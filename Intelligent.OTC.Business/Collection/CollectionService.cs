using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using Intelligent.OTC.Domain.Repositories;
using Intelligent.OTC.Common.UnitOfWork;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.DomainModel;
using Intelligent.OTC.Domain;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Utils;
using System.Threading;
using System.IO;
using Intelligent.OTC.Business.Interfaces;
using AutoMapper;
using MathNet.Numerics.Statistics;
using System.Data.Entity.SqlServer;
using EntityFramework.BulkInsert.Extensions;
using static Intelligent.OTC.Common.AppConst;
using System.Configuration;
using System.Transactions;
using System.Data.SqlClient;
using Intelligent.OTC.Common.Repository;
using System.Web;
using Newtonsoft.Json;
using System.Data.Entity.Validation;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.Common.Exceptions;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using OfficeOpenXml;

namespace Intelligent.OTC.Business.Collection
{
    public partial class CollectionService : ICollectionService
    {
        public OTCRepository CommonRep { get; set; }
        public ICacheService CacheSvr { get; set; }
        private int? Version { get; set; }
        private T_Deal Deal { get; set; }
        private IQueryable<InvoiceAging> Query { get; set; }
        public SysUser CurrentUser
        {
            get
            {
                return AppContext.Current.User;
            }
        }

        public void ProcessDealCollection(string deal, string legalName = null)
        {
            try
            {
                Deal = CommonRep.GetDbSet<T_Deal>().FirstOrDefault(s => s.Name == deal);
                if (Deal == null) return;
                var legals = CommonRep.GetDbSet<Sites>().Where(s => s.Deal == deal).ToList();
                if (legals == null || legals.Count() == 0) return;
                string version = DateTime.Now.ToString("o").Replace("-", "").Replace(":", "").Replace(".", "").Replace("+", "");
                foreach (var legal in legals)
                {
                    if (!string.IsNullOrWhiteSpace(legalName))
                    {
                        if (legal.LegalEntity != legalName)
                            continue;
                    }
                    T_CustomerAssessment_Log log = new T_CustomerAssessment_Log();
                    log.DealId = Deal.Id;
                    log.LegalEntity = legal.LegalEntity;
                    log.Status = false;
                    log.AssessmentDate = DateTime.Now;
                    if (CurrentUser != null)
                        log.AssessmentUser = CurrentUser.Id;
                    log.Version = version;

                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required))
                    {
                        CommonRep.GetDbSet<T_CustomerAssessment_Log>().Add(log);
                        CommonRep.Commit();
                        Version = log.Id;
                        GetCustomerScoreByAlgorithm(legal.LegalEntity);
                        scope.Complete();
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex, "Job");
                throw new Exception(ex.Message);
            }
        }

        public DashBoardModel GetDashboardReport(string collector, string mail)
        {
            bool alldata = false;
            if (AppContext.Current.User.ActionPermissions.IndexOf("alldataforsupervisor") >= 0)
            {
                alldata = true;
            }
            DashBoardModel model = new DashBoardModel();
            SqlParameter[] para = new SqlParameter[3];
            para[0] = new SqlParameter("@collector", collector);
            para[1] = new SqlParameter("@mail", mail);
            para[2] = new SqlParameter("@alldata", alldata == true ? "1" : "0");
            DataSet ds = CommonRep.GetDBContext().Database.ExecuteDataSet("spDashboardReport", para);
            if (ds != null)
            {
                if (ds.Tables.Count > 0)
                {
                    var row = ds.Tables[0].Rows[0];
                    model.TotalAMT = Convert.ToDecimal(row[0]);
                    model.ConfirmTotal = Convert.ToDecimal(row[1]);
                    model.OverdueTotal = Convert.ToDecimal(row[2]);
                    model.DisputeTotal = Convert.ToDecimal(row[3]);
                    model.NoCollector = Convert.ToInt32(row[4]);
                    model.NoUpload = row[5].ToString();
                }

                model.OverdueReasonStatistics = new List<AMTItem>();
                if (ds.Tables.Count > 1)
                {
                    var rows = ds.Tables[1].Rows;
                    foreach (DataRow row in rows)
                    {
                        AMTItem item = new AMTItem();
                        item.ItemName = row[1].ToString();
                        item.Amt = Convert.ToDecimal(row[0]);
                        model.OverdueReasonStatistics.Add(item);
                    }
                }

                model.OverdueAgingStatistics = new List<AMTItem>();
                if (ds.Tables.Count > 2)
                {
                    var rows = ds.Tables[2].Rows;
                    foreach (DataRow row in rows)
                    {
                        AMTItem item = new AMTItem();
                        item.ItemName = row[1].ToString();
                        item.Amt = Convert.ToDecimal(row[0]);
                        model.OverdueAgingStatistics.Add(item);
                    }
                }
            }
            return model;
        }

        private void GetCustomerScoreByAlgorithm(string legal)
        {
            spAnalysisByLegalEntity(legal);

            var algorithm = 0;
            var duedateBuffer = 0;
            IBaseDataService bdSer = SpringFactory.GetObjectImpl<IBaseDataService>("BaseDataService");
            var config = bdSer.GetSysTypeDetail("033");
            if (config != null)
            {
                if (config.Any(s => s.Seq == 1))
                    algorithm = int.Parse(config.Find(s => s.Seq == 1).DetailValue);
                if (config.Any(s => s.Seq == 2))
                    duedateBuffer = int.Parse(config.Find(s => s.Seq == 2).DetailValue);
            }
            var factorList = CommonRep.GetDbSet<T_AssessmentFactor>().Where(s => s.Algorithm == algorithm).ToList();

            //TenDevided
            foreach (var factor in factorList)
            {
                List<double> valueList = CommonRep.GetDbSet<T_CustomerScore>().Where(s => s.FactorId == factor.Id && s.Version == Version).ToList().Select(s => Convert.ToDouble(s.FactorValue)).ToList();
                if (valueList.Count == 0)
                    return;
                var devidedList = getValueInTenDevided(valueList);
                spAnalysisTenDevided(factor.Id, devidedList);
            }
            spAnalysisSync();
        }

        public string ExportDailyAgingReport(string legalEntity, string custNum, string custName, string SiteUseId)
        {
            string templateName = "DailyAgingReportTemplate";
            string outputPath = "DailyAgingReportPath";
            var tplName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[templateName].ToString());
            var fileName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[outputPath].ToString());
            var pathName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[outputPath].ToString() + "AgingDataExport." + AppContext.Current.User.EID + ".xlsx");

            if (Directory.Exists(fileName) == false)
            {
                Directory.CreateDirectory(fileName);
            }
            var query = GetQueryAging(legalEntity, custNum, custName, SiteUseId);
            if (query.Count() > 50000)
            {
                throw new OTCServiceException("数据量太大(>50000)，请按条件分批导出！");
            }
            WriteDailyAgingDataToExcel(tplName, pathName, query);

            HttpRequest request = HttpContext.Current.Request;
            StringBuilder appUriBuilder = new StringBuilder(request.Url.Scheme);
            appUriBuilder.Append(Uri.SchemeDelimiter);
            appUriBuilder.Append(request.Url.Authority);
            if (String.Compare(request.ApplicationPath, @"/") != 0)
            {
                appUriBuilder.Append(request.ApplicationPath);
            }
            var virPatnName = appUriBuilder.ToString() + ConfigurationManager.AppSettings[outputPath].ToString().Trim('~') + "AgingDataExport." + AppContext.Current.User.EID + ".xlsx";
            return virPatnName;
        }

        public string ExportDailyAgingReportNew(string legalEntity, string custNum, string custName, string SiteUseId)
        {
            string templateName = "~/Template/DailyAgingReportNew.xlsx";
            string outputPath = "DailyAgingReportPath";
            var tplName = HttpContext.Current.Server.MapPath(templateName);
            var fileName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[outputPath].ToString());
            var pathName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[outputPath].ToString() + "AgingDataNewExport." + AppContext.Current.User.EID + ".xlsx");

            if (Directory.Exists(fileName) == false)
            {
                Directory.CreateDirectory(fileName);
            }
            var query = GetQueryAging(legalEntity, custNum, custName, SiteUseId);
            var invoiceQuery = GetQueryInvoiceAging(legalEntity, custNum, custName, SiteUseId);
            if (query.Count() > 50000)
            {
                throw new OTCServiceException("数据量太大(>50000)，请按条件分批导出！");
            }
            WriteDailyAgingDataNewToExcel(tplName, pathName, query, invoiceQuery);

            HttpRequest request = HttpContext.Current.Request;
            StringBuilder appUriBuilder = new StringBuilder(request.Url.Scheme);
            appUriBuilder.Append(Uri.SchemeDelimiter);
            appUriBuilder.Append(request.Url.Authority);
            if (String.Compare(request.ApplicationPath, @"/") != 0)
            {
                appUriBuilder.Append(request.ApplicationPath);
            }
            var virPatnName = appUriBuilder.ToString() + ConfigurationManager.AppSettings[outputPath].ToString().Trim('~') + "AgingDataNewExport." + AppContext.Current.User.EID + ".xlsx";
            return virPatnName;
        }

        public string ExportAgingReportNew(string region, string legalentity, string custName, string siteUseId, string invoicecode, string status, string docType, string poNum, string soNum, string creditTerm, string invoiceMemo, string eb, string invoiceDateFrom, string invoiceDateTo, string DuedateFrom, string DuedateTo) {

            string templateName = "NewAgingReportTemplate";
            string outputPath = "NewAgingReportPath";
            var tplName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[templateName].ToString());
            var fileName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[outputPath].ToString());
            var pathName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[outputPath].ToString() + "AR Aging " + DateTime.Now.ToString("yyyyMMdd") + "-" + AppContext.Current.User.EID + ".xlsx");

            if (Directory.Exists(fileName) == false)
            {
                Directory.CreateDirectory(fileName);
            }
            File.Copy(tplName, pathName, true);

            AgingReportDtoPage detail = new AgingReportDtoPage();
            detail = QueryAgingReport(region, legalentity, custName, siteUseId, invoicecode, status, docType, poNum, soNum, creditTerm, invoiceMemo, eb, invoiceDateFrom, invoiceDateTo, DuedateFrom, DuedateTo,1,9999999);
            AgingReportDtoPage summary = new AgingReportDtoPage();
            summary = QueryAgingSummaryReport(region, legalentity, custName, siteUseId, invoicecode, status, docType, poNum, soNum, creditTerm, invoiceMemo, eb, invoiceDateFrom, invoiceDateTo, DuedateFrom, DuedateTo, 1, 9999999);
            
            ExcelPackage package = new ExcelPackage(new FileInfo(pathName));
            ExcelWorksheet worksheet = package.Workbook.Worksheets[2];
            int startRow = 3;
            foreach (var lst in summary.summary)
            {
                worksheet.Cells[startRow, 1].Value = lst.Ebname;
                worksheet.Cells[startRow, 2].Value = lst.AccntNumber;
                worksheet.Cells[startRow, 3].Value = lst.SiteUseId;
                worksheet.Cells[startRow, 4].Value = lst.CustomerName;
                worksheet.Cells[startRow, 5].Value = lst.PaymentTermDesc;
                worksheet.Cells[startRow, 6].Value = lst.OverCreditLmt;
                worksheet.Cells[startRow, 7].Value = lst.FuncCurrCode;
                worksheet.Cells[startRow, 8].Value = lst.Fsr;
                worksheet.Cells[startRow, 9].Value = lst.AmtRemaining01To15;
                worksheet.Cells[startRow, 10].Value = lst.AmtRemaining16To30;
                worksheet.Cells[startRow, 11].Value = lst.AmtRemaining31To45;
                worksheet.Cells[startRow, 12].Value = lst.AmtRemaining46To60;
                worksheet.Cells[startRow, 13].Value = lst.AmtRemaining61To90;
                worksheet.Cells[startRow, 14].Value = lst.AmtRemaining91To120;
                worksheet.Cells[startRow, 15].Value = lst.AmtRemaining121To180;
                worksheet.Cells[startRow, 16].Value = lst.AmtRemaining181To270;
                worksheet.Cells[startRow, 17].Value = lst.AmtRemaining271To360;
                worksheet.Cells[startRow, 18].Value = lst.AmtRemaining360Plus;
                worksheet.Cells[startRow, 19].Value = lst.AmtRemainingTotalFutureDue;
                startRow++;
            }

            ExcelWorksheet worksheetdetail = package.Workbook.Worksheets[1];
            int startRowDetail = 2;
            foreach (var lst in detail.detail)
            {
                worksheetdetail.Cells[startRowDetail, 1].Value = lst.Ebname;
                worksheetdetail.Cells[startRowDetail, 2].Value = lst.Customertype;
                worksheetdetail.Cells[startRowDetail, 3].Value = lst.AccntNumber;
                worksheetdetail.Cells[startRowDetail, 4].Value = lst.SiteUseId;
                worksheetdetail.Cells[startRowDetail, 5].Value = lst.CustomerName;
                worksheetdetail.Cells[startRowDetail, 6].Value = lst.SellingLocationCode;
                worksheetdetail.Cells[startRowDetail, 7].Value = lst.Class;
                worksheetdetail.Cells[startRowDetail, 8].Value = lst.TrxNum;
                worksheetdetail.Cells[startRowDetail, 9].Value = lst.TrxDate;
                worksheetdetail.Cells[startRowDetail, 10].Value = lst.DueDate;
                worksheetdetail.Cells[startRowDetail, 11].Value = lst.DueDays;
                worksheetdetail.Cells[startRowDetail, 12].Value = lst.AmtRemaining;
                worksheetdetail.Cells[startRowDetail, 13].Value = lst.AmountWoVat;
                worksheetdetail.Cells[startRowDetail, 14].Value = lst.PaymentTermName;
                worksheetdetail.Cells[startRowDetail, 15].Value = lst.OverCreditLmt;
                worksheetdetail.Cells[startRowDetail, 16].Value = lst.OverCreditLmtAcct;
                worksheetdetail.Cells[startRowDetail, 17].Value = lst.FuncCurrCode;
                worksheetdetail.Cells[startRowDetail, 18].Value = lst.InvCurrCode;
                worksheetdetail.Cells[startRowDetail, 19].Value = lst.SalesName;
                worksheetdetail.Cells[startRowDetail, 20].Value = lst.AgingBucket;
                worksheetdetail.Cells[startRowDetail, 21].Value = lst.PaymentTermDesc;
                worksheetdetail.Cells[startRowDetail, 22].Value = lst.SellingLocationCode2;
                worksheetdetail.Cells[startRowDetail, 23].Value = lst.Isr;
                worksheetdetail.Cells[startRowDetail, 24].Value = lst.Fsr;
                worksheetdetail.Cells[startRowDetail, 25].Value = lst.OrgId;
                worksheetdetail.Cells[startRowDetail, 26].Value = lst.Cmpinv;
                worksheetdetail.Cells[startRowDetail, 27].Value = lst.SalesOrder;
                worksheetdetail.Cells[startRowDetail, 28].Value = lst.Cpo;
                worksheetdetail.Cells[startRowDetail, 29].Value = lst.FsrNameHist;
                worksheetdetail.Cells[startRowDetail, 30].Value = lst.IsrNameHist;
                worksheetdetail.Cells[startRowDetail, 31].Value = lst.Eb;
                worksheetdetail.Cells[startRowDetail, 32].Value = lst.AmtRemainingTran;
                startRowDetail++;
            }
           
            package.Save();
            package.Dispose();

            HttpRequest request = HttpContext.Current.Request;
            StringBuilder appUriBuilder = new StringBuilder(request.Url.Scheme);
            appUriBuilder.Append(Uri.SchemeDelimiter);
            appUriBuilder.Append(request.Url.Authority);
            if (String.Compare(request.ApplicationPath, @"/") != 0)
            {
                appUriBuilder.Append(request.ApplicationPath);
            }
            var virPatnName = appUriBuilder.ToString() + ConfigurationManager.AppSettings[outputPath].ToString().Trim('~') + "AR Aging " + DateTime.Now.ToString("yyyyMMdd") + "-" + AppContext.Current.User.EID + ".xlsx";
            return virPatnName;
        }

        public string ExportAgingReport(string filter)
        {
            string templateName = "AgingReportTemplate";
            string outputPath = "AgingReportPath";
            var tplName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[templateName].ToString());
            var fileName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[outputPath].ToString());
            var pathName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[outputPath].ToString() + "AgingDataExport." + AppContext.Current.User.EID + ".xlsx");

            if (Directory.Exists(fileName) == false)
            {
                Directory.CreateDirectory(fileName);
            }
            IQueryable<CollectingReportDto> query = GetQueryAging(filter);
            if (query.Count() > 50000) {
                throw new OTCServiceException("数据量太大(>50000)，请按条件分批导出！");
            }
            WriteAgingDataToExcel(tplName, pathName, query);

            HttpRequest request = HttpContext.Current.Request;
            StringBuilder appUriBuilder = new StringBuilder(request.Url.Scheme);
            appUriBuilder.Append(Uri.SchemeDelimiter);
            appUriBuilder.Append(request.Url.Authority);
            if (String.Compare(request.ApplicationPath, @"/") != 0)
            {
                appUriBuilder.Append(request.ApplicationPath);
            }
            var virPatnName = appUriBuilder.ToString() + ConfigurationManager.AppSettings[outputPath].ToString().Trim('~') + "AgingDataExport." + AppContext.Current.User.EID + ".xlsx";
            return virPatnName;
        }

        public AgingReportDtoPage QueryAgingReport(string region, string legalentity, string custName, string siteUseId, string invoicecode, string status, string docType, string poNum, string soNum, string creditTerm, string invoiceMemo, string eb, string invoiceDateFrom, string invoiceDateTo, string DuedateFrom, string DuedateTo, int page, int pagesize)
        {
            AgingReportDtoPage model = new AgingReportDtoPage();
            List<AgingReportDto> list = new List<AgingReportDto>();
            string strSQLWhere = "";
            string strCountSQLWhere = "";
            agingReportWhereSQL(region, legalentity, custName, siteUseId, invoicecode, status, docType, poNum, soNum, creditTerm, invoiceMemo, eb, invoiceDateFrom, invoiceDateTo, DuedateFrom, DuedateTo, ref strSQLWhere, ref strCountSQLWhere);
            string strCountSQL = string.Format(@"SELECT count(*)  
                                     FROM T_INVOICE_AGING AS aging with(nolock)         
                                     LEFT JOIN T_CUSTOMER AS c with(nolock) ON aging.SiteUseId = c.SiteUseId 
                                    WHERE 1 = 1 {0}", strSQLWhere);
            string strSQL = string.Format(@"SELECT
                                    *
                                FROM
                                    (SELECT 
                                     ROW_NUMBER () OVER (ORDER BY aging.Ebname, aging.Customertype, c.CUSTOMER_NUM, c.SiteUseId, aging.INVOICE_NUM) AS RowNumber,
                                     aging.Ebname                   AS Ebname, 
                                     aging.Customertype             AS Customertype,
                                     aging.CUSTOMER_NUM             AS AccntNumber,
                                     aging.SiteUseId                AS SiteUseId,
                                     c.CUSTOMER_NAME                AS CustomerName,
                                     aging.SellingLocationCode      AS SellingLocationCode,
                                     aging.CLASS                    AS Class,
                                     aging.INVOICE_NUM              AS TrxNum,
                                     aging.INVOICE_DATE             AS TrxDate,
                                     aging.DUE_DATE                 AS DueDate,
                                     aging.DAYS_LATE_SYS            AS DueDays,
                                     aging.BALANCE_AMT              AS AmtRemaining,
                                     aging.WoVat_AMT                AS AmountWoVat,
                                     aging.CREDIT_TREM              AS PaymentTermName,
                                     aging.CreditLmt                AS OverCreditLmt,
                                     aging.CreditLmtAcct            AS OverCreditLmtAcct,
                                     aging.FuncCurrCode             AS FuncCurrCode,
                                     aging.CURRENCY                 AS InvCurrCode,
                                     aging.Sales                    AS SalesName,
                                     aging.AgingBucket              AS AgingBucket,
                                     aging.CreditTremDescription    AS PaymentTermDesc,
                                     aging.SellingLocationCode2     AS SellingLocationCode2,
                                     aging.LsrNameHist              AS Isr,
                                     aging.Fsr                      AS Fsr,
                                     aging.LEGAL_ENTITY             AS OrgId,
                                     aging.Cmpinv                   AS Cmpinv,
                                     aging.SO_NUM                   AS SalesOrder,
                                     aging.PO_MUM                   AS Cpo,
                                     aging.FsrNameHist              AS FsrNameHist,
                                     aging.LsrNameHist              AS IsrNameHist,
                                     aging.Eb                       AS Eb,
                                     aging.RemainingAmtTran         AS AmtRemainingTran
                                     FROM T_INVOICE_AGING           AS aging with(nolock) 
                                     LEFT JOIN T_CUSTOMER AS c with(nolock) ON aging.SiteUseId = c.SiteUseId
                                     WHERE 1 = 1 {0}", strCountSQLWhere);
            strSQL += string.Format(@" ) AS t WHERE RowNumber BETWEEN {0} AND {1}", page == 1 ? 0 : pagesize * (page - 1) + 1, pagesize * page);
            list = CommonRep.ExecuteSqlQuery<AgingReportDto>(strSQL).ToList();
            model.detail = list;
            model.detailcount = SqlHelper.ExcuteScalar<int>(strCountSQL);
            return model;
        }

        public AgingReportDtoPage QueryAgingSummaryReport(string region, string legalentity, string custName, string siteUseId, string invoicecode, string status, string docType, string poNum, string soNum, string creditTerm, string invoiceMemo, string eb, string invoiceDateFrom, string invoiceDateTo, string DuedateFrom, string DuedateTo, int page, int pagesize)
        {
            AgingReportDtoPage model = new AgingReportDtoPage();
            List<AgingReportSumDto> list = new List<AgingReportSumDto>();
            string strSQLWhere = "";
            string strCountSQLWhere = "";
            agingReportWhereSQL(region, legalentity, custName, siteUseId, invoicecode, status, docType, poNum, soNum, creditTerm, invoiceMemo, eb, invoiceDateFrom, invoiceDateTo, DuedateFrom, DuedateTo, ref strSQLWhere, ref strCountSQLWhere);
            string strCountSQL = string.Format(@"SELECT count(*)  
                                     FROM T_CUSTOMER_AGING AS AGINGSUMMARY
                                    LEFT JOIN T_CUSTOMER AS ca ON AGINGSUMMARY.SiteUseId = ca.SiteUseId
                                    where AGINGSUMMARY.siteuseid in 
                                    (select distinct aging.siteuseid from T_INVOICE_AGING as aging  with(nolock)
                                    LEFT JOIN T_CUSTOMER AS c with(nolock) ON aging.SiteUseId = c.SiteUseId
                                     WHERE 1 = 1 {0})", strCountSQLWhere); 
            string strSQL = string.Format(@"SELECT
                                    *
                                FROM
                                    (SELECT 
                                     ROW_NUMBER () OVER (ORDER BY AGINGSUMMARY.Ebname, ca.CUSTOMER_NUM, ca.SiteUseId) AS RowNumber,
                                     AGINGSUMMARY.Ebname AS Ebname,
                                     AGINGSUMMARY.CUSTOMER_NUM AS AccntNumber,
                                     AGINGSUMMARY.SiteUseId  AS SiteUseId,
                                     AGINGSUMMARY.CUSTOMER_NAME AS CustomerName,
                                     AGINGSUMMARY.CREDIT_TREM  AS PaymentTermDesc,
                                     AGINGSUMMARY.CREDIT_LIMIT AS OverCreditLmt,
                                     AGINGSUMMARY.CURRENCY AS FuncCurrCode,
                                     AGINGSUMMARY.SALES AS Fsr,
                                     isNull(AGINGSUMMARY.DUE15_AMT,0) AS AmtRemaining01To15,
                                     isNull(AGINGSUMMARY.DUE30_AMT,0) AS AmtRemaining16To30,
                                     isNull(AGINGSUMMARY.DUE45_AMT,0) AS AmtRemaining31To45,
                                     isNull(AGINGSUMMARY.DUE60_AMT,0) AS AmtRemaining46To60,
                                     isNull(AGINGSUMMARY.DUE90_AMT,0) AS AmtRemaining61To90,
                                     isNull(AGINGSUMMARY.DUE120_AMT,0) AS AmtRemaining91To120,
                                     isNull(AGINGSUMMARY.DUE150_AMT,0) + isNull(AGINGSUMMARY.DUE180_AMT,0) AS AmtRemaining121To180,
                                     isNull(AGINGSUMMARY.DUE210_AMT,0) + isNull(AGINGSUMMARY.DUE240_AMT,0) + isNull(AGINGSUMMARY.DUE270_AMT,0) AS AmtRemaining181To270,
                                     isNull(AGINGSUMMARY.DUE300_AMT,0) + isNull(AGINGSUMMARY.DUE330_AMT,0) + isNull(AGINGSUMMARY.DUE360_AMT,0) AS AmtRemaining271To360,
                                     isNull(AGINGSUMMARY.DUEOVER360_AMT,0) AS AmtRemaining360Plus,
                                     isNull(AGINGSUMMARY.TotalFutureDue,0) AS AmtRemainingTotalFutureDue 
                                     FROM T_CUSTOMER_AGING AS AGINGSUMMARY with(nolock) 
                                    LEFT JOIN T_CUSTOMER AS ca with(nolock) ON AGINGSUMMARY.SiteUseId = ca.SiteUseId
                                    where CA.SiteUseId in 
                                    (select distinct aging.SiteUseId from T_INVOICE_AGING as aging with(nolock) 
                                    LEFT JOIN T_CUSTOMER AS c with(nolock) ON aging.SiteUseId = c.SiteUseId
                                     WHERE 1 = 1 {0})", strSQLWhere);
            strSQL += string.Format(@" ) AS t WHERE RowNumber BETWEEN {0} AND {1}", page == 1 ? 0 : pagesize * (page - 1) + 1, pagesize * page);
            list = CommonRep.ExecuteSqlQuery<AgingReportSumDto>(strSQL).ToList();
            model.summary = list;
            model.summarycount = SqlHelper.ExcuteScalar<int>(strCountSQL);
            return model;
        }

        public void agingReportWhereSQL(string region, string legalentity, string custName, string siteUseId, string invoicecode, string status, string docType, string poNum, string soNum, string creditTerm, string invoiceMemo, string eb, string invoiceDateFrom, string invoiceDateTo, string DuedateFrom, string DuedateTo, ref string strSQL, ref string strCountSQL) {
            if (!string.IsNullOrEmpty(region) && region != "null")
            {
                region = region.ToUpper();
                string[] strRegionGroup = region.Split(';');
                strSQL = " AND (";
                strCountSQL = " AND (";
                string strRegionWhere = "";
                if (strRegionGroup.Contains("CN-CCNC")) {
                    if (!string.IsNullOrEmpty(strRegionWhere)) {
                        strRegionWhere += " OR ";
                    }
                    strRegionWhere += "(c.region in ('CC','NC') AND C.Ebname NOT LIKE '%Waching%'  AND C.Ebname NOT LIKE '%SEED%')";
                }
                if (strRegionGroup.Contains("CN-WACHING"))
                {
                    if (!string.IsNullOrEmpty(strRegionWhere))
                    {
                        strRegionWhere += " OR ";
                    }
                    strRegionWhere += "(c.region in ('CC','NC','SZ') AND C.Ebname LIKE '%Waching%')";
                }
                if (strRegionGroup.Contains("CN-SC"))
                {
                    if (!string.IsNullOrEmpty(strRegionWhere))
                    {
                        strRegionWhere += " OR ";
                    }
                    strRegionWhere += "(c.region in ('SZ') AND C.Ebname NOT LIKE '%Waching%'  AND C.Ebname NOT LIKE '%SEED%')";
                }
                if (strRegionGroup.Contains("CN-SEED"))
                {
                    if (!string.IsNullOrEmpty(strRegionWhere))
                    {
                        strRegionWhere += " OR ";
                    }
                    strRegionWhere += "(c.region in ('CC','NC','SZ') AND C.Ebname LIKE '%SEED%')";
                }
                if (strRegionGroup.Contains("TW"))
                {
                    if (!string.IsNullOrEmpty(strRegionWhere))
                    {
                        strRegionWhere += " OR ";
                    }
                    strRegionWhere += "(c.region in ('TW'))";
                }
                if (strRegionGroup.Contains("KR"))
                {
                    if (!string.IsNullOrEmpty(strRegionWhere))
                    {
                        strRegionWhere += " OR ";
                    }
                    strRegionWhere += "(c.region in ('KR'))";
                }
                if (strRegionGroup.Contains("HK"))
                {
                    if (!string.IsNullOrEmpty(strRegionWhere))
                    {
                        strRegionWhere += " OR ";
                    }
                    strRegionWhere += "(c.region in ('HK'))";
                }
                if (strRegionGroup.Contains("INDIA"))
                {
                    if (!string.IsNullOrEmpty(strRegionWhere))
                    {
                        strRegionWhere += " OR ";
                    }
                    strRegionWhere += "(c.region in ('INDIA'))";
                }
                if (strRegionGroup.Contains("ASEAN"))
                {
                    if (!string.IsNullOrEmpty(strRegionWhere))
                    {
                        strRegionWhere += " OR ";
                    }
                    strRegionWhere += "(c.region like '%ASEAN%')";
                }
                if (strRegionGroup.Contains("ATM-TW"))
                {
                    if (!string.IsNullOrEmpty(strRegionWhere))
                    {
                        strRegionWhere += " OR ";
                    }
                    strRegionWhere += "(c.region = 'ATM-TW')";
                }
                if (strRegionGroup.Contains("ATM-SZ"))
                {
                    if (!string.IsNullOrEmpty(strRegionWhere))
                    {
                        strRegionWhere += " OR ";
                    }
                    strRegionWhere += "(c.region = 'ATM-SZ')";
                }
                if (strRegionGroup.Contains("UA"))
                {
                    if (!string.IsNullOrEmpty(strRegionWhere))
                    {
                        strRegionWhere += " OR ";
                    }
                    strRegionWhere += "(c.region = 'UA')";
                }
                if (strRegionGroup.Contains("UA-TW"))
                {
                    if (!string.IsNullOrEmpty(strRegionWhere))
                    {
                        strRegionWhere += " OR ";
                    }
                    strRegionWhere += "(c.region = 'UA-TW')";
                }

                strSQL = strSQL + strRegionWhere + " ) ";
                strCountSQL = strCountSQL + strRegionWhere + " ) ";
            }

            if (!string.IsNullOrEmpty(legalentity) && legalentity != "null")
            {
                strSQL += " AND c.Organization = '" + legalentity + "'";
                strCountSQL += " AND c.Organization = '" + legalentity + "'";
            }
            if (!string.IsNullOrEmpty(custName) && custName != "null")
            {
                strSQL += " AND (c.CUSTOMER_NUM like '%" + custName + "%' or c.CUSTOMER_NAME like '%" + custName + "%' )";
                strCountSQL += " AND (c.CUSTOMER_NUM like '%" + custName + "%' or c.CUSTOMER_NAME like '%" + custName + "%' )";
            }
            if (!string.IsNullOrEmpty(siteUseId) && siteUseId != "null")
            {
                strSQL += " AND (c.SiteUseId like '%" + siteUseId + "%' )";
                strCountSQL += " AND (c.SiteUseId like '%" + siteUseId + "%' )";
            }
            if (!string.IsNullOrEmpty(invoicecode) && invoicecode != "null")
            {
                strSQL += " AND (aging.INVOICE_NUM like '%" + invoicecode + "%' )";
                strCountSQL += " AND (aging.INVOICE_NUM like '%" + invoicecode + "%' )";
            }
            if (!string.IsNullOrEmpty(status) && status != "null")
            {
                if (status == "000")
                {
                    strSQL += " AND (aging.TRACK_STATES <> '014' )";
                    strCountSQL += " AND (aging.TRACK_STATES <> '014' )";
                } else { 
                    strSQL += " AND (aging.TRACK_STATES = '" + status + "' )";
                    strCountSQL += " AND (aging.TRACK_STATES = '" + status + "' )";
                }
            }
            if (!string.IsNullOrEmpty(docType) && docType != "null")
            {
                strSQL += " AND (aging.Class = '" + docType + "' )";
                strCountSQL += " AND (aging.Class = '" + docType + "' )";
            }
            if (!string.IsNullOrEmpty(poNum) && poNum != "null")
            {
                strSQL += " AND (aging.PO_MUM like '%" + poNum + "%' )";
                strCountSQL += " AND (aging.PO_MUM like '%" + poNum + "%' )";
            }
            if (!string.IsNullOrEmpty(soNum) && soNum != "null")
            {
                strSQL += " AND (aging.SO_NUM like '%" + soNum + "%' )";
                strCountSQL += " AND (aging.SO_NUM like '%" + soNum + "%' )";
            }
            if (!string.IsNullOrEmpty(creditTerm) && creditTerm != "null")
            {
                strSQL += " AND (aging.CreditTremDescription like '%" + creditTerm + "%' )";
                strCountSQL += " AND (aging.CreditTremDescription like '%" + creditTerm + "%' )";
            }
            if (!string.IsNullOrEmpty(invoiceMemo) && invoiceMemo != "null")
            {
                strSQL += " AND (aging.BalanceMemo like '%" + invoiceMemo + "%' )";
                strCountSQL += " AND (aging.BalanceMemo like '%" + invoiceMemo + "%' )";
            }
            if (!string.IsNullOrEmpty(eb) && eb != "null")
            {
                strSQL += " AND (c.Ebname like '%" + eb + "%' )";
                strCountSQL += " AND (c.Ebname like '%" + eb + "%' )";
            }
            if (!string.IsNullOrEmpty(invoiceDateFrom) && invoiceDateFrom != "null")
            {
                strSQL += " AND (aging.INVOICE_DATE >= '" + invoiceDateFrom + " 00:00:00' )";
                strCountSQL += " AND (aging.INVOICE_DATE >= '" + invoiceDateFrom + " 00:00:00' )";
            }
            if (!string.IsNullOrEmpty(invoiceDateTo) && invoiceDateTo != "null")
            {
                strSQL += " AND (aging.INVOICE_DATE <= '" + invoiceDateTo + " 23:59:59' )";
                strCountSQL += " AND (aging.INVOICE_DATE <= '" + invoiceDateTo + " 23:59:59' )";
            }
            if (!string.IsNullOrEmpty(DuedateFrom) && DuedateFrom != "null")
            {
                strSQL += " AND (aging.DUE_DATE >= '" + DuedateFrom + " 00:00:00' )";
                strCountSQL += " AND (aging.DUE_DATE >= '" + DuedateFrom + " 00:00:00' )";
            }
            if (!string.IsNullOrEmpty(DuedateTo) && DuedateTo != "null")
            {
                strSQL += " AND (aging.DUE_DATE <= '" + DuedateTo + " 23:59:59' )";
                strCountSQL += " AND (aging.DUE_DATE <= '" + DuedateTo + " 23:59:59' )";
            }
        }

        public string ExportDisputeReport(string filter)
        {
            string templateName = "DisputeReportTemplate";
            string outputPath = "DisputeReportPath";
            var tplName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[templateName].ToString());
            var fileName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[outputPath].ToString());
            var pathName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[outputPath].ToString() + "DisputeDataExport." + AppContext.Current.User.EID + ".xlsx");

            if (Directory.Exists(fileName) == false)
            {
                Directory.CreateDirectory(fileName);
            }
            IQueryable<CollectingReportDto> query = GetQueryDispute(filter);
            if (query.Count() > 50000)
            {
                Exception ex = new OTCServiceException("数据量太大(>50000)，请按条件分批导出！");
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
            WriteDisputeDataToExcel(tplName, pathName, query);

            HttpRequest request = HttpContext.Current.Request;
            StringBuilder appUriBuilder = new StringBuilder(request.Url.Scheme);
            appUriBuilder.Append(Uri.SchemeDelimiter);
            appUriBuilder.Append(request.Url.Authority);
            if (String.Compare(request.ApplicationPath, @"/") != 0)
            {
                appUriBuilder.Append(request.ApplicationPath);
            }
            var virPatnName = appUriBuilder.ToString() + ConfigurationManager.AppSettings[outputPath].ToString().Trim('~') + "DisputeDataExport." + AppContext.Current.User.EID + ".xlsx";
            return virPatnName;
        }

        public ReportModel QueryDisputeReport(int pageindex, int pagesize, string filter)
        {
            ReportModel model = new ReportModel();
            using (var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions()
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
            }))
            {
                IQueryable<CollectingReportDto> query = GetQueryDispute(filter);
                model.TotalItems = query.Count();
                var list = query.Skip((pageindex - 1) * pagesize).Take(pagesize).ToList();
                model.List = Mapper.Map<List<ReportItem>>(list);
                scope.Complete();
            }
            return model;
        }

        public string ExportOverdueReport(string filter)
        {
            string templateName = "OverdueportTemplate";
            string outputPath = "OverdueportPath";
            var tplName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[templateName].ToString());
            var fileName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[outputPath].ToString());
            var pathName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[outputPath].ToString() + "OverdueDataExport." + AppContext.Current.User.EID + ".xlsx");

            if (Directory.Exists(fileName) == false)
            {
                Directory.CreateDirectory(fileName);
            }
            IQueryable<CollectingReportDto> query = GetQueryOverdue(filter);

            WriteOverdueDataToExcel(tplName, pathName, query);

            HttpRequest request = HttpContext.Current.Request;
            StringBuilder appUriBuilder = new StringBuilder(request.Url.Scheme);
            appUriBuilder.Append(Uri.SchemeDelimiter);
            appUriBuilder.Append(request.Url.Authority);
            if (String.Compare(request.ApplicationPath, @"/") != 0)
            {
                appUriBuilder.Append(request.ApplicationPath);
            }
            var virPatnName = appUriBuilder.ToString() + ConfigurationManager.AppSettings[outputPath].ToString().Trim('~') + "OverdueDataExport." + AppContext.Current.User.EID + ".xlsx";
            return virPatnName;
        }

        public ReportModel QueryOverdueReport(int pageindex, int pagesize, string filter)
        {
            ReportModel model = new ReportModel();
            using (var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions()
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
            }))
            {
                IQueryable<CollectingReportDto> query = GetQueryOverdue(filter);
                model.TotalItems = query.Count();
                var list = query.ToList().Skip((pageindex - 1) * pagesize).Take(pagesize);
                model.List = Mapper.Map<List<ReportItem>>(list);
                scope.Complete();
            }
            return model;
        }

        /// <summary>
        /// ten devided method
        /// </summary>
        /// <param name="indexValue"></param>
        /// <returns></returns>
        private List<double> getValueInTenDevided(List<double> indexValue)
        {
            double q1 = 0;
            double q2 = 0;
            double q3 = 0;
            double q4 = 0;
            double q5 = 0;
            double q6 = 0;
            double q7 = 0;
            double q8 = 0;
            double q9 = 0;

            List<double> result = new List<double>();
            List<double> constList = ConstTenDevidedRate();

            q1 = Statistics.QuantileCustom(indexValue, constList[0], QuantileDefinition.Excel);
            q2 = Statistics.QuantileCustom(indexValue, constList[1], QuantileDefinition.Excel);
            q3 = Statistics.QuantileCustom(indexValue, constList[2], QuantileDefinition.Excel);
            q4 = Statistics.QuantileCustom(indexValue, constList[3], QuantileDefinition.Excel);
            q5 = Statistics.QuantileCustom(indexValue, constList[4], QuantileDefinition.Excel);
            q6 = Statistics.QuantileCustom(indexValue, constList[5], QuantileDefinition.Excel);
            q7 = Statistics.QuantileCustom(indexValue, constList[6], QuantileDefinition.Excel);
            q8 = Statistics.QuantileCustom(indexValue, constList[7], QuantileDefinition.Excel);
            q9 = Statistics.QuantileCustom(indexValue, constList[8], QuantileDefinition.Excel);
            result.Add(indexValue.Min());
            result.Add(q1);
            result.Add(q2);
            result.Add(q3);
            result.Add(q4);
            result.Add(q5);
            result.Add(q6);
            result.Add(q7);
            result.Add(q8);
            result.Add(q9);
            result.Add(indexValue.Max());
            return result;
        }

        private List<double> ConstTenDevidedRate()
        {
            List<double> result = new List<double>();
            double[] array = new double[] { 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9 };
            result.AddRange(array);
            return result;
        }

        private void spAnalysisByLegalEntity(string legal)
        {
            var user = CurrentUser == null ? 0 : CurrentUser.Id;
            var sql = "spAnalysisByLegalEntity";
            SqlParameter[] para = new SqlParameter[3];
            para[0] = new SqlParameter("@legal", legal);
            para[1] = new SqlParameter("@user", user);
            para[2] = new SqlParameter("@version", Version);
            CommonRep.GetDBContext().Database.ExecuteSP(sql, para);
        }

        private void spAnalysisTenDevided(int facterid, List<double> devidedList)
        {
            var sql = "spAnalysisTenDevided";
            SqlParameter[] para = new SqlParameter[13];
            para[0] = new SqlParameter("@factor", facterid);
            para[1] = new SqlParameter("@version", Version);
            para[2] = new SqlParameter("@v1", devidedList[0]);
            para[3] = new SqlParameter("@v2", devidedList[1]);
            para[4] = new SqlParameter("@v3", devidedList[2]);
            para[5] = new SqlParameter("@v4", devidedList[3]);
            para[6] = new SqlParameter("@v5", devidedList[4]);
            para[7] = new SqlParameter("@v6", devidedList[5]);
            para[8] = new SqlParameter("@v7", devidedList[6]);
            para[9] = new SqlParameter("@v8", devidedList[7]);
            para[10] = new SqlParameter("@v9", devidedList[8]);
            para[11] = new SqlParameter("@v10", devidedList[9]);
            para[12] = new SqlParameter("@v11", devidedList[10]);
            CommonRep.GetDBContext().Database.ExecuteSP(sql, para);
        }

        private void spAnalysisSync()
        {
            var sql = "spAnalysisSync";
            SqlParameter[] para = new SqlParameter[1];
            para[0] = new SqlParameter("@version", Version);
            CommonRep.GetDBContext().Database.ExecuteSP(sql, para);
        }


        private void WriteDailyAgingDataToExcel(string temp, string path, IQueryable<DailyAgingDto> list)
        {
            try
            {
                ExportService export = new ExportService(temp);
                export.Save(path, true);
                export = new ExportService(path);
                var sheetName = export.Sheets[0];
                export.ActiveSheetName = sheetName;
                using (var scope = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions()
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                }))
                    {
                    export.ExportDailyAgingDataList(list.ToList());
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        private void WriteDailyAgingDataNewToExcel(string temp, string path, IQueryable<DailyAgingDto> dailyAgings, IList<InvoiceAgingDto> invoiceAgings)
        {
            try
            {
                NpoiHelper helper = new NpoiHelper(temp);
                helper.Save(path, true);
                helper = new NpoiHelper(path);

                ICellStyle cellStyle = helper.Book.CreateCellStyle();
                cellStyle.Alignment = HorizontalAlignment.Center;

                ICellStyle cellStyleMoney = helper.Book.CreateCellStyle();
                cellStyleMoney.Alignment = HorizontalAlignment.Center;
                cellStyleMoney.DataFormat= HSSFDataFormat.GetBuiltinFormat("#,##0");

                int rowNo = 1;
                ISheet sheet = helper.Book.GetSheetAt(0);
                foreach (var item in dailyAgings)
                {
                    IRow row = sheet.CreateRow(rowNo);
                    ICell cell0 = row.CreateCell(0);
                    cell0.SetCellValue(item.Collector);
                    cell0.CellStyle = cellStyle;
                    ICell cell1 = row.CreateCell(1);
                    cell1.SetCellValue(item.legalEntity);
                    cell1.CellStyle = cellStyle;
                    ICell cell2 = row.CreateCell(2);
                    cell2.SetCellValue(item.CustomerName);
                    cell2.CellStyle = cellStyle;
                    ICell cell3 = row.CreateCell(3);
                    cell3.SetCellValue(item.AccntNumber);
                    cell3.CellStyle = cellStyle;
                    ICell cell4 = row.CreateCell(4);
                    cell4.SetCellValue(item.SiteUseId);
                    cell4.CellStyle = cellStyle;
                    ICell cell5 = row.CreateCell(5);
                    cell5.SetCellValue(item.PaymentTermDesc);
                    cell5.CellStyle = cellStyle;
                    ICell cell6 = row.CreateCell(6);
                    cell6.SetCellValue(item.Ebname);
                    cell6.CellStyle = cellStyle;
                    if (item.OverCreditLmt.HasValue)
                    {
                        ICell cell7 = row.CreateCell(7);
                        cell7.SetCellValue((double)item.OverCreditLmt);
                        cell7.CellStyle = cellStyleMoney;
                    }
                    ICell cell8 = row.CreateCell(8);
                    cell8.SetCellValue(item.FuncCurrCode);
                    cell8.CellStyle = cellStyle;
                    if (item.TotalFutureDue.HasValue)
                    {
                        ICell cell9 = row.CreateCell(9);
                        cell9.SetCellValue((double)item.TotalFutureDue);
                        cell9.CellStyle = cellStyleMoney;
                    }
                    if (item.Due15Amt.HasValue)
                    {
                        ICell cell10 = row.CreateCell(10);
                        cell10.SetCellValue((double)item.Due15Amt);
                        cell10.CellStyle = cellStyleMoney;
                    }
                    if (item.Due30Amt.HasValue)
                    {
                        ICell cell11 = row.CreateCell(11);
                        cell11.SetCellValue((double)item.Due30Amt);
                        cell11.CellStyle = cellStyleMoney;
                    }
                    if (item.Due45Amt.HasValue)
                    {
                        ICell cell12 = row.CreateCell(12);
                        cell12.SetCellValue((double)item.Due45Amt);
                        cell12.CellStyle = cellStyleMoney;
                    }
                    if (item.Due60Amt.HasValue)
                    {
                        ICell cell13 = row.CreateCell(13);
                        cell13.SetCellValue((double)item.Due60Amt);
                        cell13.CellStyle = cellStyleMoney;
                    }
                    if (item.Due90Amt.HasValue)
                    {
                        ICell cell14 = row.CreateCell(14);
                        cell14.SetCellValue((double)item.Due90Amt);
                        cell14.CellStyle = cellStyleMoney;
                    }
                    if (item.Due120Amt.HasValue)
                    {
                        ICell cell15 = row.CreateCell(15);
                        cell15.SetCellValue((double)item.Due120Amt);
                        cell15.CellStyle = cellStyleMoney;
                    }
                    if (item.Due180Amt.HasValue)
                    {
                        ICell cell16 = row.CreateCell(16);
                        cell16.SetCellValue((double)item.Due180Amt);
                        cell16.CellStyle = cellStyleMoney;
                    }
                    if (item.Due270Amt.HasValue)
                    {
                        ICell cell17 = row.CreateCell(17);
                        cell17.SetCellValue((double)item.Due270Amt);
                        cell17.CellStyle = cellStyleMoney;
                    }
                    if (item.Due360Amt.HasValue)
                    {
                        ICell cell18 = row.CreateCell(18);
                        cell18.SetCellValue((double)item.Due360Amt);
                        cell18.CellStyle = cellStyleMoney;
                    }
                    if (item.DueOver360Amt.HasValue)
                    {
                        ICell cell19 = row.CreateCell(19);
                        cell19.SetCellValue((double)item.DueOver360Amt);
                        cell19.CellStyle = cellStyleMoney;
                    }
                    if (item.TotalAR.HasValue)
                    {
                        ICell cell20 = row.CreateCell(20);
                        cell20.SetCellValue((double)item.TotalAR);
                        cell20.CellStyle = cellStyleMoney;
                    }
                    helper.SetData(rowNo, 21, item.comments);
                    helper.SetData(rowNo, 22, item.Fsr);
                    rowNo++;
                }

                rowNo = 1;
                helper.ActiveSheet = 1;
                ISheet sheetDetail = helper.Book.GetSheetAt(1);
                foreach (var item in invoiceAgings)
                {
                    IRow row = sheetDetail.CreateRow(rowNo);
                    helper.SetData(rowNo, 0, item.CustomerName);
                    helper.SetData(rowNo, 1, item.CustomerNum);
                    ICell cell1 = row.CreateCell(1);
                    cell1.SetCellValue(item.CustomerNum);
                    cell1.CellStyle = cellStyle;
                    ICell cell2 = row.CreateCell(2);
                    cell2.SetCellValue(item.SiteUseId);
                    cell2.CellStyle = cellStyle;
                    ICell cell3 = row.CreateCell(3);
                    cell3.SetCellValue(item.SellingLocationCode);
                    cell3.CellStyle = cellStyle;
                    ICell cell4 = row.CreateCell(4);
                    cell4.SetCellValue(item.Class);
                    cell4.CellStyle = cellStyle;
                    ICell cell5 = row.CreateCell(5);
                    cell5.SetCellValue(item.InvoiceNum);
                    cell5.CellStyle = cellStyle;
                    if (item.InvoiceDate.HasValue)
                    {
                        ICell cell6 = row.CreateCell(6);
                        cell6.SetCellValue(Convert.ToDateTime(item.InvoiceDate).ToString("dd-MMM-yyyy"));
                        cell6.CellStyle = cellStyle;
                    }
                    if (item.DueDate.HasValue)
                    {
                        ICell cell7 = row.CreateCell(7);
                        cell7.SetCellValue(Convert.ToDateTime(item.DueDate).ToString("dd-MMM-yyyy"));
                        cell7.CellStyle = cellStyle;
                    }
                    ICell cell8 = row.CreateCell(8);
                    cell8.SetCellValue(item.CreditTrem);
                    cell8.CellStyle = cellStyle;
                    if (item.CreditLmt.HasValue)
                    {
                        ICell cell9 = row.CreateCell(9);
                        cell9.SetCellValue((double)item.CreditLmt);
                        cell9.CellStyle = cellStyleMoney;
                    }
                    if (item.CreditLmtAcct.HasValue)
                    {
                        ICell cell10 = row.CreateCell(10);
                        cell10.SetCellValue((double)item.CreditLmtAcct);
                        cell10.CellStyle = cellStyleMoney;
                    }
                    ICell cell11 = row.CreateCell(11);
                    cell11.SetCellValue(item.FuncCurrCode);
                    cell11.CellStyle = cellStyle;
                    ICell cell12 = row.CreateCell(12);
                    cell12.SetCellValue(item.Currency);
                    cell12.CellStyle = cellStyle;
                    ICell cell13 = row.CreateCell(13);
                    cell13.SetCellValue(item.Sales);
                    cell13.CellStyle = cellStyle;
                    if (item.DaysLateSys.HasValue)
                    {
                        ICell cell14 = row.CreateCell(14);
                        cell14.SetCellValue((double)item.DaysLateSys);
                        cell14.CellStyle = cellStyle;
                    }
                    if (item.BalanceAmt.HasValue)
                    {
                        ICell cell15 = row.CreateCell(15);
                        cell15.SetCellValue((double)item.BalanceAmt);
                        cell15.CellStyle = cellStyleMoney;
                    }
                    if (item.WoVat_AMT.HasValue)
                    {
                        ICell cell16 = row.CreateCell(16);
                        cell16.SetCellValue((double)item.WoVat_AMT);
                        cell16.CellStyle = cellStyleMoney;
                    }
                    helper.SetData(rowNo, 17, item.AgingBucket);
                    helper.SetData(rowNo, 18, item.CreditTremDescription);
                    helper.SetData(rowNo, 19, item.SellingLocationCode2);
                    helper.SetData(rowNo, 20, item.Ebname);
                    helper.SetData(rowNo, 21, item.Customertype);
                    helper.SetData(rowNo, 22, item.LsrNameHist);
                    helper.SetData(rowNo, 23, item.Fsr);
                    helper.SetData(rowNo, 24, item.LegalEntity);
                    helper.SetData(rowNo, 25, item.Cmpinv);
                    helper.SetData(rowNo, 26, item.SoNum);
                    helper.SetData(rowNo, 27, item.PoNum);
                    helper.SetData(rowNo, 28, item.FsrNameHist);
                    helper.SetData(rowNo, 29, item.LsrNameHist);
                    helper.SetData(rowNo, 30, item.Eb);
                    if (item.RemainingAmtTran.HasValue)
                    {
                        helper.SetData(rowNo, 31, item.RemainingAmtTran);
                    }
                    rowNo++;
                }

                helper.ActiveSheet = 0;

                //设置sheet
                helper.Save(path, true);
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        private void WriteAgingDataToExcel(string temp, string path, IQueryable<CollectingReportDto> list)
        {
            try
            {
                
                ExportService export = new ExportService(temp);
                export.Save(path, true);
                export = new ExportService(path);
                var sheetName = export.Sheets[0];
                export.ActiveSheetName = sheetName;
                using (var scope = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions()
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                }))
                {
                    export.ExportAgingDataList(list.ToList());
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        private void WriteDisputeDataToExcel(string temp, string path, IQueryable<CollectingReportDto> list)
        {
            try
            {
                ExportService export = new ExportService(temp);
                export.Save(path, true);
                export = new ExportService(path);
                var sheetName = export.Sheets[0];
                export.ActiveSheetName = sheetName;
                using (var scope = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions()
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                }))
                {
                    export.ExportDataList(list.ToList());
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        private void WriteOverdueDataToExcel(string temp, string path, IQueryable<CollectingReportDto> list)
        {
            try
            {
                ExportService export = new ExportService(temp);
                export.Save(path, true);
                export = new ExportService(path);
                var sheetName = export.Sheets[0];
                export.ActiveSheetName = sheetName;
                using (var scope = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions()
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                }))
                {
                    export.ExportDataList(list.ToList());
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        public IQueryable<DailyAgingDto> GetQueryAging(string legalEntity, string custNum, string custName, string SiteUseId)
        {
            XcceleratorService collecotr = SpringFactory.GetObjectImpl<XcceleratorService>("XcceleratorService");
            var collecotrList = ",";
            var userId = AppContext.Current.User.EID;
            collecotr.GetUserTeamList(userId).ForEach(s => collecotrList += s.EID + ",");
            var custAgQ = CommonRep.GetQueryable<V_DailyAgingReport>().Where(s => s.REMOVE_FLG == "1");
            if (AppContext.Current.User.ActionPermissions.IndexOf("alldataforsupervisor") >= 0)
            {
                custAgQ = custAgQ.Where(o => o.COLLECTOR == null || o.COLLECTOR.Trim() == string.Empty || collecotrList.Contains("," + o.COLLECTOR + ","));
            }
            else
            {
                custAgQ = custAgQ.Where(o => collecotrList.Contains("," + o.COLLECTOR + ","));
            }

            var result = from ag in custAgQ
                         select new DailyAgingDto
                         {
                             ID = ag.Id,
                             legalEntity = ag.LEGAL_ENTITY,
                             CustomerName = ag.CUSTOMER_NAME,
                             LocalizeCustomerName = ag.LOCALIZE_CUSTOMER_NAME,
                             AccntNumber = ag.CUSTOMER_NUM,
                             SiteUseId = ag.SiteUseId,
                             PaymentTermDesc = ag.CREDIT_TREM,
                             Ebname = ag.Ebname,
                             OverCreditLmt = ag.CREDIT_LIMIT,
                             Collector = ag.COLLECTOR,
                             FuncCurrCode = ag.CURRENCY,
                             TotalFutureDue = ag.TotalFutureDue,
                             Due15Amt = ag.DUE15_AMT,
                             Due30Amt = ag.DUE30_AMT,
                             Due45Amt = ag.DUE45_AMT,
                             Due60Amt = ag.DUE60_AMT,
                             Due90Amt = ag.DUE90_AMT,
                             Due120Amt = ag.DUE120_AMT,
                             Due180Amt = ag.DUE180_AMT,
                             Due270Amt = ag.DUE270_AMT,
                             Due360Amt = ag.DUE360_AMT,
                             DueOver360Amt = ag.DUEOVER360_AMT,

                             LargerDue120Amt = ((ag.DUE150_AMT == null ? 0 : ag.DUE150_AMT)
                             + (ag.DUE180_AMT == null ? 0 : ag.DUE180_AMT)
                             + (ag.DUE210_AMT == null ? 0 : ag.DUE210_AMT)
                             + (ag.DUE240_AMT == null ? 0 : ag.DUE240_AMT)
                             + (ag.DUE270_AMT == null ? 0 : ag.DUE270_AMT)
                             + (ag.DUE300_AMT == null ? 0 : ag.DUE300_AMT)
                             + (ag.DUE330_AMT == null ? 0 : ag.DUE330_AMT)
                             + (ag.DUE360_AMT == null ? 0 : ag.DUE360_AMT)),
                             TotalAR = (ag.DUE150_AMT == null ? 0 : ag.DUE150_AMT)
                             + (ag.DUE180_AMT == null ? 0 : ag.DUE180_AMT)
                             + (ag.DUE210_AMT == null ? 0 : ag.DUE210_AMT)
                             + (ag.DUE240_AMT == null ? 0 : ag.DUE240_AMT)
                             + (ag.DUE270_AMT == null ? 0 : ag.DUE270_AMT)
                             + (ag.DUE300_AMT == null ? 0 : ag.DUE300_AMT)
                             + (ag.DUE330_AMT == null ? 0 : ag.DUE330_AMT)
                             + (ag.DUE360_AMT == null ? 0 : ag.DUE360_AMT)
                             + (ag.DUEOVER360_AMT == null ? 0 : ag.DUEOVER360_AMT)
                             + (ag.DUE15_AMT == null ? 0 : ag.DUE15_AMT)
                             + (ag.DUE30_AMT == null ? 0 : ag.DUE30_AMT)
                             + (ag.DUE45_AMT == null ? 0 : ag.DUE45_AMT)
                             + (ag.DUE60_AMT == null ? 0 : ag.DUE60_AMT)
                             + (ag.DUE90_AMT == null ? 0 : ag.DUE90_AMT)
                             + (ag.DUE120_AMT == null ? 0 : ag.DUE120_AMT)
                             + (ag.TotalFutureDue == null ? 0 : ag.TotalFutureDue),
                             TotalOverDue = ag.DUEOVER_TOTAL_AMT,
                             Lsr = ag.lsr,
                             Fsr = ag.fsr,
                             SpecialNote = ag.specialNote,
                             PTPComment = ag.ptpComment,
                             TotalPTPAmount = ag.totalPTPAmount,
                             DisputeComment = ag.disputeComment,
                             DisputeAmount = ag.disputeAmount,

                             CustomerODPercent = ag.CustomerODPercent,
                             DisputeODPercent = ag.DisputeODPercent,
                             PtpODPercent = ag.PtpODPercent,
                             OthersODPercent = ag.OthersODPercent,
                             CurrentMonthCustomerContact = ag.CurrentMonthCustomerContact,

                             InvoiceMemo = ag.invoiceMemo,
                             comments = ag.Comments,
                             CommentLastDate = ag.CommentLastDate,
                             CommentExpirationDate = ag.CommentExpirationDate

                         };

            if (!String.IsNullOrEmpty(legalEntity) && legalEntity != "undefined" && legalEntity != "null")
            {
                result = result.Where(p => p.legalEntity == legalEntity);
            }

            if (!String.IsNullOrEmpty(custName) && custName != "undefined")
            {
                result = result.Where(p => p.CustomerName.Contains(custName));
            }

            if (!String.IsNullOrEmpty(custNum) && custNum != "undefined")
            {
                result = result.Where(p => p.AccntNumber == custNum);
            }

            if (!String.IsNullOrEmpty(SiteUseId) && SiteUseId != "undefined")
            {
                result = result.Where(p => p.SiteUseId == SiteUseId);
            }

            DataTable nExtDt = GetDailyAgingReportExtendsTable();
            if (nExtDt != null && nExtDt.Rows.Count > 0)
            {
                DailyAgingDto nObj;
                List<DailyAgingDto> nLst = new List<DailyAgingDto>();
                using (var scope = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions()
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                }))
                {
                    nLst = result.ToList();
                    scope.Complete();
                }
                for (int i = 0; i < nExtDt.Rows.Count; i++)
                {
                    int nId = (int)nExtDt.Rows[i]["ID"];
                    nObj = nLst.FirstOrDefault(x => x.ID == nId);
                    if (nObj != null) { 
                        nObj.DisputeAnalysis = nExtDt.Rows[i]["DisputeAnalysis"].ToString();
                        nObj.AutomaticSendMailDate = nExtDt.Rows[i]["AutomaticSendMailDate"].ToString();
                        nObj.AutomaticSendMailCount = (int?)nExtDt.Rows[i]["AutomaticSendMailCount"];
                        nObj.FollowUpCallDate = nExtDt.Rows[i]["FollowUpCallDate"].ToString();
                        nObj.FollowUpCallCount = (int?)nExtDt.Rows[i]["FollowUpCallCount"];
                    }
                }

                return nLst.AsQueryable();
            }
            
            return result;
            
        }

        public IList<InvoiceAgingDto> GetQueryInvoiceAging(string legalEntity, string custNum, string custName, string SiteUseId)
        {
            XcceleratorService collecotr = SpringFactory.GetObjectImpl<XcceleratorService>("XcceleratorService");
            var collecotrList = ",";
            var userId = AppContext.Current.User.EID;
            collecotr.GetUserTeamList(userId).ForEach(s => collecotrList += s.EID + ",");

            var customerAgingQ = CommonRep.GetQueryable<CustomerAging>();
            var invoiceAgingQ = CommonRep.GetQueryable<InvoiceAging>().Where(s => s.TrackStates != "014" && s.TrackStates != "016");
            var q = from i in invoiceAgingQ
                    join c in customerAgingQ
                    on new { CustomerNum = i.CustomerNum, SiteUseId = i.SiteUseId, LegalEntity = i.LegalEntity } equals new { CustomerNum = c.CustomerNum, SiteUseId = c.SiteUseId, LegalEntity = c.LegalEntity }
                    select new { i, c.Collector, c.CustomerName };

            if (AppContext.Current.User.ActionPermissions.IndexOf("alldataforsupervisor") >= 0)
            {
                q = q.Where(o => o.Collector == null || o.Collector.Trim() == string.Empty || collecotrList.Contains("," + o.Collector + ","));
            }
            else
            {
                q = q.Where(o => collecotrList.Contains("," + o.Collector + ","));
            }

            if (!String.IsNullOrEmpty(legalEntity) && legalEntity != "undefined" && legalEntity != "null")
            {
                q = q.Where(p => p.i.LegalEntity == legalEntity);
            }

            if (!String.IsNullOrEmpty(custName) && custName != "undefined")
            {
                q = q.Where(p => p.i.CustomerName.Contains(custName));
            }

            if (!String.IsNullOrEmpty(custNum) && custNum != "undefined")
            {
                q = q.Where(p => p.i.CustomerNum == custNum);
            }

            if (!String.IsNullOrEmpty(SiteUseId) && SiteUseId != "undefined")
            {
                q = q.Where(p => p.i.SiteUseId == SiteUseId);
            }


            var result = q.Select(o => new InvoiceAgingDto()
            {
                CustomerName = o.CustomerName,
                CustomerNum = o.i.CustomerNum,
                SiteUseId = o.i.SiteUseId,
                SellingLocationCode = o.i.SellingLocationCode,
                Class = o.i.Class,
                InvoiceNum = o.i.InvoiceNum,
                InvoiceDate = o.i.InvoiceDate,
                DueDate = o.i.DueDate,
                CreditTrem = o.i.CreditTrem,
                CreditLmt = o.i.CreditLmt,
                CreditLmtAcct = o.i.CreditLmtAcct,
                FuncCurrCode = o.i.FuncCurrCode,
                Currency = o.i.Currency,
                Sales = o.i.Sales,
                DaysLateSys = o.i.DaysLateSys,
                BalanceAmt = o.i.BalanceAmt,
                WoVat_AMT = o.i.WoVat_AMT,
                AgingBucket = o.i.AgingBucket,
                CreditTremDescription = o.i.CreditTremDescription,
                SellingLocationCode2 = o.i.SellingLocationCode2,
                Ebname = o.i.Ebname,
                Customertype = o.i.Customertype,
                LsrNameHist = o.i.LsrNameHist,
                Fsr = o.i.Fsr,
                LegalEntity = o.i.LegalEntity,
                Cmpinv = o.i.Cmpinv,
                SoNum = o.i.SoNum,
                PoNum = o.i.PoNum,
                FsrNameHist = o.i.FsrNameHist,
                Eb = o.i.Eb,
                RemainingAmtTran = o.i.RemainingAmtTran
            });

            List<InvoiceAgingDto> list = new List<InvoiceAgingDto>();
            using (var scope = new TransactionScope(
                     TransactionScopeOption.Required,
                     new TransactionOptions()
                     {
                         IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                     }))
            {
                list = result.ToList();
                scope.Complete();
            }
            return list;

        }

        private IQueryable<CollectingReportDto> GetQueryAging(string filter)
        {
            XcceleratorService collecotr = SpringFactory.GetObjectImpl<XcceleratorService>("XcceleratorService");
            var collecotrList = ",";
            var userId = AppContext.Current.User.EID;
            collecotr.GetUserTeamList(userId).ForEach(s => collecotrList += s.EID + ",");
            //Invoice Aging
            var closed = Helper.EnumToCode<TrackStatus>(TrackStatus.Closed);
            var cancel = Helper.EnumToCode<TrackStatus>(TrackStatus.Cancel);
            var writeoff = Helper.EnumToCode<TrackStatus>(TrackStatus.Write_off_uncollectible_accounts);
            var open = Helper.EnumToCode<TrackStatus>(TrackStatus.Open);
            var invAgQ = CommonRep.GetQueryable<InvoiceAging>();
            var custAgQ = CommonRep.GetQueryable<CustomerAging>();
            //Customer
            var custQ = CommonRep.GetQueryable<Customer>().Where(s => s.IsActive == true);
            if (AppContext.Current.User.ActionPermissions.IndexOf("alldataforsupervisor") >= 0)
            {
            }
            else
            {
                custQ = custQ.Where(o => collecotrList.Contains("," + o.Collector + ","));
            }
            //vat
            var vatQ = CommonRep.GetQueryable<T_INVOICE_VAT>().Where(s => s.LineNumber == 1);
            //config
            var conQ = CommonRep.GetQueryable<SysTypeDetail>().Where(s => s.TypeCode == "029"); //Invoitce Track Status
            var conQ_Reason = CommonRep.GetQueryable<SysTypeDetail>().Where(s => s.TypeCode == "025"); //DisputeReason
            var conQ_Department = CommonRep.GetQueryable<SysTypeDetail>().Where(s => s.TypeCode == "038"); //ActionOwnerDepartment
            var dis_status = CommonRep.GetQueryable<SysTypeDetail>().Where(s => s.TypeCode == "026");
            //dispute
            var disQ = from di in CommonRep.GetQueryable<DisputeInvoice>()
                       join i in CommonRep.GetQueryable<InvoiceAging>() 
                       on new { inv = di.InvoiceId } equals new { inv = i.InvoiceNum }
                       where i.TrackStates == "007" ||
                             i.TrackStates == "008" ||
                             i.TrackStates == "009" ||
                             i.TrackStates == "011" ||
                             i.TrackStates == "012"
                       group di by di.InvoiceId into g
                       select new { g.Key, DisputeID = g.Max(s => s.DisputeId) };

            var dis = from q in CommonRep.GetQueryable<Dispute>()
                      join di in disQ on q.Id equals di.DisputeID
                      select new
                      {
                          di.Key,
                          q.CustomerNum,
                          q.SiteUseId,
                          q.CreateDate,
                          q.IssueReason,
                          q.Comments,
                          q.ActionOwnerDepartmentCode,
                          q.Status,
                          DisputeStatus = (q.Status != "026011" && q.Status != "026012") ? "Y" : ""
                      };
            //ptp
            var ptpQ = from ptp in CommonRep.GetQueryable<T_PTPPayment_Invoice>()
                       join invs in invAgQ on ptp.InvoiceId equals invs.Id
                       where invs.PtpDate != null
                       group ptp by ptp.InvoiceId into g
                       select new { Key = g.Key, PTPId = g.Max(s => s.PTPPaymentId) };//key:InvoiceId,PTPId:PTPPaymentId

            var pt = from pp in CommonRep.GetQueryable<T_PTPPayment>()
                     join ptp in ptpQ on pp.Id equals ptp.PTPId
                     join ph in CommonRep.GetQueryable<V_Invoice_PTPDate>() on ptp.Key equals ph.Id
                     where pp.PTPPaymentType=="PTP"
                     select new { ptp.Key, pp.CustomerNum, pp.SiteUseId, pp.CreateTime, pp.PromiseDate, pp.Comments,
                         PaymentID=pp.Id,
                         IsPartial=pp.IsPartialPay==true?"Y":"N",
                         PartialAmount= pp.IsPartialPay == true?pp.PromissAmount: default(decimal?),
                         PtpAmount= pp.IsPartialPay == true ? default(decimal?) : 0,
                         PTPDateHis = ph.PromiseDate
                     };

            var result = from inv in invAgQ
                         join ca in custAgQ on new { custId = inv.CustomerNum, siteUseId = inv.SiteUseId } equals new { custId = ca.CustomerNum, siteUseId = ca.SiteUseId }
                         join cust in custQ on new { custId = inv.CustomerNum, siteUseId = inv.SiteUseId } equals new { custId = cust.CustomerNum, siteUseId = cust.SiteUseId }
                         join vat in vatQ on new { InvoiceNum = inv.InvoiceNum, Class = inv.Class } equals new { InvoiceNum = vat.Trx_Number, Class = "INV" } into vat_Data
                         from vat_d in vat_Data.DefaultIfEmpty()
                         join d in dis on new { InvoiceNum = inv.InvoiceNum, Class = inv.Class } equals new { InvoiceNum = d.Key, Class = "INV" } into dis_Data
                         from dis_d in dis_Data.DefaultIfEmpty()
                         join p in pt on inv.Id equals p.Key into ptp_Data
                         from ptp_d in ptp_Data.DefaultIfEmpty()
                         join con in conQ on inv.TrackStates equals con.DetailValue into con_Data
                         from con_d in con_Data.DefaultIfEmpty()
                         join con_R in conQ_Reason on dis_d.IssueReason equals con_R.DetailValue into con_RData
                         from con_Rd in con_RData.DefaultIfEmpty()
                         join con_Q in conQ_Department on dis_d.ActionOwnerDepartmentCode equals con_Q.DetailValue into con_QData
                         from con_Qd in con_QData.DefaultIfEmpty()
                         join dis_s in dis_status on dis_d.Status equals dis_s.DetailValue into dis_QData
                         from dis_Qd in dis_QData.DefaultIfEmpty()
                         orderby inv.Id descending
                         select new CollectingReportDto
                         {
                             ID = inv.Id,
                             CustomerName = cust.CustomerName,
                             AccntNumber = inv.CustomerNum,
                             SiteUseId = inv.SiteUseId,
                             SellingLocationCode = inv.SellingLocationCode,
                             CLASS = inv.Class,
                             TrxNum = inv.InvoiceNum,
                             TrxDate = inv.InvoiceDate.HasValue ? inv.InvoiceDate : null,
                             DueDate = inv.DueDate.HasValue ? inv.DueDate : null,
                             PaymentTermName = inv.CreditTrem,
                             OverCreditLmt = inv.CreditLmt,
                             OverCreditLmtAcct = inv.CreditLmtAcct,
                             FuncCurrCode = inv.FuncCurrCode,
                             InvCurrCode = inv.Currency,
                             SalesName = inv.Sales,
                             DueDays = inv.DaysLateSys,
                             AmtRemaining = inv.BalanceAmt,
                             AmountWoVat = inv.WoVat_AMT,
                             AgingBucket = inv.AgingBucket,
                             PaymentTermDesc = inv.CreditTremDescription,
                             SellingLocationCode2 = inv.SellingLocationCode2,
                             Ebname = inv.Ebname,
                             Customertype = inv.Customertype,
                             Isr = inv.CustomerService,
                             Fsr = inv.Fsr,
                             OrgId = inv.LegalEntity,
                             Cmpinv = inv.Cmpinv,
                             SalesOrder = inv.SoNum,
                             Cpo = inv.PoNum,
                             FsrNameHist = inv.FsrNameHist,
                             isrNameHist = inv.LsrNameHist,
                             Eb = inv.Eb,
                             LocalName = cust.LOCALIZE_CUSTOMER_NAME,
                             VatNo = vat_d == null ? "" : vat_d.Trx_Number,
                             VatDate = vat_d == null ? "" : vat_d.VATInvoiceDate,
                             Collector = ca.Collector,
                             CurrentStatus = con_d == null ? "" : con_d.DetailName,
                             Lastupdatedate = inv.TRACK_DATE,
                             ClearingDocument = "",
                             ClearingDate = inv.CloseDate,//Inv  CloseDate
                             PtpIdentifiedDate = ptp_d == null ? null : ptp_d.CreateTime,
                             PtpDate = ptp_d == null ? null : ptp_d.PromiseDate,
                             PtpComment = ptp_d == null ? null : ptp_d.Comments,
                             PtpBroken = ptp_d == null ? null : inv.CloseDate.HasValue ? SqlFunctions.DateDiff("day", ptp_d.PromiseDate, inv.CloseDate) > 0 ? "Y" : "N" : SqlFunctions.DateDiff("day", ptp_d.PromiseDate, DateTime.Now) > 0 ? "Y" : "N",
                             PtpDatehis = ptp_d == null ? null : ptp_d.PTPDateHis, 
                             Dispute = dis_d == null ? "" : dis_d.DisputeStatus,
                             DisputeIdentifiedDate = dis_d == null ? default(DateTime?) : dis_d.CreateDate,
                             DisputeReason = dis_d == null ? "" : con_Rd.DetailName,
                             DisputeComment = dis_d == null ? "" : dis_d.Comments,
                             ActionOwnerDepartment = dis_d == null ? "" : con_Qd.DetailName,
                             ActionOwnerName = cust.CUSTOMER_SERVICE,
                             NextActionDate = dis_d == null ? default(DateTime?) : SqlFunctions.DateAdd("day", 7, dis_d.CreateDate),
                             CommentsHelpNeeded = inv.Comments,
                             LegalEntity = inv.LegalEntity,
                             TrackStates = inv.TrackStates,
                             Payment_Date = inv.Payment_Date,
                             PONum = inv.PoNum,
                             SONum = inv.SoNum,
                             DisputeStatus = dis_d == null ? "" : dis_Qd.DetailName,
                             PaymentID = ptp_d==null? default(int?): ptp_d.PaymentID,
                             IsPartial = ptp_d == null ?"": ptp_d.IsPartial,
                             PartialAmount = (ptp_d != null&& ptp_d.IsPartial=="Y") ?  ptp_d.PartialAmount: default(int?),
                             PtpAmount = (ptp_d != null && ptp_d.IsPartial == "N")? inv.BalanceAmt : default(decimal?),
                             IsForwarder = inv.IsForwarder == true ? "Y" : "",
                             Forwarder = inv.Forwarder
                         };

            if (!string.IsNullOrWhiteSpace(filter))
            {
                filter = filter.Replace("{", "").Replace("}", "").Replace("\"", "");
                string[] array = filter.Split(',');
                foreach (var condition in array)
                {
                    string[] con = condition.Split(':');
                    if (con.Length > 1 && !string.IsNullOrWhiteSpace(con[1]))
                    {
                        var value = con[1];
                        if (con[0] == "legalentity")
                        {
                            if (value != "" && value != "null")
                                result = result.Where(s => s.LegalEntity == value);
                        }
                        else if (con[0] == "custCode")
                        {
                            result = result.Where(s => s.AccntNumber == value || s.AccntNumber.IndexOf(value) >= 0);
                        }
                        else if (con[0] == "custName")
                        {
                            result = result.Where(s => s.CustomerName == value || s.CustomerName.IndexOf(value) >= 0);
                        }
                        else if (con[0] == "siteUseId")
                        {
                            result = result.Where(s => s.SiteUseId == value || s.SiteUseId.IndexOf(value) >= 0);
                        }
                        else if (con[0] == "duedateFrom")
                        {
                            DateTime dtFrom = Convert.ToDateTime(value + " 00:00:00");
                            result = result.Where(s => s.DueDate >= dtFrom);
                        }
                        else if (con[0] == "duedateTo")
                        {
                            DateTime dtTo = Convert.ToDateTime(value + " 23:59:59");
                            result = result.Where(s => s.DueDate <= dtTo);
                        }
                        else if (con[0] == "status")
                        {
                            if (value != "" && value != "null")
                            {
                                if (value == open)
                                {
                                    result = result.Where(s => s.TrackStates == "000" || s.TrackStates == "001" || s.TrackStates == "002" || s.TrackStates == "003"
                                    || s.TrackStates == "004" || s.TrackStates == "005" || s.TrackStates == "006" || s.TrackStates == "007"
                                    || s.TrackStates == "008" || s.TrackStates == "009" || s.TrackStates == "010" || s.TrackStates == "011"
                                    || s.TrackStates == "012" || s.TrackStates == "015"
                                    );
                                }
                                else
                                {
                                    result = result.Where(s => s.TrackStates == value);
                                }
                            }
                        }
                        else if (con[0] == "invoicecode")
                        {
                            result = result.Where(s => s.TrxNum == value || s.TrxNum.IndexOf(value) >= 0);
                        }
                        else if (con[0] == "eb")
                        {
                            if (value != "" && value != "null")
                                result = result.Where(s => s.Eb == value || s.Eb.IndexOf(value) >= 0);
                        }
                        else if (con[0] == "docType")
                        {
                            if (value != "" && value != "null")
                                result = result.Where(s => s.CLASS == value);
                        }
                        else if (con[0] == "poNum")
                        {
                            if (value != "" && value != "null")
                                result = result.Where(s => s.PONum == value || s.PONum.IndexOf(value) >= 0);
                        }
                        else if (con[0] == "soNum")
                        {
                            if (value != "" && value != "null")
                                result = result.Where(s => s.SONum == value || s.SONum.IndexOf(value) >= 0);
                        }
                        else if (con[0] == "creditTerm")
                        {
                            if (value != "" && value != "null")
                                result = result.Where(s => s.PaymentTermName == value || s.PaymentTermName.IndexOf(value) >= 0);
                        }
                        else if (con[0] == "invoiceMemo")
                        {
                            if (value != "" && value != "null")
                                result = result.Where(s => s.CommentsHelpNeeded == value || s.CommentsHelpNeeded.IndexOf(value) >= 0);
                        }
                        else if (con[0] == "ptpDateFrom")
                        {
                            DateTime ptpFrom = Convert.ToDateTime(value + " 00:00:00");
                            result = result.Where(s => s.PtpDate.HasValue && s.PtpDate >= ptpFrom);
                        }
                        else if (con[0] == "ptpDateTo")
                        {
                            DateTime ptpTo = Convert.ToDateTime(value + " 23:59:59");
                            result = result.Where(s => s.PtpDate.HasValue && s.PtpDate <= ptpTo);
                        }
                        else if (con[0] == "invoiceDateFrom")
                        {
                            DateTime trxFrom = Convert.ToDateTime(value + " 00:00:00");
                            result = result.Where(s => s.TrxDate.HasValue && s.TrxDate >= trxFrom);
                        }
                        else if (con[0] == "invoiceDateTo")
                        {
                            DateTime trxTo = Convert.ToDateTime(value + " 23:59:59");
                            result = result.Where(s => s.TrxDate.HasValue && s.TrxDate <= trxTo);
                        }
                    }
                }
            }
            return result;
        }

        private DataTable GetDailyAgingReportExtendsTable()
        {
            DataTable nIdTable = CommonRep.ExecuteDataTable(CommandType.StoredProcedure, "p_QueryDailyAgingReportExtends", null);
            return nIdTable;
        }

        private IQueryable<CollectingReportDto> GetQueryDispute(string filter)
        {
            XcceleratorService collecotr = SpringFactory.GetObjectImpl<XcceleratorService>("XcceleratorService");
            var collecotrList = ",";
            var userId = AppContext.Current.User.EID;
            collecotr.GetUserTeamList(userId).ForEach(s => collecotrList += s.EID + ",");
            //Invoice Aging
            var closed = Helper.EnumToCode<TrackStatus>(TrackStatus.Closed);
            var cancel = Helper.EnumToCode<TrackStatus>(TrackStatus.Cancel);
            var invAgQ = CommonRep.GetQueryable<InvoiceAging>().Where(s => s.Class == "INV");
            //Customer Aging
            var custAgQ = CommonRep.GetQueryable<CustomerAging>();
            //Customer
            var custQ = CommonRep.GetQueryable<Customer>().Where(s => s.IsActive == true);
            if (AppContext.Current.User.ActionPermissions.IndexOf("alldataforsupervisor") >= 0)
            {
            }
            else
            {
                custQ = custQ.Where(o => collecotrList.Contains("," + o.Collector + ","));
            }
            //vat
            var vatQ = CommonRep.GetQueryable<T_INVOICE_VAT>().Where(s => s.LineNumber == 1);
            //config
            var conQ = CommonRep.GetQueryable<SysTypeDetail>().Where(s => s.TypeCode == "029");
            var conQ_Reason = CommonRep.GetQueryable<SysTypeDetail>().Where(s => s.TypeCode == "025"); //DisputeReason
            var conQ_Department = CommonRep.GetQueryable<SysTypeDetail>().Where(s => s.TypeCode == "038"); //ActionOwnerDepartment
            var dis_status = CommonRep.GetQueryable<SysTypeDetail>().Where(s => s.TypeCode == "026");

            //dispute
            var disQ = from di in CommonRep.GetQueryable<DisputeInvoice>()
                       join i in CommonRep.GetQueryable<InvoiceAging>()
                       on new { inv = di.InvoiceId } equals new { inv = i.InvoiceNum }
                       where i.TrackStates == "007" ||
                             i.TrackStates == "008" ||
                             i.TrackStates == "009" ||
                             i.TrackStates == "011" ||
                             i.TrackStates == "012"
                    group di by di.InvoiceId into g
                               select new { g.Key, DisputeID = g.Max(s => s.DisputeId) };

            var dis = from q in CommonRep.GetQueryable<Dispute>()
                      join di in disQ on q.Id equals di.DisputeID
                      select new
                      {
                          di.Key,
                          q.CustomerNum,
                          q.SiteUseId,
                          q.CreateDate,
                          q.IssueReason,
                          q.Comments,
                          q.ActionOwnerDepartmentCode,
                          q.Status,
                          DisputeStatus = (q.Status != "026011" && q.Status != "026012") ? "Y" : ""
                      };
            //ptp
            var ptpQ = from ptp in CommonRep.GetQueryable<T_PTPPayment_Invoice>()
                       group ptp by ptp.InvoiceId into g
                       select new { Key = g.Key, PTPId = g.Max(s => s.PTPPaymentId) };

            var pt = from pp in CommonRep.GetQueryable<T_PTPPayment>()
                     join ptp in ptpQ on pp.Id equals ptp.PTPId
                     join ph in CommonRep.GetQueryable<V_Invoice_PTPDate>() on ptp.Key equals ph.Id
                     select new { ptp.Key, pp.CustomerNum, pp.SiteUseId, pp.CreateTime, pp.PromiseDate, pp.Comments, PTPDatehis = ph.PromiseDate };

            var result = from inv in invAgQ
                         join ca in custAgQ on new { custId = inv.CustomerNum, siteUseId = inv.SiteUseId } equals new { custId = ca.CustomerNum, siteUseId = ca.SiteUseId }
                         join cust in custQ on new { custId = inv.CustomerNum, siteUseId = inv.SiteUseId } equals new { custId = cust.CustomerNum, siteUseId = cust.SiteUseId }
                         join d in dis on inv.InvoiceNum equals d.Key
                         join vat in vatQ on inv.InvoiceNum equals vat.Trx_Number into vat_Data
                         from vat_d in vat_Data.DefaultIfEmpty()
                         join con in conQ on inv.TrackStates equals con.DetailValue into con_Data
                         from con_d in con_Data.DefaultIfEmpty()
                         join p in pt on inv.Id equals p.Key into ptp_Data
                         from ptp_d in ptp_Data.DefaultIfEmpty()
                         join con_R in conQ_Reason on d.IssueReason equals con_R.DetailValue into con_RData
                         from con_Rd in con_RData.DefaultIfEmpty()
                         join con_Q in conQ_Department on d.ActionOwnerDepartmentCode equals con_Q.DetailValue into con_QData
                         from con_Qd in con_QData.DefaultIfEmpty()
                         join dis_s in dis_status on d.Status equals dis_s.DetailValue into dis_QData
                         from dis_Qd in dis_QData.DefaultIfEmpty()
                         orderby inv.Id descending
                         select new CollectingReportDto
                         {
                             ID = inv.Id,
                             CustomerName = cust.CustomerName,
                             AccntNumber = inv.CustomerNum,
                             SiteUseId = inv.SiteUseId,
                             SellingLocationCode = inv.SellingLocationCode,
                             CLASS = inv.Class,
                             TrxNum = inv.InvoiceNum,
                             TrxDate = inv.InvoiceDate.HasValue ? inv.InvoiceDate : null,
                             DueDate = inv.DueDate.HasValue ? inv.DueDate : null,
                             PaymentTermName = inv.CreditTrem,
                             OverCreditLmt = inv.CreditLmt,
                             OverCreditLmtAcct = inv.CreditLmtAcct,
                             FuncCurrCode = inv.FuncCurrCode,
                             InvCurrCode = inv.Currency,
                             SalesName = inv.Sales,
                             DueDays = inv.DaysLateSys,
                             AmtRemaining = inv.BalanceAmt,
                             AmountWoVat = inv.WoVat_AMT,
                             AgingBucket = inv.AgingBucket,
                             PaymentTermDesc = inv.CreditTremDescription,
                             SellingLocationCode2 = inv.SellingLocationCode2,
                             Ebname = inv.Ebname,
                             Customertype = inv.Customertype,
                             Isr = inv.CustomerService,
                             Fsr = inv.Fsr,
                             OrgId = inv.LegalEntity,
                             Cmpinv = inv.Cmpinv,
                             SalesOrder = inv.SoNum,
                             Cpo = inv.PoNum,
                             FsrNameHist = inv.FsrNameHist,
                             isrNameHist = inv.LsrNameHist,
                             Eb = inv.Eb,
                             LocalName = cust.LOCALIZE_CUSTOMER_NAME,
                             VatNo = vat_d == null ? "" : vat_d.Trx_Number,
                             VatDate = vat_d == null ? "" : vat_d.VATInvoiceDate,
                             Collector = ca.Collector,
                             CurrentStatus = con_d == null ? "" : con_d.DetailName,
                             Lastupdatedate = inv.TRACK_DATE,
                             ClearingDocument = "",
                             ClearingDate = inv.CloseDate,
                             PtpIdentifiedDate = ptp_d == null ? null : ptp_d.CreateTime,
                             PtpDate = ptp_d == null ? null : ptp_d.PromiseDate,
                             PtpDatehis = ptp_d == null ? null : ptp_d.PTPDatehis,
                             PtpBroken = ptp_d == null ? null : inv.CloseDate.HasValue ? SqlFunctions.DateDiff("day", ptp_d.PromiseDate, inv.CloseDate) > 0 ? "Y" : "N" : SqlFunctions.DateDiff("day", ptp_d.PromiseDate, DateTime.Now) > 0 ? "Y" : "N",
                             PtpComment = ptp_d == null ? null : ptp_d.Comments,
                             Dispute = d == null ? "" : d.DisputeStatus,
                             DisputeIdentifiedDate = d == null ? default(DateTime?) : d.CreateDate,
                             DisputeReason = d == null ? "" : con_Rd.DetailName,
                             DisputeComment = d == null ? "" : d.Comments,
                             ActionOwnerDepartment = d == null ? "" : con_Qd.DetailName,
                             ActionOwnerName = cust.CUSTOMER_SERVICE,
                             NextActionDate = d == null ? default(DateTime?) : SqlFunctions.DateAdd("day", 7, d.CreateDate),
                             CommentsHelpNeeded = inv.Comments,
                             TrackStates = inv.TrackStates,
                             States = d == null ? "" : d.Status,
                             IssueReason = d == null ? "" : d.IssueReason,
                             ActionOwnerDepartmentCode = d.ActionOwnerDepartmentCode,
                             DisputeStatus = d == null ? "" : dis_Qd.DetailName,
                             IsForwarder = inv.IsForwarder == true ? "Y" : "",
                             Forwarder = inv.Forwarder
                         };
            var closedFlag = false;
            var disclosedFlag = false;
            if (!string.IsNullOrWhiteSpace(filter))
            {
                filter = filter.Replace("{", "").Replace("}", "").Replace("\"", "");
                string[] array = filter.Split(',');
                foreach (var condition in array)
                {
                    string[] con = condition.Split(':');
                    if (con.Length > 1 && !string.IsNullOrWhiteSpace(con[1]))
                    {
                        var value = con[1];
                        if (con[0] == "legalEntity")
                        {
                            if (value != "" && value != "null")
                                result = result.Where(s => s.LegalEntity == value);
                        }
                        else if (con[0] == "custCode")
                        {
                            result = result.Where(s => s.AccntNumber.Contains(value));
                        }
                        else if (con[0] == "custName")
                        {
                            result = result.Where(s => s.CustomerName.Contains(value));
                        }
                        else if (con[0] == "siteUseId")
                        {
                            result = result.Where(s => s.SiteUseId.Contains(value));
                        }
                        else if (con[0] == "duedateFrom")
                        {
                            DateTime dtFrom = Convert.ToDateTime(value + " 00:00:00");
                            result = result.Where(s => s.DueDate >= dtFrom);
                        }
                        else if (con[0] == "duedateTo")
                        {
                            DateTime dtTo = Convert.ToDateTime(value + " 23:59:59");
                            result = result.Where(s => s.DueDate <= dtTo);
                        }
                        else if (con[0] == "status")
                        {
                            if (value != "" && value != "null")
                            {
                                result = result.Where(s => s.States == value);
                            }
                        }
                        else if (con[0] == "trackstatus")
                        {
                            if (value != "" && value != "null")
                            {
                                result = result.Where(s => s.TrackStates == value);
                            }
                        }
                        else if (con[0] == "reason")
                        {
                            if (value != "" && value != "null")
                                result = result.Where(s => s.IssueReason == value);
                        }
                        else if (con[0] == "department")
                        {
                            if (value != "" && value != "null")
                                result = result.Where(s => s.ActionOwnerDepartmentCode == value);
                        }
                        else if (con[0] == "invoicecode")
                        {
                            result = result.Where(s => s.TrxNum.Contains(value));
                        }
                        else if (con[0] == "eb")
                        {
                            if (value != "" && value != "null")
                                result = result.Where(s => s.Eb.Contains(value));
                        }
                        else if (con[0] == "closed")
                        {
                            if (value != "")
                            {
                                var temp = false;
                                bool.TryParse(value, out temp);
                                closedFlag = temp;
                            }
                        }
                        else if (con[0] == "disclosed")
                        {
                            if (value != "")
                            {
                                var temp = false;
                                bool.TryParse(value, out temp);
                                disclosedFlag = temp;
                            }
                        }
                    }
                }
            }
            if (!disclosedFlag)
            {
                result = result.Where(s => s.States != "026011" && s.States != "026012");
            }
            else
            {
                result = result.Where(s => s.States == "026011" || s.States == "026012");
            }
            if (closedFlag)
            {
                result = result.Where(s => s.TrackStates == "013" || s.TrackStates == "014" || s.TrackStates == "016");
            }
            else
            {
                result = result.Where(s => s.TrackStates == "001" || s.TrackStates == "002" || s.TrackStates == "003"
                || s.TrackStates == "004" || s.TrackStates == "005" || s.TrackStates == "006" || s.TrackStates == "007"
                || s.TrackStates == "008" || s.TrackStates == "009" || s.TrackStates == "010" || s.TrackStates == "011"
                || s.TrackStates == "012" || s.TrackStates == "015");
            }
            return result;
        }

        private IQueryable<CollectingReportDto> GetQueryOverdue(string filter)
        {
            IQueryable<CollectingReportDto> result = null;
            try
            {
                XcceleratorService collecotr = SpringFactory.GetObjectImpl<XcceleratorService>("XcceleratorService");
                var collecotrList = ",";
                var userId = AppContext.Current.User.EID;
                collecotr.GetUserTeamList(userId).ForEach(s => collecotrList += s.EID + ",");
                //Invoice Aging
            
                var invAgQ = CommonRep.GetQueryable<InvoiceAging>().Where(s => s.Class == "INV" && s.DaysLateSys > 0);

                var custAgQ = CommonRep.GetQueryable<CustomerAging>();
                //Customer
                var custQ = CommonRep.GetQueryable<Customer>().Where(s => s.IsActive == true);
                if (AppContext.Current.User.ActionPermissions.IndexOf("alldataforsupervisor") >= 0)
                {
                }
                else
                {
                    custQ = custQ.Where(o => collecotrList.Contains("," + o.Collector + ","));
                }
                //vat
                var vatQ = CommonRep.GetQueryable<T_INVOICE_VAT>().Where(s => s.LineNumber == 1);
                //config
                var conQ = CommonRep.GetQueryable<SysTypeDetail>().Where(s => s.TypeCode == "029");
                var conQ_Reason = CommonRep.GetQueryable<SysTypeDetail>().Where(s => s.TypeCode == "025"); //DisputeReason
                var conQ_Department = CommonRep.GetQueryable<SysTypeDetail>().Where(s => s.TypeCode == "038"); //ActionOwnerDepartment
                var dis_status = CommonRep.GetQueryable<SysTypeDetail>().Where(s => s.TypeCode == "026");
                //dispute
                var disQ = from di in CommonRep.GetQueryable<DisputeInvoice>()
                           group di by di.InvoiceId into g
                           select new { g.Key, DisputeID = g.Max(s => s.DisputeId) };

                var dis = from q in CommonRep.GetQueryable<Dispute>()
                          join di in disQ on q.Id equals di.DisputeID
                          select new
                          {
                              di.Key,
                              q.CustomerNum,
                              q.SiteUseId,
                              q.CreateDate,
                              q.IssueReason,
                              q.Comments,
                              q.ActionOwnerDepartmentCode,
                              q.Status,
                              DisputeStatus = (q.Status != "026011" && q.Status != "026012") ? "Y" : ""
                          };
                //ptp
                var ptpQ = from ptp in CommonRep.GetQueryable<T_PTPPayment_Invoice>()
                           group ptp by ptp.InvoiceId into g
                           select new { Key = g.Key, PTPId = g.Max(s => s.PTPPaymentId) };

                var pt = from pp in CommonRep.GetQueryable<T_PTPPayment>()
                         join ptp in ptpQ on pp.Id equals ptp.PTPId
                         join ph in CommonRep.GetQueryable<V_Invoice_PTPDate>() on ptp.Key equals ph.Id
                         select new { ptp.Key, pp.CustomerNum, pp.SiteUseId, pp.CreateTime, pp.PromiseDate, pp.Comments,PTPDatehis = ph.PromiseDate };

                result = from inv in invAgQ
                         join ca in custAgQ on new { custId = inv.CustomerNum, siteUseId = inv.SiteUseId } equals new { custId = ca.CustomerNum, siteUseId = ca.SiteUseId }
                         join cust in custQ on new { custId = inv.CustomerNum, siteUseId = inv.SiteUseId } equals new { custId = cust.CustomerNum, siteUseId = cust.SiteUseId }
                         join vat in vatQ on inv.InvoiceNum equals vat.Trx_Number into vat_Data
                         from vat_d in vat_Data.DefaultIfEmpty()
                         join con in conQ on inv.TrackStates equals con.DetailValue into con_Data
                         from con_d in con_Data.DefaultIfEmpty()
                         join d in dis on inv.InvoiceNum equals d.Key into dis_Data
                         from dis_d in dis_Data.DefaultIfEmpty()
                         join p in pt on inv.Id equals p.Key into ptp_Data
                         from ptp_d in ptp_Data.DefaultIfEmpty()
                         join con_R in conQ_Reason on dis_d.IssueReason equals con_R.DetailValue into con_RData
                         from con_Rd in con_RData.DefaultIfEmpty()
                         join con_Q in conQ_Department on dis_d.ActionOwnerDepartmentCode equals con_Q.DetailValue into con_QData
                         from con_Qd in con_QData.DefaultIfEmpty()
                         join dis_s in dis_status on dis_d.Status equals dis_s.DetailValue into dis_QData
                         from dis_Qd in dis_QData.DefaultIfEmpty()
                         orderby inv.Id descending
                         select new CollectingReportDto
                         {
                             ID = inv.Id,
                             CustomerName = cust.CustomerName,
                             AccntNumber = inv.CustomerNum,
                             SiteUseId = inv.SiteUseId,
                             SellingLocationCode = inv.SellingLocationCode,
                             CLASS = inv.Class,
                             TrxNum = inv.InvoiceNum,
                             TrxDate = inv.InvoiceDate.HasValue ? inv.InvoiceDate : null,
                             DueDate = inv.DueDate.HasValue ? inv.DueDate : null,
                             OverdueReason = inv.OverdueReason,
                             PaymentTermName = inv.CreditTrem,
                             OverCreditLmt = inv.CreditLmt,
                             OverCreditLmtAcct = inv.CreditLmtAcct,
                             FuncCurrCode = inv.FuncCurrCode,
                             InvCurrCode = inv.Currency,
                             SalesName = inv.Sales,
                             DueDays = inv.DaysLateSys,
                             AmtRemaining = inv.BalanceAmt,
                             AmountWoVat = inv.WoVat_AMT,
                             AgingBucket = inv.AgingBucket,
                             PaymentTermDesc = inv.CreditTremDescription,
                             SellingLocationCode2 = inv.SellingLocationCode2,
                             Ebname = inv.Ebname,
                             Customertype = inv.Customertype,
                             Isr = inv.CustomerService,
                             Fsr = inv.Fsr,
                             OrgId = inv.LegalEntity,
                             Cmpinv = inv.Cmpinv,
                             SalesOrder = inv.SoNum,
                             Cpo = inv.PoNum,
                             FsrNameHist = inv.FsrNameHist,
                             isrNameHist = inv.LsrNameHist,
                             Eb = inv.Eb,
                             LocalName = cust.LOCALIZE_CUSTOMER_NAME,
                             VatNo = vat_d == null ? "" : vat_d.Trx_Number,
                             VatDate = vat_d == null ? "" : vat_d.VATInvoiceDate,
                             Collector = ca.Collector,
                             CurrentStatus = con_d == null ? "" : con_d.DetailName,
                             Lastupdatedate = inv.TRACK_DATE,
                             ClearingDocument = "",
                             ClearingDate = inv.CloseDate,//Inv  CloseDate
                             PtpIdentifiedDate = ptp_d == null ? null : ptp_d.CreateTime,
                             PtpDate = ptp_d == null ? null : ptp_d.PromiseDate,
                             PtpDatehis = ptp_d == null ? null : ptp_d.PTPDatehis,
                             PtpBroken = ptp_d == null ? null : inv.CloseDate.HasValue ? SqlFunctions.DateDiff("day", ptp_d.PromiseDate, inv.CloseDate) > 0 ? "Y" : "N" : SqlFunctions.DateDiff("day", ptp_d.PromiseDate, DateTime.Now) > 0 ? "Y" : "N",
                             PtpComment = ptp_d == null ? null : ptp_d.Comments,
                             Dispute = dis_d == null ? "" : dis_d.DisputeStatus,
                             DisputeIdentifiedDate = dis_d == null ? default(DateTime?) : dis_d.CreateDate,
                             DisputeReason = dis_d == null ? "" : con_Rd.DetailName,
                             DisputeComment = dis_d == null ? "" : dis_d.Comments,
                             ActionOwnerDepartment = dis_d == null ? "" : con_Qd.DetailName,
                             ActionOwnerName = cust.CUSTOMER_SERVICE,
                             NextActionDate = dis_d == null ? default(DateTime?) : SqlFunctions.DateAdd("day", 7, dis_d.CreateDate),
                             CommentsHelpNeeded = inv.Comments,
                             TrackStates = inv.TrackStates,
                             States = dis_d == null ? "" : dis_d.Status,
                             DisputeStatus = dis_d == null ? "" : dis_Qd.DetailName,
                             IsForwarder = inv.IsForwarder == true ? "Y" : "",
                             Forwarder = inv.Forwarder
                         };

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    filter = filter.Replace("{", "").Replace("}", "").Replace("\"", "");
                    string[] array = filter.Split(',');
                    foreach (var condition in array)
                    {
                        string[] con = condition.Split(':');
                        if (con.Length > 1 && !string.IsNullOrWhiteSpace(con[1]))
                        {
                            var value = con[1];
                            if (con[0] == "legalEntity")
                            {
                                if (value != "" && value != "null")
                                    result = result.Where(s => s.OrgId == value);
                            }
                            else if (con[0] == "custCode")
                            {
                                result = result.Where(s => s.AccntNumber.Contains(value));
                            }
                            else if (con[0] == "custName")
                            {
                                result = result.Where(s => s.CustomerName.Contains(value));
                            }
                            else if (con[0] == "siteUseId")
                            {
                                result = result.Where(s => s.SiteUseId.Contains(value));
                            }
                            else if (con[0] == "duedate")
                            {
                                if (value != "" && value != "null")
                                {
                                    switch (value)
                                    {
                                        case "037001":
                                            result = result.Where(s => s.DueDays >= 1 && s.DueDays <= 15);
                                            break;
                                        case "037002":
                                            result = result.Where(s => s.DueDays >= 16 && s.DueDays <= 30);
                                            break;
                                        case "037003":
                                            result = result.Where(s => s.DueDays >= 31 && s.DueDays <= 45);
                                            break;
                                        case "037004":
                                            result = result.Where(s => s.DueDays >= 46 && s.DueDays <= 60);
                                            break;
                                        case "037005":
                                            result = result.Where(s => s.DueDays >= 61 && s.DueDays <= 90);
                                            break;
                                        case "037006":
                                            result = result.Where(s => s.DueDays >= 91 && s.DueDays <= 120);
                                            break;
                                        case "037007":
                                            result = result.Where(s => s.DueDays >= 121);
                                            break;
                                        default:
                                            result = result.Where(s => s.DueDays > 0);
                                            break;
                                    }
                                }
                            }
                            else if (con[0] == "invoicecode")
                            {
                                result = result.Where(s => s.TrxNum.Contains(value));
                            }
                            else if (con[0] == "overdueReason")
                            {
                                if (value != "" && value != "null")
                                {
                                    result = result.Where(s => s.OverdueReason == value);
                                }
                            }
                            else if (con[0] == "eb")
                            {
                                if (value != "" && value != "null")
                                    result = result.Where(s => s.Eb.Contains(value));
                            }
                        }
                    }
                }

            }
            catch (DbEntityValidationException ex)
            {
                Helper.Log.Error(ex.Message, ex);

                StringBuilder errors = new StringBuilder();
                IEnumerable<DbEntityValidationResult> validationResult = ex.EntityValidationErrors;
                foreach (DbEntityValidationResult r in validationResult)
                {
                    ICollection<DbValidationError> validationError = r.ValidationErrors;
                    foreach (DbValidationError err in validationError)
                    {
                        errors.Append(err.PropertyName + ":" + err.ErrorMessage + "\r\n");
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw;
            }
            return result;
        }
    }
}
