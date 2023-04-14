using Intelligent.OTC.Business.Collection;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Domain.DomainModel;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "overduereport")]
    public class OverdueReportController : ApiController
    {
        [HttpGet]
        [Route("api/overdue/query")]
        public ReportModel QueryReport(int pageindex, int pagesize, string filter)
        {
            CollectionService service = SpringFactory.GetObjectImpl<CollectionService>("CollectionService");
            return service.QueryOverdueReport(pageindex, pagesize, filter);
        }

        [HttpGet]
        [Route("api/overdue/download")]
        public string DownloadReport(string filter)
        {
            CollectionService service = SpringFactory.GetObjectImpl<CollectionService>("CollectionService");
            return service.ExportOverdueReport(filter);
        }
    }
}