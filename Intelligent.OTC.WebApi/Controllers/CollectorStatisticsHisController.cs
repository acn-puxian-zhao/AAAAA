using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Domain.DataModel;
using System;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    public class CollectorStatisticsHisController : ApiController
    {
        [HttpPost]
        [Route("api/collectorStatisticsHis/GetCollectorStatisticsHis")]
        public CollectorStatisticsGraphDto GetCollectorStatisticsHis(DateTime start, DateTime end, string type, string collector)
        {
            CollectorStatisticsHisService service = SpringFactory.GetObjectImpl<CollectorStatisticsHisService>("CollectorStatisticsHisService");
            return service.GetCustomerContactCount(start, end, type, collector);
        }
    }
}