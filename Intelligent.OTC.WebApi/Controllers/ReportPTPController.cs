using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Domain.Dtos;
using System.Collections.Generic;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "reportptp")]
    public class ReportPTPController : ApiController
    {
        [HttpGet]
        [Route("api/reportptp/statistics")]
        public List<ReportPTPSumItem> Statistics()
        {
            ReportPTPService service = SpringFactory.GetObjectImpl<ReportPTPService>("ReportPTPService");
            return service.GetSum();
        }

        [HttpGet]
        [Route("api/reportptp/details")]
        public PageResultDto<ReportPTPDetailItem> Details(int page, int pageSize, int category)
        {
            PageResultDto<ReportPTPDetailItem> resultDto = new PageResultDto<ReportPTPDetailItem>();
            ReportPTPService service = SpringFactory.GetObjectImpl<ReportPTPService>("ReportPTPService");

            int total = 0;
            resultDto.dataRows = service.GetDetails(page, pageSize, category, out total);
            resultDto.count = total;

            return resultDto;
        }

        [HttpGet]
        [Route("api/reportptp/download")]
        public string DownloadReport()
        {
            ReportPTPService service = SpringFactory.GetObjectImpl<ReportPTPService>("ReportPTPService");
            return service.Export();
        }
    }
}