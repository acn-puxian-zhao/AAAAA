using Intelligent.OTC.Business.Collection;
using Intelligent.OTC.Common;
using Intelligent.OTC.Domain.DomainModel;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    public class DailyAgingController : ApiController
    {
        [HttpPost]
        [Route("api/dailyAging/query")]
        public List<DailyAgingDto> QueryCustomerAging(int pageindex, int pagesize, string filter,string legalEntity,string custName,
            string custNum, string siteUseId)
        {
            CollectionService service = SpringFactory.GetObjectImpl<CollectionService>("CollectionService");
            var result = service.GetQueryAging(legalEntity, custNum, custName,siteUseId);
            var list = result.OrderByDescending(p => p.ID).Skip((pageindex - 1) * pagesize).Take(pagesize).ToList();
            if (list != null && list.Count > 0)
            {
                list[0].count = result.Count();
            }
            return list;
        }

        [HttpGet]
        [Route("api/dailyAging/download")]
        public string DownloadDailyAging(string filter, string legalEntity,  string custName, string custNum, string siteUseId)
        {
            CollectionService service = SpringFactory.GetObjectImpl<CollectionService>("CollectionService");
            return service.ExportDailyAgingReport(legalEntity, custNum, custName, siteUseId);
        }

        [HttpGet]
        [Route("api/dailyAging/downloadnew")]
        public string DownloadDailyAgingNew(string filter, string legalEntity, string custName, string custNum, string siteUseId)
        {
            CollectionService service = SpringFactory.GetObjectImpl<CollectionService>("CollectionService");
            return service.ExportDailyAgingReportNew(legalEntity, custNum, custName, siteUseId);
        }
    }
}
