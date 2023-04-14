using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Domain.DataModel;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.OData;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "common")]
    public class BaseDataController : ApiController
    {
        [HttpGet]
        [EnableQuery]
        public IQueryable<SysTypeDetail> Get(string strTypecode)
        {
            BaseDataService basedata = SpringFactory.GetObjectImpl<BaseDataService>("BaseDataService");
            var dataList = basedata.GetSysTypeDetail(strTypecode);
            return dataList.AsQueryable();
        }

        [HttpGet]
        public Dictionary<string, List<SysTypeDetail>> GetSysTypeDetails(string strTypeCodes)
        {
            IBaseDataService basedata = SpringFactory.GetObjectImpl<IBaseDataService>("BaseDataService");
            return basedata.GetSysTypeDetails(strTypeCodes);
        }

        [HttpPost]
        public void Post(string authCode, string userMail)
        {
            BaseDataService basedata = SpringFactory.GetObjectImpl<BaseDataService>("BaseDataService");
            basedata.InitialUser(authCode, userMail);
        }

        [HttpGet]
        public string Get(string mailBox, string deal)
        {
            MailService ms = SpringFactory.GetObjectImpl<MailService>("MailService");
            ms.ProcessMailBox(mailBox, deal);
            return "ProcessMailBox called.";
        }

        [HttpGet]
        public string Get(string mailBox, string deal, string messageId)
        {
            MailService ms = SpringFactory.GetObjectImpl<MailService>("MailService");
            if (messageId == "ALL")
            {
                ms.RetryAllMail(new { mailBox, deal });
                return "RetryAllMail completed.";
            }
            else
            {
                ms.RetryOneMail(mailBox, deal, messageId);
                return "RetryProcessMail completed.";
            }
        }

        [HttpGet]
        public bool GetAuthentication()
        {
            BaseDataService basedata = SpringFactory.GetObjectImpl<BaseDataService>("BaseDataService");
            return basedata.CheckAuthentication();
        }

        [HttpPost]
        public void SaveCollectionCalendarConfig(string customerNum, string legalEntity, [FromBody] List<string> list)
        {
            BaseDataService service = SpringFactory.GetObjectImpl<BaseDataService>("BaseDataService");
            service.SaveCollectionCalendarConfig(customerNum, legalEntity, list);
        }
    }
}