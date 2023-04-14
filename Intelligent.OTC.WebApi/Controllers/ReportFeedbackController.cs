using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Domain.Dtos;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "reportfeedback")]
    public class ReportFeedbackController : ApiController
    {
        [HttpGet]
        [Route("api/reportfeedback/statistics")]
        public List<ReportFeedbackSumItem> Statistics()
        {
            ReportFeedbackService service = SpringFactory.GetObjectImpl<ReportFeedbackService>("ReportFeedbackService");
            return service.GetSum();
        }

        [HttpGet]
        [Route("api/reportfeedback/notfeedback")]
        public PageResultDto<ReportNotFeedbackItem> NotFeedback(int page, int pageSize)
        {
            var resultDto = new PageResultDto<ReportNotFeedbackItem>();
            ReportFeedbackService service = SpringFactory.GetObjectImpl<ReportFeedbackService>("ReportFeedbackService");

            int total = 0;
            resultDto.dataRows = service.GetNotFeedbackList(page, pageSize, out total);
            resultDto.count = total;

            return resultDto;
        }

        [HttpGet]
        [Route("api/reportfeedback/hasfeedback")]
        public PageResultDto<ReportHasFeedbackItem> HasFeedback(int page, int pageSize)
        {
            var resultDto = new PageResultDto<ReportHasFeedbackItem>();
            ReportFeedbackService service = SpringFactory.GetObjectImpl<ReportFeedbackService>("ReportFeedbackService");

            int total = 0;
            resultDto.dataRows = service.GetHasFeedbackList(page, pageSize, out total);
            resultDto.count = total;

            return resultDto;
        }

        [HttpGet]
        [Route("api/reportfeedback/getfeedbackhistory")]
        public PageResultDto<FeedbackHistoryDto> getfeedbackhistory(int page, int pageSize)
        {
            var resultDto = new PageResultDto<FeedbackHistoryDto>();
            ReportFeedbackService service = SpringFactory.GetObjectImpl<ReportFeedbackService>("ReportFeedbackService");

            int total = 0;
            resultDto.dataRows = service.getfeedbackhistory(page, pageSize, out total);
            foreach (FeedbackHistoryDto item in resultDto.dataRows) {
                item.filepath = urlconvertor(item.filepath);
            }
            resultDto.count = total;

            return resultDto;
        }

        private string urlconvertor(string imagesurl1)
        {
            string tmpRootDir = HttpContext.Current.Server.MapPath(HttpContext.Current.Request.ApplicationPath.ToString()).Replace(ConfigurationManager.AppSettings["OTCSUB"].ToString(), "");//获取程序根目录
            string imagesurl2 = imagesurl1.Replace(tmpRootDir, ""); //转换成相对路径
            imagesurl2 = imagesurl2.Replace(@"\", @"/");
            return imagesurl2;
        }

        [HttpGet]
        [Route("api/reportfeedback/details")]
        public PageResultDto<ReportFeedbackDetailItem> Details(string sDate, int page, int pageSize)
        {
            var resultDto = new PageResultDto<ReportFeedbackDetailItem>();
            ReportFeedbackService service = SpringFactory.GetObjectImpl<ReportFeedbackService>("ReportFeedbackService");

            int total = 0;
            resultDto.dataRows = service.GetDetails(page, pageSize, out total);
            resultDto.count = total;

            return resultDto;
        }

        [HttpGet]
        [Route("api/reportfeedback/download")]
        public string DownloadReport()
        {
            ReportFeedbackService service = SpringFactory.GetObjectImpl<ReportFeedbackService>("ReportFeedbackService");
            return service.Export();
        }

        [HttpGet]
        [Route("api/reportfeedback/detaildownload")]
        public string DownloadDetail(string sDate)
        {
            ReportFeedbackService service = SpringFactory.GetObjectImpl<ReportFeedbackService>("ReportFeedbackService");
            return service.ExportDetail();
        }
    }
}