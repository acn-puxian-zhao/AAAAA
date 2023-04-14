using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Domain.Dtos;
using System.Collections.Generic;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "reportfeedbackBySales")]
    public class ReportFeedbackBySalesController : ApiController
    {
        [HttpGet]
        [Route("api/reportfeedbackBySales/statistics")]
        public List<ReportFeedbackSumItemBySales> Statistics()
        {
            ReportFeedbackService service = SpringFactory.GetObjectImpl<ReportFeedbackService>("ReportFeedbackService");
            return service.GetSumBySales();
        }

        [HttpGet]
        [Route("api/reportfeedbackBySales/details")]
        public PageResultDto<ReportFeedbackDetailItemBySales> Details(int page, int pageSize)
        {
            var resultDto = new PageResultDto<ReportFeedbackDetailItemBySales>();
            ReportFeedbackService service = SpringFactory.GetObjectImpl<ReportFeedbackService>("ReportFeedbackService");

            int total = 0;
            resultDto.dataRows = service.GetDetailsBySales(page, pageSize, out total);
            resultDto.count = total;

            return resultDto;
        }

        [HttpGet]
        [Route("api/reportfeedbackBySales/download")]
        public string DownloadReport()
        {
            ReportFeedbackService service = SpringFactory.GetObjectImpl<ReportFeedbackService>("ReportFeedbackService");
            return service.ExportBySales();
        }
    }
}