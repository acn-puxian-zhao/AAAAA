using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.Domain.Repositories;
using NPOI.SS.UserModel;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Intelligent.OTC.Business
{
    public class ReportFeedbackService
    {
        public OTCRepository CommonRep { get; set; }

        public List<ReportFeedbackSumItem> GetSum()
        {
            IEnumerable<ReportFeedbackSumItem> result = null;

            try
            {
                string sql = @"SELECT [Region]
                                  ,[SendCount] as TotalCount
                                  ,[ResponseCount] as FeedbackCount
                              FROM [dbo].[V_Report_Feedback_SUM]";

                object[] parameters = new object[0];
                result = CommonRep.ExecuteSqlQuery<ReportFeedbackSumItem>(sql, parameters).OrderBy(o=>o.Region).ToList();
            }
            catch (Exception ex)
            {
                result = new List<ReportFeedbackSumItem>();
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("V_Report_Feedback_SUM 异常!");
            }
           
            return result.ToList();
        }

        public List<ReportNotFeedbackItem> GetNotFeedbackList(int page, int pageSize, out int total)
        {
            IEnumerable<ReportNotFeedbackItem> result = null;

            try
            {
                string sql = @" SELECT Region                   as Region,
                                       COLLECTOR                as Collector,
                                       Organization             as Organization,
                                       CUSTOMERNAME             as CustomerName,    
                                       CREDITTREM               as CreditTerm,
                                       CUSTOMER_NUM             as CustomerNum,
                                       SiteUseId                as SiteUseId,
                                       CLASS                    as Class,
                                       INVOICE_NUM              as InvoiceNum,
                                       invoice_date             as InvoiceDate,
                                       DUE_DATE                 as DueDate,
                                       FuncCurrCode             as FuncCurrCode,
                                       currency                 as Currency,
                                       DAYS_LATE_SYS            as DaysLateSys,
                                       BALANCE_AMT              as BalanceAmt,
                                       AgingBucket              as AgingBucket,
                                       CreditTremDescription    as CreditTremDescription,
                                       Ebname                   as Ebname,
                                       LsrNameHist              as LsrNameHist,
                                       FsrNameHist              as FsrNameHist,
                                       LEGAL_ENTITY             as LegalEntity,
                                       Cmpinv                   as Cmpinv,
                                       SO_NUM                   as SoNum,
                                       PO_MUM                   as PoNum,
                                       PTP_DATE                 as PtpDate,
                                       OverdueReason            as OverdueReason,
                                       COMMENTS                 as Comments,
                                       Status                   as Status,
                                       CloseDate                as CloseDate
                                FROM V_Report_Feedback_SUM_NotBackDetail";

                object[] parameters = new object[0];
                result = CommonRep.ExecuteSqlQuery<ReportNotFeedbackItem>(sql, parameters).OrderBy(o => o.Collector).ThenBy(o => o.Region);
                total = result.Count();
                result = result.Skip((page - 1) * pageSize).Take(pageSize);
            }
            catch (Exception ex)
            {
                result = new List<ReportNotFeedbackItem>();
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("V_Report_Feedback_SUM_NotBackDetail 异常!");
            }

            return result.ToList();
        }

        public List<ReportHasFeedbackItem> GetHasFeedbackList(int page, int pageSize, out int total)
        {
            IEnumerable<ReportHasFeedbackItem> result = null;

            try
            {
                string sql = @" SELECT Region                   as Region,
                                       COLLECTOR                as Collector,
                                       Organization             as Organization,
                                       CUSTOMERNAME             as CustomerName,    
                                       CREDITTREM               as CreditTerm,
                                       CUSTOMER_NUM             as CustomerNum,
                                       SiteUseId                as SiteUseId,
                                       CLASS                    as Class,
                                       INVOICE_NUM              as InvoiceNum,
                                       invoice_date             as InvoiceDate,
                                       DUE_DATE                 as DueDate,
                                       FuncCurrCode             as FuncCurrCode,
                                       currency                 as Currency,
                                       DAYS_LATE_SYS            as DaysLateSys,
                                       BALANCE_AMT              as BalanceAmt,
                                       AgingBucket              as AgingBucket,
                                       CreditTremDescription    as CreditTremDescription,
                                       Ebname                   as Ebname,
                                       LsrNameHist              as LsrNameHist,
                                       FsrNameHist              as FsrNameHist,
                                       LEGAL_ENTITY             as LegalEntity,
                                       Cmpinv                   as Cmpinv,
                                       SO_NUM                   as SoNum,
                                       PO_MUM                   as PoNum,
                                       PTP_DATE                 as PtpDate,
                                       OverdueReason            as OverdueReason,
                                       COMMENTS                 as Comments,
                                       Status                   as Status,
                                       CloseDate                as CloseDate
                                FROM V_Report_Feedback_SUM_HasBackDetail";

                object[] parameters = new object[0];
                result = CommonRep.ExecuteSqlQuery<ReportHasFeedbackItem>(sql, parameters).OrderBy(o => o.Collector).ThenBy(o => o.Region);
                total = result.Count();
                result = result.Skip((page - 1) * pageSize).Take(pageSize);
            }
            catch (Exception ex)
            {
                result = new List<ReportHasFeedbackItem>();
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("V_Report_Feedback_SUM_HasBackDetail 异常!");
            }

            return result.ToList();
        }

        public List<ReportFeedbackDetailItem> GetDetails(int page, int pageSize, out int total)
        {
            IEnumerable<ReportFeedbackDetailItem> result = null;

            try
            {
                PeriodControl period = CommonRep.GetDbSet<PeriodControl>().Where(s => s.PeriodBegin <= DateTime.Now && DateTime.Now <= s.PeriodEnd).FirstOrDefault();
                if (period == null) { throw new Exception("Period not exists."); }
                string strStartDate = period.PeriodBegin.ToString("yyyy-MM-dd");
                string strEndDate = period.PeriodEnd.ToString("yyyy-MM-dd");

                string sql = string.Format(getFeedbackDetailsSql(), strStartDate, strEndDate);

                object[] parameters = new object[0];
                result = CommonRep.ExecuteSqlQuery<ReportFeedbackDetailItem>(sql, parameters).OrderBy(o => o.Region).ThenBy(o => o.Collector);
                total = result.Count();
                result = result.Skip((page - 1) * pageSize).Take(pageSize);
            }

            catch (Exception ex)
            {
                result = new List<ReportFeedbackDetailItem>();
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("V_Report_Feedback_Detail 异常!");
            }

            return result.ToList();
        }

        public List<FeedbackHistoryDto> getfeedbackhistory(int page, int pageSize, out int total) {

            IEnumerable<FeedbackHistoryDto> result = null;
            try
            {
                string sql = @"select reportdate,
                                       filename,
                                       filepath
                                   from T_Feedback_Report_History with (nolock)";
                object[] parameters = new object[0];
                result = CommonRep.ExecuteSqlQuery<FeedbackHistoryDto>(sql, parameters).OrderByDescending(o => o.reportdate);
                total = result.Count();
                result = result.Skip((page - 1) * pageSize).Take(pageSize);
            }
            catch (Exception ex)
            {
                result = new List<FeedbackHistoryDto>();
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("getfeedbackhistory 异常!");
            }
            return result.ToList();
        }

        public string getFeedbackDetailsSql() {
            string strSQL =  @"SELECT c.Region,
                            c.COLLECTOR as Collector,
                            c.Organization,
                            c.CUSTOMER_NAME AS CustomerName,
                            c.CREDIT_TREM AS CreditTerm,
                            aging.CUSTOMER_NUM as CustomerNum,
                            aging.SiteUseId,
                            aging.CLASS as Class,
                            aging.INVOICE_NUM as InvoiceNum,
                            CONVERT(VARCHAR(10), aging.INVOICE_DATE, 120) AS InvoiceDate,
                            CONVERT(VARCHAR(10), aging.DUE_DATE, 120) AS DueDate,
                            aging.FuncCurrCode,
                            CURRENCY as Currency,
                            aging.DAYS_LATE_SYS as DueDays,
                            aging.BALANCE_AMT as InvoiceAmount,
                            aging.AgingBucket as AgingBucket,
                            aging.CreditTremDescription as CreditTremDesc,
                            aging.Ebname as EbName,
                            aging.LsrNameHist AS CS,
                            aging.FsrNameHist AS Sales,
                            aging.LEGAL_ENTITY AS LegalEntity,
                            aging.Cmpinv AS Cmpinv,
                            aging.SO_NUM AS SONum,
                            aging.PO_MUM AS PONum,
                            CONVERT(VARCHAR(10), aging.PTP_DATE, 120) AS PtpDate,
                            aging.OverdueReason,
                            aging.COMMENTS,
                            aging.MemoExpirationDate,
                            (CASE
                                WHEN aging.TRACK_STATES = '014' THEN
                                    'Closed'
                                ELSE
                                    'Opening'
                            END
                            ) AS Status,
                            (CASE
                                WHEN aging.TRACK_STATES = '014' THEN
                                    CONVERT(VARCHAR(10), aging.CloseDate, 120)
                                ELSE
                                    ''
                            END
                            ) AS CloseDate,
                            (CASE
                                WHEN
                                (
                                    ISNULL(aging.COMMENTS, '') <> ''
                                    OR ISNULL(aging.OverdueReason, '') <> ''
                                    OR aging.PTP_DATE is not NULL
                                ) THEN
                                    CONVERT(VARCHAR(10), aging.TRACK_DATE, 120)
                                ELSE
                                    NULL
                            END
                            ) feedback,
                            (CASE
                                WHEN aging.PERIOD_ID =
                                (
                                    SELECT ID
                                    FROM T_PERIOD_CONTROL with (nolock)
                                    WHERE CONVERT(VARCHAR(10), PERIOD_BEGIN, 120) >= '{0}'
                                            AND CONVERT(VARCHAR(10), PERIOD_END, 120) <= '{1}'
                                ) THEN
                                    STUFF(
                                    (
                                        SELECT ',' + CONVERT(VARCHAR(10), [t].[ACTION_DATE], 120)
                                        FROM
                                        (
                                            SELECT DISTINCT
                                                    SiteUseId,
                                                    ALERT_TYPE,
                                                    CONVERT(VARCHAR(10), ACTION_DATE, 120) AS ACTION_DATE,
                                                    STATUS
                                            FROM dbo.T_COLLECTOR_ALERT with (nolock)
                                            WHERE STATUS = 'Finish'
                                        ) t
                                        WHERE t.SiteUseId = c.SiteUseId
                                                AND t.ALERT_TYPE IN ( 1, 2, 3 )
                                                AND t.STATUS = 'Finish'
                                                AND CONVERT(VARCHAR(10), [t].[ACTION_DATE], 120) >= CONVERT(
                                                                                                                VARCHAR(10),
                                                                                                                aging.CREATE_DATE,
                                                                                                                120
                                                                                                            )
                                                AND t.ACTION_DATE >= '{0}'
                                                AND t.ACTION_DATE <= '{1}'
                                        ORDER BY t.ACTION_DATE
                                        FOR XML PATH('')
                                    ),
                                    1,
                                    1,
                                    ''
                                            )
                                ELSE
                                    ''
                            END
                            ) AS SendDate
                    FROM T_INVOICE_AGING AS aging with (nolock)
                        JOIN T_CUSTOMER AS c with (nolock)
                            ON aging.SiteUseId = c.SiteUseId
                    WHERE ISNULL(c.COLLECTOR, '') <> ''
                            AND
                            (
                                TRACK_STATES <> '014'
                                OR
                                (
                                    aging.CloseDate >= '{0}'
                                    AND TRACK_STATES = '014'
                                )
                            )
                            AND aging.CREATE_DATE <= '{1} 23:59:59'

                    ";
            Helper.Log.Info(strSQL);
            return strSQL;
        }

        public IQueryable<ReportFeedbackDetailItem> GetDetails()
        {
            IQueryable<ReportFeedbackDetailItem> result = null;

            try
            {
                PeriodControl period = CommonRep.GetDbSet<PeriodControl>().Where(s => s.PeriodBegin <= DateTime.Now && DateTime.Now <= s.PeriodEnd).FirstOrDefault();
                if (period == null) { throw new Exception("Period not exists."); }
                string strStartDate = period.PeriodBegin.ToString("yyyy-MM-dd");
                string strEndDate = period.PeriodEnd.ToString("yyyy-MM-dd");

                string sql = string.Format(getFeedbackDetailsSql(), strStartDate, strEndDate);

                object[] parameters = new object[0];
                result = CommonRep.ExecuteSqlQuery<ReportFeedbackDetailItem>(sql, parameters).OrderBy(o => o.Region).ThenBy(o => o.Collector).AsQueryable();
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("V_Report_Feedback_Detail 异常!");
            }

            return result;
        }

        public string Export()
        {
            string custPathName = "OverdueportPath";
            string tempFile = HttpContext.Current.Server.MapPath("~/Template/ReportFeedbackTemplate.xlsx");
            string targetFoler = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[custPathName].ToString());
            string targetFile = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[custPathName].ToString() + "FeedbackReport_" + AppContext.Current.User.EID + ".xlsx");
            if (Directory.Exists(targetFoler) == false)
            {
                Directory.CreateDirectory(targetFoler);
            }

            try
            {
                var statistics = GetSum();
                int total = 0;
                var notFeedback = GetNotFeedbackList(1,999999999, out total);
                var hasFeedback = GetHasFeedbackList(1, 999999999, out total);
                WriteFeedbackToExcel(tempFile, targetFile, statistics, notFeedback, hasFeedback);
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Export 异常!");
            }
          
            HttpRequest request = HttpContext.Current.Request;
            StringBuilder appUriBuilder = new StringBuilder(request.Url.Scheme);
            appUriBuilder.Append(Uri.SchemeDelimiter);
            appUriBuilder.Append(request.Url.Authority);
            if (String.Compare(request.ApplicationPath, @"/") != 0)
            {
                appUriBuilder.Append(request.ApplicationPath);
            }

            string virPathName = appUriBuilder.ToString() + ConfigurationManager.AppSettings[custPathName].ToString().Trim('~') + "FeedbackReport_" + AppContext.Current.User.EID + ".xlsx";
            return virPathName;
        }

        public string ExportDetail()
        {
            try
            {
                string custPathName = "OverdueportPath";
                string tempFile = HttpContext.Current.Server.MapPath("~/Template/ReportFeedbackDetailTemplate.xlsx");
                string targetFoler = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[custPathName].ToString());
                string strUserId = AppContext.Current.User.EID == null ? "History" + DateTime.Now.ToString("yyyyMMdd") : AppContext.Current.User.EID;
                string targetFile = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[custPathName].ToString() + "Feedback Detail_" + strUserId + ".xlsx");
                if (Directory.Exists(targetFoler) == false)
                {
                    Directory.CreateDirectory(targetFoler);
                }
                try
                {
                    int total = 0;
                    var detail = GetDetails();
                    WriteFeedbackDetailToExcel(tempFile, targetFile, detail.ToList());
                }
                catch (Exception ex)
                {
                    Helper.Log.Error(ex.Message, ex);
                    throw new OTCServiceException("Export 异常!");
                }

                HttpRequest request = HttpContext.Current.Request;
                StringBuilder appUriBuilder = new StringBuilder(request.Url.Scheme);
                appUriBuilder.Append(Uri.SchemeDelimiter);
                appUriBuilder.Append(request.Url.Authority);
                if (String.Compare(request.ApplicationPath, @"/") != 0)
                {
                    appUriBuilder.Append(request.ApplicationPath);
                }

                string virPathName = appUriBuilder.ToString() + ConfigurationManager.AppSettings[custPathName].ToString().Trim('~') + "Feedback Detail_" + strUserId + ".xlsx";
                
                return virPathName;
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex.Message);
            }
            return "";
        }

        public string ExportDetailJob()
        {
            try
            {
                string custPathName = "OverdueportPath";
                string tempFile = "~/Template/ReportFeedbackDetailTemplate.xlsx".TrimStart('~').Replace("/", "\\").TrimStart('\\');
                tempFile = Path.Combine(HttpRuntime.AppDomainAppPath, tempFile);
                string targetFoler = ConfigurationManager.AppSettings[custPathName].ToString().TrimStart('~').Replace("/", "\\").TrimStart('\\');
                targetFoler = Path.Combine(HttpRuntime.AppDomainAppPath, targetFoler);
                string strUserId = "History" + DateTime.Now.ToString("yyyyMMdd");
                string targetFile = Path.Combine(HttpRuntime.AppDomainAppPath, targetFoler + "Feedback_Detail_" + strUserId + ".xlsx");
                
                if (Directory.Exists(targetFoler) == false)
                {
                    Directory.CreateDirectory(targetFoler);
                }
                try
                {
                    int total = 0;
                    var detail = GetDetails();
                    WriteFeedbackDetailToExcel(tempFile, targetFile, detail.ToList());
                }
                catch (Exception ex)
                {
                    Helper.Log.Error(ex.Message, ex);
                    throw new OTCServiceException("Export 异常!");
                }

                T_Feedback_Report_History fileHistory = new T_Feedback_Report_History();
                fileHistory.FileName = Path.GetFileName(targetFile);
                fileHistory.FilePath = targetFile;
                fileHistory.ReportDate = AppContext.Current.User.Now;
                CommonRep.Add(fileHistory);
                CommonRep.Commit();

                return targetFile;
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex.Message);
            }
            return "";
        }

        private void WriteFeedbackToExcel(string tempFile, string target, IList<ReportFeedbackSumItem> models, IList<ReportNotFeedbackItem> notFeedback, IList<ReportHasFeedbackItem> hasFeedback)
        {
            try
            {
                NpoiHelper helper = new NpoiHelper(tempFile);
                helper.Save(target, true);
                helper = new NpoiHelper(target);

                ICellStyle styleCell = helper.Book.CreateCellStyle();
                IFont font = helper.Book.CreateFont();
                font.FontName = "Arial";
                font.FontHeight = 9;
                styleCell.SetFont(font);
                styleCell.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                styleCell.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                styleCell.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;

                int rowNo = 1;
                ISheet sheet = helper.Book.GetSheetAt(0);
                foreach (var item in models)
                {
                    IRow row = sheet.CreateRow(rowNo);
                    //Region
                    ICell cell = row.CreateCell(0);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Region);

                    //TotalAmount
                    cell = row.CreateCell(1);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.TotalCount);

                    //FeedbackCount
                    cell = row.CreateCell(2);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.FeedbackCount);

                    //Rate
                    cell = row.CreateCell(3);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Rate + "%");

                    rowNo++;
                }

                rowNo = 1;
                ISheet sheet1 = helper.Book.GetSheetAt(1);
                foreach (var item in notFeedback)
                {
                    IRow row = sheet1.CreateRow(rowNo);
                    //Region
                    ICell cell = row.CreateCell(0);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Region);
                    //Collector
                    cell = row.CreateCell(1);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Collector);
                    //Organization
                    cell = row.CreateCell(2);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Organization);
                    //Customer Name
                    cell = row.CreateCell(3);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.CustomerName);
                    //Customer Num
                    cell = row.CreateCell(4);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.CustomerNum);
                    //SiteUseId
                    cell = row.CreateCell(5);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.SiteUseId);
                    //Class
                    cell = row.CreateCell(6);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Class);
                    //Invoice Number
                    cell = row.CreateCell(7);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.InvoiceNum);
                    //Invoice Date
                    cell = row.CreateCell(8);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.InvoiceDate);
                    //Due Date
                    cell = row.CreateCell(9);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.DueDate);
                    //FuncCurrCode
                    cell = row.CreateCell(10);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.FuncCurrCode);
                    //Currency
                    cell = row.CreateCell(11);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Currency);
                    //DAYS_LATE_SYS
                    cell = row.CreateCell(12);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.DaysLateSys);
                    //BALANCE_AMT
                    cell = row.CreateCell(13);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(Convert.ToDouble(item.BalanceAmt));
                    //AgingBucket
                    cell = row.CreateCell(14);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.AgingBucket);
                    //CreditTremDescription
                    cell = row.CreateCell(15);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.CreditTremDescription);
                    //Ebname
                    cell = row.CreateCell(16);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Ebname);
                    //LsrNameHist
                    cell = row.CreateCell(17);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.LsrNameHist);
                    //FsrNameHist
                    cell = row.CreateCell(18);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.FsrNameHist);
                    //Cmpinv
                    cell = row.CreateCell(19);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Cmpinv);
                    //SO_NUM
                    cell = row.CreateCell(20);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.SoNum);
                    //PO_MUM
                    cell = row.CreateCell(21);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.PoNum);
                    //PTP_DATE
                    cell = row.CreateCell(22);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.PtpDate);
                    //OverdueReason
                    cell = row.CreateCell(23);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.OverdueReason);
                    //COMMENTS
                    cell = row.CreateCell(24);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Comments);
                    //Status
                    cell = row.CreateCell(25);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Status);
                    //CloseDate
                    cell = row.CreateCell(26);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.CloseDate);
                    rowNo++;
                }

                rowNo = 1;
                ISheet sheet2 = helper.Book.GetSheetAt(2);
                foreach (var item in hasFeedback)
                {
                    IRow row = sheet2.CreateRow(rowNo);
                    //Region
                    ICell cell = row.CreateCell(0);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Region);
                    //Collector
                    cell = row.CreateCell(1);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Collector);
                    //Organization
                    cell = row.CreateCell(2);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Organization);
                    //Customer Name
                    cell = row.CreateCell(3);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.CustomerName);
                    //Customer Num
                    cell = row.CreateCell(4);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.CustomerNum);
                    //SiteUseId
                    cell = row.CreateCell(5);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.SiteUseId);
                    //Class
                    cell = row.CreateCell(6);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Class);
                    //Invoice Number
                    cell = row.CreateCell(7);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.InvoiceNum);
                    //Invoice Date
                    cell = row.CreateCell(8);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.InvoiceDate);
                    //Due Date
                    cell = row.CreateCell(9);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.DueDate);
                    //FuncCurrCode
                    cell = row.CreateCell(10);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.FuncCurrCode);
                    //Currency
                    cell = row.CreateCell(11);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Currency);
                    //DAYS_LATE_SYS
                    cell = row.CreateCell(12);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.DaysLateSys);
                    //BALANCE_AMT
                    cell = row.CreateCell(13);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(Convert.ToDouble(item.BalanceAmt));
                    //AgingBucket
                    cell = row.CreateCell(14);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.AgingBucket);
                    //CreditTremDescription
                    cell = row.CreateCell(15);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.CreditTremDescription);
                    //Ebname
                    cell = row.CreateCell(16);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Ebname);
                    //LsrNameHist
                    cell = row.CreateCell(17);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.LsrNameHist);
                    //FsrNameHist
                    cell = row.CreateCell(18);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.FsrNameHist);
                    //Cmpinv
                    cell = row.CreateCell(19);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Cmpinv);
                    //SO_NUM
                    cell = row.CreateCell(20);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.SoNum);
                    //PO_MUM
                    cell = row.CreateCell(21);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.PoNum);
                    //PTP_DATE
                    cell = row.CreateCell(22);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.PtpDate);
                    //OverdueReason
                    cell = row.CreateCell(23);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.OverdueReason);
                    //COMMENTS
                    cell = row.CreateCell(24);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Comments);
                    //Status
                    cell = row.CreateCell(25);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Status);
                    //CloseDate
                    cell = row.CreateCell(26);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.CloseDate);
                    rowNo++;
                }

                //设置sheet
                helper.Save(target, true);
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }


        private void WriteFeedbackDetailToExcel(string tempFile, string target, IList<ReportFeedbackDetailItem> details)
        {
            try
            {
                File.Copy(tempFile, target, true);
                using (ExcelPackage package = new ExcelPackage(new FileInfo(target)))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[1];

                    for (int rowDetailNo = 1; rowDetailNo <= details.Count(); rowDetailNo++)
                    {
                        var detail = details[rowDetailNo - 1];

                        //Region
                        worksheet.Cells[rowDetailNo + 1, 1].Value = detail.Region;

                        //region
                        worksheet.Cells[rowDetailNo + 1, 2].Value = detail.Collector;

                        //Organization
                        worksheet.Cells[rowDetailNo + 1, 3].Value = detail.Organization;

                        //CustomerName
                        worksheet.Cells[rowDetailNo + 1, 4].Value = detail.CustomerName;

                        //CreditTerm
                        worksheet.Cells[rowDetailNo + 1, 5].Value = detail.CreditTerm;

                        //CustomerNum
                        worksheet.Cells[rowDetailNo + 1, 6].Value = detail.CustomerNum;

                        //SiteUseId
                        worksheet.Cells[rowDetailNo + 1, 7].Value = detail.SiteUseId;

                        //Class
                        worksheet.Cells[rowDetailNo + 1, 8].Value = detail.Class;

                        //InvoiceNum
                        worksheet.Cells[rowDetailNo + 1, 9].Value = detail.InvoiceNum;

                        //InvoiceDate
                        worksheet.Cells[rowDetailNo + 1, 10].Value = detail.InvoiceDate;

                        //DueDate
                        worksheet.Cells[rowDetailNo + 1, 11].Value = detail.DueDate;

                        //FuncCurrCode
                        worksheet.Cells[rowDetailNo + 1, 12].Value = detail.FuncCurrCode;

                        //Currency
                        worksheet.Cells[rowDetailNo + 1, 13].Value = detail.Currency;

                        //Due Days
                        worksheet.Cells[rowDetailNo + 1, 14].Value = detail.DueDays;

                        //InvoiceAmount
                        worksheet.Cells[rowDetailNo + 1, 15].Value = detail.InvoiceAmount;

                        // Aging Bucket
                        worksheet.Cells[rowDetailNo + 1, 16].Value = detail.AgingBucket;

                        //Payment Term Desc
                        worksheet.Cells[rowDetailNo + 1, 17].Value = detail.CreditTremDesc;

                        //EbName
                        worksheet.Cells[rowDetailNo + 1, 18].Value = detail.EbName;

                        //LsrNameHist
                        worksheet.Cells[rowDetailNo + 1, 19].Value = detail.CS;

                        //Sales
                        worksheet.Cells[rowDetailNo + 1, 20].Value = detail.Sales;

                        //LegalEntity
                        worksheet.Cells[rowDetailNo + 1, 21].Value = detail.LegalEntity;

                        //Cmpinv
                        worksheet.Cells[rowDetailNo + 1, 22].Value = detail.Cmpinv;

                        //SONum
                        worksheet.Cells[rowDetailNo + 1, 23].Value = detail.SONum;

                        //PONum
                        worksheet.Cells[rowDetailNo + 1, 24].Value = detail.PONum;

                        //PtpDate
                        worksheet.Cells[rowDetailNo + 1, 25].Value = detail.PtpDate;

                        //OverdueReason
                        worksheet.Cells[rowDetailNo + 1, 26].Value = detail.OverdueReason;

                        //Comments
                        worksheet.Cells[rowDetailNo + 1, 27].Value = detail.Comments;

                        //Comments ExpirationDate
                        worksheet.Cells[rowDetailNo + 1, 28].Value = detail.MemoExpirationDate == null ? "" : Convert.ToDateTime(detail.MemoExpirationDate).ToString("yyyy-MM-dd");

                        //Status
                        worksheet.Cells[rowDetailNo + 1, 29].Value = detail.Status;

                        //CloseDate
                        worksheet.Cells[rowDetailNo + 1, 30].Value = detail.CloseDate;

                        //feedback
                        worksheet.Cells[rowDetailNo + 1, 31].Value = detail.feedback;

                        //SendDate
                        worksheet.Cells[rowDetailNo + 1, 32].Value = detail.SendDate;

                    }
                    // 保存文件
                    package.Save();
                    package.Dispose();
                }

            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        public List<ReportFeedbackSumItemByCs> GetSumByCs() {

            IEnumerable<ReportFeedbackSumItemByCs> result = null;

            try
            {
                string sql = @"SELECT [Region]
                                  ,[Cs]
                                  ,[Branchmanager]
                                  ,[Currency]
                                  ,[BALANCE_AMT] as BalanceAmt
                              FROM [dbo].[V_Report_FeedbackbyCS_Sum]";
                object[] parameters = new object[0];
                result = CommonRep.ExecuteSqlQuery<ReportFeedbackSumItemByCs>(sql, parameters).OrderByDescending(o => o.BalanceAmt).ToList();
            }
            catch (Exception ex)
            {
                result = new List<ReportFeedbackSumItemByCs>();
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("V_Report_FeedbackbyCS_Sum 异常!");
            }

            return result.ToList();
        }

        public List<ReportFeedbackDetailItemByCs> GetDetailsByCs(int page, int pageSize, out int total)
        {
            IEnumerable<ReportFeedbackDetailItemByCs> result = null;

            try
            {
                string sql = @"SELECT Region            as Region,
                                      CS                as Cs,
                                      Ebname            as EbName,
                                      CREDIT_TREM       as CreditTerm, 
                                      CUSTOMER_NAME     as CustomerName, 
                                      CUSTOMER_NUM      as CustomerNum,
                                      SiteUseId         as SiteUseId,
                                      INVOICE_NUM       as InvoiceNum,
                                      CONVERT(VARCHAR(10),INVOICE_DATE,120)      as InvoiceDate,
                                      CONVERT(VARCHAR(10),DUE_DATE,120)         as DueDate,
                                      CURRENCY          as Currency, 
                                      BALANCE_AMT       as BalanceAmount, 
                                      CONVERT(VARCHAR(10),PTP_DATE,120)         as PtpDate,
                                      OverdueReason     as OverDueReason, 
                                      COMMENTS          as Comments
                              FROM [dbo].[V_Report_FeedbackbyCS_detail]";

                object[] parameters = new object[0];
                result = CommonRep.ExecuteSqlQuery<ReportFeedbackDetailItemByCs>(sql, parameters).OrderBy(o => o.Region).ThenBy(o => o.Cs);
                total = result.Count();
                result = result.Skip((page - 1) * pageSize).Take(pageSize);
            }
            catch (Exception ex)
            {
                result = new List<ReportFeedbackDetailItemByCs>();
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("V_Report_FeedbackbyCS_detail 异常!");
            }

            return result.ToList();
        }

        public IQueryable<ReportFeedbackDetailItemByCs> GetDetailsByCs()
        {
            IQueryable<ReportFeedbackDetailItemByCs> result = null;

            try
            {
                string sql = @"SELECT Region            as Region,
                                      CS                as Cs,
                                      Ebname            as EbName,
                                      CREDIT_TREM       as CreditTerm, 
                                      CUSTOMER_NAME     as CustomerName, 
                                      CUSTOMER_NUM      as CustomerNum,
                                      SiteUseId         as SiteUseId,
                                      INVOICE_NUM       as InvoiceNum,
                                      CONVERT(VARCHAR(10),INVOICE_DATE,120)      as InvoiceDate,
                                      CONVERT(VARCHAR(10),DUE_DATE,120)         as DueDate,
                                      CURRENCY          as Currency, 
                                      BALANCE_AMT       as BalanceAmount, 
                                      CONVERT(VARCHAR(10),PTP_DATE,120)         as PtpDate,
                                      OverdueReason     as OverDueReason, 
                                      COMMENTS          as Comments
                              FROM [dbo].[V_Report_FeedbackbyCS_detail]";

                object[] parameters = new object[0];
                result = CommonRep.ExecuteSqlQuery<ReportFeedbackDetailItemByCs>(sql, parameters).OrderBy(o => o.Region).ThenBy(o => o.Cs).AsQueryable();
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("V_Report_FeedbackbyCS_detail 异常!");
            }

            return result;
        }

        public string ExportByCs()
        {
            string custPathName = "OverdueportPath";
            string tempFile = HttpContext.Current.Server.MapPath("~/Template/ReportFeedbackByCsTemplate.xlsx");
            string targetFoler = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[custPathName].ToString());
            string targetFile = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[custPathName].ToString() + "NotFeedback_ByCs.xlsx");
            if (Directory.Exists(targetFoler) == false)
            {
                Directory.CreateDirectory(targetFoler);
            }

            try
            {
                var statistics = GetSumByCs();
                var details = GetDetailsByCs();
                WriteFeedbackByCsToExcel(tempFile, targetFile, statistics, details.ToList());
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Export 异常!");
            }

            HttpRequest request = HttpContext.Current.Request;
            StringBuilder appUriBuilder = new StringBuilder(request.Url.Scheme);
            appUriBuilder.Append(Uri.SchemeDelimiter);
            appUriBuilder.Append(request.Url.Authority);
            if (String.Compare(request.ApplicationPath, @"/") != 0)
            {
                appUriBuilder.Append(request.ApplicationPath);
            }

            string virPathName = appUriBuilder.ToString() + ConfigurationManager.AppSettings[custPathName].ToString().Trim('~') + "NotFeedback_ByCs.xlsx";
            return virPathName;
        }

        private void WriteFeedbackByCsToExcel(string tempFile, string target, IList<ReportFeedbackSumItemByCs> models, IList<ReportFeedbackDetailItemByCs> details)
        {
            try
            {
                NpoiHelper helper = new NpoiHelper(tempFile);
                helper.Save(target, true);
                helper = new NpoiHelper(target);

                ICellStyle styleCell = helper.Book.CreateCellStyle();
                IFont font = helper.Book.CreateFont();
                font.FontName = "Arial";
                font.FontHeight = 9;
                styleCell.SetFont(font);
                styleCell.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                styleCell.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                styleCell.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;

                int rowNo = 1;
                ISheet sheet = helper.Book.GetSheetAt(0);
                foreach (var item in models)
                {
                    IRow row = sheet.CreateRow(rowNo);
                    //Region
                    ICell cell = row.CreateCell(0);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Region);

                    //Cs
                    cell = row.CreateCell(1);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Cs);

                    //Currency
                    cell = row.CreateCell(2);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Currency);

                    //BalanceAmt
                    cell = row.CreateCell(3);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(Convert.ToDouble(item.BalanceAmt));

                    //Branchmanager
                    cell = row.CreateCell(4);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Branchmanager);

                    rowNo++;
                }

                ISheet sheetDetail = helper.Book.GetSheetAt(1);
                for (int rowDetailNo = 1; rowDetailNo <= details.Count(); rowDetailNo++)
                {
                    var detail = details[rowDetailNo - 1];

                    IRow row = sheetDetail.CreateRow(rowDetailNo);

                    //Region
                    ICell cell = row.CreateCell(0);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.Region);

                    //Cs
                    cell = row.CreateCell(1);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.Cs);

                    //EbName
                    cell = row.CreateCell(2);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.EbName);

                    //CreditTerm
                    cell = row.CreateCell(3);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.CreditTerm);

                    //CustomerName
                    cell = row.CreateCell(4);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.CustomerName);

                    //CustomerNum
                    cell = row.CreateCell(5);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.CustomerNum);

                    //SiteUseId
                    cell = row.CreateCell(6);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.SiteUseId);

                    //InvoiceNum
                    cell = row.CreateCell(7);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.InvoiceNum);

                    //InvoiceDate
                    cell = row.CreateCell(8);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.InvoiceDate);

                    //DueDate
                    cell = row.CreateCell(9);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.DueDate);

                    //CurrCode
                    cell = row.CreateCell(10);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.Currency);

                    //BalanceAmount
                    cell = row.CreateCell(11);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue((double)detail.BalanceAmount);

                    //PTP DATE
                    cell = row.CreateCell(12);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.PtpDate);
                    
                    //OverdueReason
                    cell = row.CreateCell(13);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.OverDueReason);

                    //Comments
                    cell = row.CreateCell(14);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.Comments);

                }

                //设置sheet
                helper.Save(target, true);
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }
        
        public List<ReportFeedbackSumItemBySales> GetSumBySales()
        {

            IEnumerable<ReportFeedbackSumItemBySales> result = null;

            try
            {
                string sql = @"SELECT [Region]
                                  ,[Sales]
                                  ,[Branchmanager]
                                  ,[Currency]
                                  ,[BALANCE_AMT] as BalanceAmt
                              FROM [dbo].[V_Report_FeedbackBySales_Sum]";
                object[] parameters = new object[0];
                result = CommonRep.ExecuteSqlQuery<ReportFeedbackSumItemBySales>(sql, parameters).OrderByDescending(o => o.BalanceAmt).ToList();
            }
            catch (Exception ex)
            {
                result = new List<ReportFeedbackSumItemBySales>();
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("V_Report_FeedbackBySales_Sum 异常!");
            }

            return result.ToList();
        }

        public List<ReportFeedbackDetailItemBySales> GetDetailsBySales(int page, int pageSize, out int total)
        {
            IEnumerable<ReportFeedbackDetailItemBySales> result = null;

            try
            {
                string sql = @"SELECT Region            as Region,
                                      Sales             as Sales,
                                      Ebname            as EbName,
                                      CREDIT_TREM       as CreditTerm, 
                                      CUSTOMER_NAME     as CustomerName, 
                                      CUSTOMER_NUM      as CustomerNum,
                                      SiteUseId         as SiteUseId,
                                      INVOICE_NUM       as InvoiceNum,
                                      CONVERT(VARCHAR(10),INVOICE_DATE,120)      as InvoiceDate,
                                      CONVERT(VARCHAR(10),DUE_DATE,120)         as DueDate,
                                      CURRENCY          as Currency, 
                                      BALANCE_AMT       as BalanceAmount, 
                                      CONVERT(VARCHAR(10),PTP_DATE,120)         as PtpDate,
                                      OverdueReason     as OverDueReason, 
                                      COMMENTS          as Comments
                              FROM [dbo].[V_Report_FeedbackBySales_detail]";

                object[] parameters = new object[0];
                result = CommonRep.ExecuteSqlQuery<ReportFeedbackDetailItemBySales>(sql, parameters).OrderBy(o => o.Region).ThenBy(o => o.Sales);
                total = result.Count();
                result = result.Skip((page - 1) * pageSize).Take(pageSize);
            }
            catch (Exception ex)
            {
                result = new List<ReportFeedbackDetailItemBySales>();
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("V_Report_FeedbackBySales_detail 异常!");
            }

            return result.ToList();
        }

        public IQueryable<ReportFeedbackDetailItemBySales> GetDetailsBySales()
        {
            IQueryable<ReportFeedbackDetailItemBySales> result = null;

            try
            {
                string sql = @"SELECT Region            as Region,
                                      Sales             as Sales,
                                      Ebname            as EbName,
                                      CREDIT_TREM       as CreditTerm, 
                                      CUSTOMER_NAME     as CustomerName, 
                                      CUSTOMER_NUM      as CustomerNum,
                                      SiteUseId         as SiteUseId,
                                      INVOICE_NUM       as InvoiceNum,
                                      CONVERT(VARCHAR(10),INVOICE_DATE,120)      as InvoiceDate,
                                      CONVERT(VARCHAR(10),DUE_DATE,120)         as DueDate,
                                      CURRENCY          as Currency, 
                                      BALANCE_AMT       as BalanceAmount, 
                                      CONVERT(VARCHAR(10),PTP_DATE,120)         as PtpDate,
                                      OverdueReason     as OverDueReason, 
                                      COMMENTS          as Comments
                              FROM [dbo].[V_Report_FeedbackBySales_detail]";

                object[] parameters = new object[0];
                result = CommonRep.ExecuteSqlQuery<ReportFeedbackDetailItemBySales>(sql, parameters).OrderBy(o => o.Region).ThenBy(o => o.Sales).AsQueryable();
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("V_Report_FeedbackBySales_detail 异常!");
            }

            return result;
        }

        public string ExportBySales()
        {
            string custPathName = "OverdueportPath";
            string tempFile = HttpContext.Current.Server.MapPath("~/Template/ReportFeedbackBySalesTemplate.xlsx");
            string targetFoler = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[custPathName].ToString());
            string targetFile = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[custPathName].ToString() + "NotFeedback_BySales.xlsx");
            if (Directory.Exists(targetFoler) == false)
            {
                Directory.CreateDirectory(targetFoler);
            }

            try
            {
                var statistics = GetSumBySales();
                var details = GetDetailsBySales();
                WriteFeedbackBySalesToExcel(tempFile, targetFile, statistics, details.ToList());
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Export 异常!");
            }

            HttpRequest request = HttpContext.Current.Request;
            StringBuilder appUriBuilder = new StringBuilder(request.Url.Scheme);
            appUriBuilder.Append(Uri.SchemeDelimiter);
            appUriBuilder.Append(request.Url.Authority);
            if (String.Compare(request.ApplicationPath, @"/") != 0)
            {
                appUriBuilder.Append(request.ApplicationPath);
            }

            string virPathName = appUriBuilder.ToString() + ConfigurationManager.AppSettings[custPathName].ToString().Trim('~') + "NotFeedback_BySales.xlsx";
            return virPathName;
        }

        private void WriteFeedbackBySalesToExcel(string tempFile, string target, IList<ReportFeedbackSumItemBySales> models, IList<ReportFeedbackDetailItemBySales> details)
        {
            try
            {
                NpoiHelper helper = new NpoiHelper(tempFile);
                helper.Save(target, true);
                helper = new NpoiHelper(target);

                ICellStyle styleCell = helper.Book.CreateCellStyle();
                IFont font = helper.Book.CreateFont();
                font.FontName = "Arial";
                font.FontHeight = 9;
                styleCell.SetFont(font);
                styleCell.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                styleCell.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                styleCell.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;

                int rowNo = 1;
                ISheet sheet = helper.Book.GetSheetAt(0);
                foreach (var item in models)
                {
                    IRow row = sheet.CreateRow(rowNo);
                    //Region
                    ICell cell = row.CreateCell(0);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Region);

                    //Cs
                    cell = row.CreateCell(1);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Sales);

                    //Currency
                    cell = row.CreateCell(2);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Currency);

                    //BalanceAmt
                    cell = row.CreateCell(3);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(Convert.ToDouble(item.BalanceAmt));

                    //Currency
                    cell = row.CreateCell(4);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(item.Branchmanager);

                    rowNo++;
                }

                ISheet sheetDetail = helper.Book.GetSheetAt(1);
                for (int rowDetailNo = 1; rowDetailNo <= details.Count(); rowDetailNo++)
                {
                    var detail = details[rowDetailNo - 1];

                    IRow row = sheetDetail.CreateRow(rowDetailNo);

                    //Region
                    ICell cell = row.CreateCell(0);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.Region);

                    //Cs
                    cell = row.CreateCell(1);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.Sales);

                    //EbName
                    cell = row.CreateCell(2);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.EbName);

                    //CreditTerm
                    cell = row.CreateCell(3);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.CreditTerm);

                    //CustomerName
                    cell = row.CreateCell(4);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.CustomerName);

                    //CustomerNum
                    cell = row.CreateCell(5);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.CustomerNum);

                    //SiteUseId
                    cell = row.CreateCell(6);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.SiteUseId);

                    //InvoiceNum
                    cell = row.CreateCell(7);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.InvoiceNum);

                    //InvoiceDate
                    cell = row.CreateCell(8);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.InvoiceDate);

                    //DueDate
                    cell = row.CreateCell(9);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.DueDate);

                    //CurrCode
                    cell = row.CreateCell(10);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.Currency);

                    //BalanceAmount
                    cell = row.CreateCell(11);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue((double)detail.BalanceAmount);

                    //PTP DATE
                    cell = row.CreateCell(12);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.PtpDate);

                    //OverdueReason
                    cell = row.CreateCell(13);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.OverDueReason);

                    //Comments
                    cell = row.CreateCell(14);
                    cell.CellStyle = styleCell;
                    cell.SetCellValue(detail.Comments);

                }

                //设置sheet
                helper.Save(target, true);
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }


    }
}
