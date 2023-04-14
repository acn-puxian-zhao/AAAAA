using Intelligent.OTC.Business.Collection;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Domain.DomainModel;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "disputereport")]
    public class DisputeReportController : ApiController
    {
        [HttpGet]
        [Route("api/dispute/query")]
        public ReportModel QueryReport(int pageindex, int pagesize, string filter)
        {
            CollectionService service = SpringFactory.GetObjectImpl<CollectionService>("CollectionService");
            return service.QueryDisputeReport(pageindex, pagesize, filter);
        }

        [HttpGet]
        [Route("api/dispute/download")]
        public string DownloadReport(string filter)
        {
            CollectionService service = SpringFactory.GetObjectImpl<CollectionService>("CollectionService");
            return service.ExportDisputeReport(filter);
        }
    }
}