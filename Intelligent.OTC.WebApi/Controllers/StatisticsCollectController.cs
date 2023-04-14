using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using System;
using System.Linq;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    public class StatisticsCollectController : ApiController
    {

        [HttpPost]
        [Route("api/statisticsCollect/GetStatisticsCollect")]
        public StatisticsCollectGridDto GetStatisticsCollect(string region,int pageindex,int pagesize)
        {
            
            StatisticsCollectService service = SpringFactory.GetObjectImpl<StatisticsCollectService>("StatisticsCollectService");
            var result = service.GetStatisticsCollect(region);
            var list = result.OrderByDescending(p => p.CustomerNum).Skip((pageindex - 1) * pagesize).Take(pagesize).Distinct().ToList();
            StatisticsCollectGridDto grid = new StatisticsCollectGridDto();
            grid.result = list;
            grid.count = result.Count();
            return grid;
        }

        [HttpPost]
        [Route("api/statisticsCollect/GetStatisticsCollector")]
        public StatisticsCollectorGridDto GetStatisticsCollector(int pageindex, int pagesize)
        {

            StatisticsCollectService service = SpringFactory.GetObjectImpl<StatisticsCollectService>("StatisticsCollectService");
            var result = service.GetStatisticsCollector();
            var list = result.OrderBy(p => p.Id).Skip((pageindex - 1) * pagesize).Take(pagesize).Distinct().ToList();

            if (list.Count > 0)
            {
                //计算合计行
                V_STATISTICS_COLLECTOR sumRow = new V_STATISTICS_COLLECTOR();
                decimal decAR = 0, decPTPAR = 0, decPTPBROKENAR = 0, decOVERDUEAR = 0, decNOTREFUSEREPLY = 0;
                foreach (V_STATISTICS_COLLECTOR item in list)
                {
                    decAR += Convert.ToDecimal(item.AR == null ? 0 : item.AR);
                    decPTPAR += Convert.ToDecimal(item.PTPAR == null ? 0 : item.PTPAR);
                    decPTPBROKENAR += Convert.ToDecimal(item.PTPBROKENAR == null ? 0 : item.PTPBROKENAR);
                    decOVERDUEAR += Convert.ToDecimal(item.OVERDUEAR == null ? 0 : item.OVERDUEAR);
                    decNOTREFUSEREPLY += Convert.ToDecimal(item.NOTREFUSEREPLY == null ? 0 : item.NOTREFUSEREPLY);
                }
                sumRow.COLLECTOR = "---Summary---";
                sumRow.AR = decAR;
                sumRow.PTPAR = decPTPAR;
                sumRow.PTPBROKENAR = decPTPBROKENAR;
                sumRow.OVERDUEAR = decOVERDUEAR;
                sumRow.NOTREFUSEREPLY = decNOTREFUSEREPLY;
                sumRow.PTPAR_PER = sumRow.AR == 0 ? 0 : sumRow.PTPAR / sumRow.AR * 100;
                sumRow.PTPBROKEN_PER = sumRow.PTPAR == 0 ? 0 : sumRow.PTPBROKENAR / sumRow.PTPAR * 100;
                sumRow.OVERDUEAR_PER = sumRow.AR == 0 ? 0 : sumRow.OVERDUEAR / sumRow.AR * 100;
                sumRow.NOTREFUSEREPLY_PER = sumRow.AR == 0 ? 0 : sumRow.NOTREFUSEREPLY / sumRow.AR * 100;
                list.Add(sumRow);
            }

            StatisticsCollectorGridDto grid = new StatisticsCollectorGridDto();
            grid.result = list;
            grid.count = result.Count();
            return grid;
        }

        [HttpPost]
        [Route("api/statisticsCollect/GetStatisticsCollectSum")]
        public StatisticsCollectSumDto GetStatisticsCollectSum()
        {
            StatisticsCollectService service = SpringFactory.GetObjectImpl<StatisticsCollectService>("StatisticsCollectService");
            return service.GetStatisticsCollectSum();
        }

        [HttpGet]
        [Route("api/statisticsCollect/downloadCustomer")]
        public string downloadCustomer(string region)
        {
            StatisticsCollectService service = SpringFactory.GetObjectImpl<StatisticsCollectService>("StatisticsCollectService");
            return service.createCustomerstatisticReport(region);
        }

        [HttpGet]
        [Route("api/statisticsCollect/downloadCollector")]
        public string downloadCollector()
        {
            StatisticsCollectService service = SpringFactory.GetObjectImpl<StatisticsCollectService>("StatisticsCollectService");
            return service.createCollectorstatisticReport();
        }

    }
}