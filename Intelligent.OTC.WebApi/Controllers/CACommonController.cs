using Intelligent.OTC.Business;
using Intelligent.OTC.Business.Interfaces;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    //[UserAuthorizeFilter(actionSet: "cacommon")]
    public class CACommonController : ApiController
    {
        [HttpGet]
        [Route("api/cacommon/getCARegionByCurrentUser")]
        public String getCARegionByCurrentUser()
        {
            CaCommonService service = SpringFactory.GetObjectImpl<CaCommonService>("CaCommonService");
            return service.getCARegionByCurrentUser();
        }

        [HttpGet]
        [Route("api/cacommon/getActionTaskList")]
        public CaActionTaskPage getActionTaskList(string transactionNumber, string status, string currency, string dateF, string dateT, int page, int pageSize)
        {
            CaCommonService service = SpringFactory.GetObjectImpl<CaCommonService>("CaCommonService");
            var res = service.getActionTaskList(transactionNumber, status, currency, dateF, dateT, page, pageSize);
            return res;
        }

        [HttpPost]
        [Route("api/cacommon/postAndClear")]
        public string postAndClear(string[] bsid) {
            CaCommonService service = SpringFactory.GetObjectImpl<CaCommonService>("CaCommonService");
            string[] bsidDo = new string[bsid.Length - 1];
            for (int i = 1; i < bsid.Length; i++) {
                bsidDo[i - 1] = bsid[i];
            }
            string fileid = service.postAndClear(bsid[0], bsidDo, "", AppContext.Current.User.EID);

            //发送任务结束提醒邮件
            IMailSendService mailService = SpringFactory.GetObjectImpl<IMailSendService>("MailSendService");
            StringBuilder strBody = new StringBuilder();
            strBody.Append("Task Finished!<br>");
            strBody.Append("TaskType: Post And Clear<br>");
            strBody.Append("StartTime: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "<br>");
            string mailFileId = "";
            if (fileid.IndexOf("&") >= 0)
            {
                string[] result = fileid.Split('&');
                mailFileId = result[0];
            }
            else 
            {
                mailFileId = fileid;
            }
            if (!string.IsNullOrEmpty(mailFileId))
            {
                mailService.sendTaskFinishedMail(strBody.ToString(), AppContext.Current.User.EID, mailFileId.Replace(";", ","));
            }
           
            return fileid;
        }

        [HttpPost]
        [Route("api/cacommon/SendPmtDetailMail")]
        public string SendPmtDetailMail(string[] bsid) {
            CaCommonService service = SpringFactory.GetObjectImpl<CaCommonService>("CaCommonService");
            string[] bsidDo = new string[bsid.Length - 1];
            for (int i = 1; i < bsid.Length; i++)
            {
                bsidDo[i - 1] = bsid[i];
            }
            //第一个参数当作类型，1：需要校验是否已经发送过;0:不校验，强制再发送
            string returnCount = service.SendPmtDetailMail(bsid[0], bsidDo);
            return returnCount;
        }

        [HttpGet]
        [Route("api/cacommon/GetDateByDay")]
        public string GetDateByDay(int addDays)
        {
            return DateTime.Now.AddDays(addDays).ToString("yyyy-MM-dd");
        }

        [HttpGet]
        [Route("api/cacommon/GetDateByMonth")]
        public string GetDateByMonth(int addMonths)
        {
            return DateTime.Now.AddMonths(addMonths).ToString("yyyy-MM-dd");
        }
        
        [HttpGet]
        [Route("api/cacommon/getCaPostResultCheck")]
        public List<CaPostResultCheck> getCaPostResultCheck(string fDate, string tDate)
        {
            CaCommonService service = SpringFactory.GetObjectImpl<CaCommonService>("CaCommonService");
            var res = service.getCaPostResultCheck(fDate, tDate);
            return res;
        }

        [HttpGet]
        [Route("api/cacommon/getCaClearResultCheck")]
        public List<CaClearResultCheck> getCaClearResultCheck(string fDate, string tDate)
        {
            CaCommonService service = SpringFactory.GetObjectImpl<CaCommonService>("CaCommonService");
            var res = service.getCaClearResultCheck(fDate, tDate);
            return res;
        }

        [HttpGet]
        [Route("api/cacommon/exportPostClearResult")]
        public HttpResponseMessage exportPostClearResult(string fDate, string tDate)
        {
            CaCommonService service = SpringFactory.GetObjectImpl<CaCommonService>("CaCommonService");
            return service.exportPostClearResult(fDate, tDate);
        }


        [HttpGet]
        [Route("api/cacommon/getbsReport")]
        public CaBsReportPage getbsReport(string fDate, string tDate, int page, int pageSize)
        {
            CaCommonService service = SpringFactory.GetObjectImpl<CaCommonService>("CaCommonService");
            return service.getbsReport(fDate, tDate, page, pageSize);
        }

        [HttpGet]
        [Route("api/cacommon/exportbsReport")]
        public string exportbsReport(string fDate, string tDate)
        {
            CaCommonService service = SpringFactory.GetObjectImpl<CaCommonService>("CaCommonService");
            return service.exportbsReport(fDate, tDate);
        }


        [HttpGet]
        [Route("api/cacommon/downloadtemplete")]
        public HttpResponseMessage downloadtemplete(string fileType)
        {
            CaCommonService service = SpringFactory.GetObjectImpl<CaCommonService>("CaCommonService");
            return service.downloadtemplete(fileType);
        }

        [HttpGet]
        [Route("api/cacommon/queryCashApplicationCountReport")]
        public CashApplicationCountReportDto queryCashApplicationCountReport(string legalentity, string fDate, string tDate)
        {
            CaCommonService service = SpringFactory.GetObjectImpl<CaCommonService>("CaCommonService");
            return service.queryCashApplicationCountReport(legalentity, fDate, tDate);
        }

        [HttpGet]
        [Route("api/cacommon/exportCashApplicationCountReport")]
        public HttpResponseMessage exportCashApplicationCountReport(string legalentity, string fDate, string tDate)
        {
            CaCommonService service = SpringFactory.GetObjectImpl<CaCommonService>("CaCommonService");
            return service.exportCashApplicationCountReport(legalentity, fDate, tDate);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="legalentity"></param>
        /// <param name="fDate"></param>
        /// <param name="tDate"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/cacommon/queryCadaliyReport")]
        public List<ExportCadaliyReportDto> queryCadaliyReport(int pageindex,int pagesize, string legalEntity, string bsType, string CreateDateFrom, string CreateDateTo,
        string transNumber, string transAmount, string ValueDateFrom, string ValueDateTo, string enter, string enterMail, string crossOff, string crossOffMail)
        {
            CaCommonService service = SpringFactory.GetObjectImpl<CaCommonService>("CaCommonService");
            var result=service.queryCadaliyReport(legalEntity, bsType, CreateDateFrom, CreateDateTo, transNumber, transAmount, ValueDateFrom, ValueDateTo, enter, enterMail, crossOff, crossOffMail);
            var list = result.OrderByDescending(p => p.TRANSACTION_NUMBER).Skip((pageindex - 1) * pagesize).Take(pagesize).ToList();
            if (list != null && list.Count > 0)
            {
                list[0].count = result.Count();
            }
            return list;
        }

        [HttpGet]
        [Route("api/cacommon/exportCadaliyReport")]
        public HttpResponseMessage exportCadaliyReport(string legalEntity, string bsType, string CreateDateFrom, string CreateDateTo,
        string transNumber, string transAmount, string ValueDateFrom, string ValueDateTo, string enter, string enterMail, string crossOff, string crossOffMail)
        {
            CaCommonService service = SpringFactory.GetObjectImpl<CaCommonService>("CaCommonService");
            return service.exportCadaliyReport(legalEntity, bsType, CreateDateFrom, CreateDateTo,transNumber, transAmount, ValueDateFrom, ValueDateTo, enter, enterMail, crossOff, crossOffMail);
        }


    }
}
