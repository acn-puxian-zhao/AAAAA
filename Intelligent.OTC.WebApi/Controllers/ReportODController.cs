using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Domain.Dtos;
using System.Collections.Generic;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "reportod")]
    public class ReportODController : ApiController
    {
        [HttpGet]
        [Route("api/reportod/statistics")]
        public List<ReportODSumItem> Statistics()
        {
            ReportODService service = SpringFactory.GetObjectImpl<ReportODService>("ReportODService");
            return service.GetSum();
        }

        [HttpGet]
        [Route("api/reportod/details")]
        public PageResultDto<ReportODDetailItem> Details(int page, int pageSize)
        {
            PageResultDto<ReportODDetailItem> resultDto = new PageResultDto<ReportODDetailItem>(); 
            ReportODService service = SpringFactory.GetObjectImpl<ReportODService>("ReportODService");

            int total = 0;
            resultDto.dataRows = service.GetDetails(page, pageSize, out total);
            resultDto.count = total;

            return resultDto;
        }

        [HttpGet]
        [Route("api/reportod/download")]
        public string DownloadReport()
        {
            ReportODService service = SpringFactory.GetObjectImpl<ReportODService>("ReportODService");
            return service.Export();
        }
    }
}