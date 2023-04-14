using Intelligent.OTC.Business.Collection;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.Domain.Repositories;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Intelligent.OTC.Business
{
    public class StatisticsCollectService : IStatisticsCollectService
    {
        public OTCRepository CommonRep { get; set; }

        /// <summary>
        /// get all CustomerAgingStaging Data from Db
        /// </summary>
        /// <returns></returns>
        public IQueryable<StatisticsCollectDto> GetStatisticsCollect(string region)
        {
            var query = from ar in CommonRep.GetQueryable<V_STATISTICS_COLLECT>()
                        select new StatisticsCollectDto
                        {
                            CustomerNum = ar.CUSTOMER_NUM,
                            CustomerName = ar.CUSTOMER_NAME,
                            SiteUseId = ar.SiteUseId,
                            openAR = Math.Round(ar.openAR == null ? 0 : (decimal)ar.openAR, 2),
                            overDure = Math.Round(ar.overDure == null ? 0 : (decimal)ar.overDure, 2),
                            dispute = Math.Round(ar.dispute == null ? 0 : (decimal)ar.dispute, 2),
                            Region = ar.Region,
                            Collector = ar.COLLECTOR
                        };

            if (region != "all")
            {
                query = query.Where(p => p.Region == region);
            }

            return query;
        }

        public string createCustomerstatisticReport(string region)
        {

            string templateName = "CustomerStatisticReportTemplate";
            string outputPath = "AgingReportPath";
            var tplName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[templateName].ToString());
            var fileName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[outputPath].ToString());
            var pathName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[outputPath].ToString() + "CustomerStatisticExport." + AppContext.Current.User.EID + ".xlsx");

            if (Directory.Exists(fileName) == false)
            {
                Directory.CreateDirectory(fileName);
            }
            IQueryable<StatisticsCollectDto> query = GetStatisticsCollect(region);
            if (query.Count() > 50000)
            {
                throw new OTCServiceException("数据量太大(>50000)，请按条件分批导出！");
            }
            WriteCustomerStatisticDataToExcel(tplName, pathName, query);

            HttpRequest request = HttpContext.Current.Request;
            StringBuilder appUriBuilder = new StringBuilder(request.Url.Scheme);
            appUriBuilder.Append(Uri.SchemeDelimiter);
            appUriBuilder.Append(request.Url.Authority);
            if (String.Compare(request.ApplicationPath, @"/") != 0)
            {
                appUriBuilder.Append(request.ApplicationPath);
            }
            var virPatnName = appUriBuilder.ToString() + ConfigurationManager.AppSettings[outputPath].ToString().Trim('~') + "CustomerStatisticExport." + AppContext.Current.User.EID + ".xlsx";
            return virPatnName;
        }

        public string createCollectorstatisticReport()
        {

            string templateName = "CollectorStatisticReportTemplate";
            string outputPath = "AgingReportPath";
            var tplName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[templateName].ToString());
            var fileName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[outputPath].ToString());
            var pathName = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[outputPath].ToString() + "CollectorStatisticExport." + AppContext.Current.User.EID + ".xlsx");

            if (Directory.Exists(fileName) == false)
            {
                Directory.CreateDirectory(fileName);
            }
            IQueryable<V_STATISTICS_COLLECTOR> query = GetStatisticsCollector();
            if (query.Count() > 50000)
            {
                throw new OTCServiceException("数据量太大(>50000)，请按条件分批导出！");
            }
            WriteCollectorStatisticDataToExcel(tplName, pathName, query);

            HttpRequest request = HttpContext.Current.Request;
            StringBuilder appUriBuilder = new StringBuilder(request.Url.Scheme);
            appUriBuilder.Append(Uri.SchemeDelimiter);
            appUriBuilder.Append(request.Url.Authority);
            if (String.Compare(request.ApplicationPath, @"/") != 0)
            {
                appUriBuilder.Append(request.ApplicationPath);
            }
            var virPatnName = appUriBuilder.ToString() + ConfigurationManager.AppSettings[outputPath].ToString().Trim('~') + "CollectorStatisticExport." + AppContext.Current.User.EID + ".xlsx";
            return virPatnName;
        }

        public IQueryable<V_STATISTICS_COLLECTOR> GetStatisticsCollector()
        {

            var query = from ar in CommonRep.GetQueryable<V_STATISTICS_COLLECTOR>()
                        select ar;

            return query;
        }

        /// <summary>
        /// get all CustomerAgingStaging Data from Db
        /// </summary>
        /// <returns></returns>
        public StatisticsCollectSumDto GetStatisticsCollectSum()
        {
            StatisticsCollectSumDto scsd = new StatisticsCollectSumDto();
            var openARSum = CommonRep.GetQueryable<V_STATISTICS_COLLECT>().Sum(p => p.openAR);
            var disputeSum = CommonRep.GetQueryable<V_STATISTICS_COLLECT>().Sum(p => p.dispute);
            var overDueSum = CommonRep.GetQueryable<V_STATISTICS_COLLECT>().Sum(p => p.overDure);
            var ptpSum = CommonRep.GetQueryable<V_STATISTICS_COLLECTOR>().Sum(p => p.PTPAR);
            scsd.openAR = openARSum;
            scsd.dispute = disputeSum;
            scsd.overDure = overDueSum;
            scsd.ptpAR = ptpSum;
            scsd.now = DateTime.Now;

            return scsd;
        }

        private void WriteCustomerStatisticDataToExcel(string temp, string path, IQueryable<StatisticsCollectDto> list)
        {
            try
            {
                ExportService export = new ExportService(temp);
                export.Save(path, true);
                export = new ExportService(path);
                var sheetName = export.Sheets[0];
                export.ActiveSheetName = sheetName;
                export.ExportCustomerStatisticDataList(list.ToList());
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        private void WriteCollectorStatisticDataToExcel(string temp, string path, IQueryable<V_STATISTICS_COLLECTOR> list)
        {
            try
            {
                ExportService export = new ExportService(temp);
                export.Save(path, true);
                export = new ExportService(path);
                var sheetName = export.Sheets[0];
                export.ActiveSheetName = sheetName;
                export.ExportCollectorStatisticDataList(list.ToList());
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        public void BackCollectorStatisticsJob()
        {
            try
            {
                CommonRep.GetDBContext().Database.ExecuteSqlCommand("spBackCollectorStatistics");
            }
            catch (Exception ex)
            {
                Helper.Log.Error("Start: call spBackCollectorStatistics", ex);
            }
        }
    }
}
