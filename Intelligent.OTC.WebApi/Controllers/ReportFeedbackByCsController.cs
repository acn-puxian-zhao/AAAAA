using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Domain.Dtos;
using System.Collections.Generic;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "reportfeedbackbycs")]
    public class ReportFeedbackByCsController : ApiController
    {
        [HttpGet]
        [Route("api/reportfeedbackbycs/statistics")]
        public List<ReportFeedbackSumItemByCs> Statistics()
        {
            ReportFeedbackService service = SpringFactory.GetObjectImpl<ReportFeedbackService>("ReportFeedbackService");
            return service.GetSumByCs();
        }

        [HttpGet]
        [Route("api/reportfeedbackbycs/details")]
        public PageResultDto<ReportFeedbackDetailItemByCs> Details(int page, int pageSize)
        {
            var resultDto = new PageResultDto<ReportFeedbackDetailItemByCs>();
            ReportFeedbackService service = SpringFactory.GetObjectImpl<ReportFeedbackService>("ReportFeedbackService");

            int total = 0;
            resultDto.dataRows = service.GetDetailsByCs(page, pageSize, out total);
            resultDto.count = total;

            return resultDto;
        }

        [HttpGet]
        [Route("api/reportfeedbackbycs/download")]
        public string DownloadReport()
        {
            ReportFeedbackService service = SpringFactory.GetObjectImpl<ReportFeedbackService>("ReportFeedbackService");
            return service.ExportByCs();
        }
    }
}