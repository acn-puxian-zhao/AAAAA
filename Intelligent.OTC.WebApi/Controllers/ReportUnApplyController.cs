using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Domain.Dtos;
using System.Collections.Generic;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "reportunapply")]
    public class ReportUnApplyController : ApiController
    {
        [HttpGet]
        [Route("api/reportunapply/statistics")]
        public List<ReportUnApplySumItem> Statistics()
        {
            ReportUnApplyService service = SpringFactory.GetObjectImpl<ReportUnApplyService>("ReportUnApplyService");
            return service.GetSum();
        }

        [HttpGet]
        [Route("api/reportunapply/details")]
        public PageResultDto<ReportUnApplyDetailItem> Details(int page, int pageSize)
        {
            PageResultDto<ReportUnApplyDetailItem> resultDto = new PageResultDto<ReportUnApplyDetailItem>();
            ReportUnApplyService service = SpringFactory.GetObjectImpl<ReportUnApplyService>("ReportUnApplyService");

            int total = 0;
            resultDto.dataRows = service.GetDetails(page, pageSize, out total);
            resultDto.count = total;

            return resultDto;
        }

        [HttpGet]
        [Route("api/reportunapply/download")]
        public string DownloadReport()
        {
            ReportUnApplyService service = SpringFactory.GetObjectImpl<ReportUnApplyService>("ReportUnApplyService");
            return service.Export();
        }
    }
}