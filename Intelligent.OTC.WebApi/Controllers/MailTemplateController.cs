using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using System.Collections.Generic;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "common")]
    public class MailTemplateController : ApiController
    {
        [HttpGet]
        [Route("api/MailTemplate/query")]
        public List<MailTemplateDto> Get(string language, string type)
        {
            //ISoaService soaService = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
            //CaMailAlertDto alertKey = new CaMailAlertDto();
            ////ID, EID, BSID, TransNumber, AlertType, LegalEntity,CustomerNum, SiteUseId, TOTITLE, CCTITLE
            //alertKey.ID = "04690c3f-879b-4e1a-974d-383a89c0a21d";
            //alertKey.EID = "siyue.chen";
            //alertKey.BSID = "8a1b8780-444f-4c40-a2a1-36982f9960e2";
            //alertKey.LegalEntity = "293";
            //alertKey.CustomerNum = "1092406";
            //alertKey.SiteUseId = "2256382";
            //alertKey.AlertType = "006";
            //string nDefaultLanguage = "001";
            ////soaService.GetCaPmtMailInstance(alertKey.EID, alertKey.ID.ToString(), alertKey.BSID, alertKey.LegalEntity, alertKey.CustomerNum, alertKey.SiteUseId, alertKey.AlertType, nDefaultLanguage);
            //soaService.GetCaClearMailInstance(alertKey.EID, alertKey.ID.ToString(), alertKey.BSID, alertKey.LegalEntity, alertKey.CustomerNum, alertKey.SiteUseId, alertKey.AlertType, nDefaultLanguage);
            //return null;
            IMailService service = SpringFactory.GetObjectImpl<IMailService>("MailService");
            return service.GetMailTemplates(language, type);
        }

        [HttpPut]
        public void Put([FromBody]MailTemplate template)
        {
            IMailService service = SpringFactory.GetObjectImpl<IMailService>("MailService");
            service.SaveOrUpdateTemplate(template);
        }

        [HttpPost]
        public void Post([FromBody]MailTemplate template)
        {

            IMailService service = SpringFactory.GetObjectImpl<IMailService>("MailService");
            service.SaveOrUpdateTemplate(template);
        }
        [HttpPost]
        [Route("api/MailTemplate/getlanguage")]
        public string GettempLanguage(string custnum, string siteUseId)
        {
            IMailService service = SpringFactory.GetObjectImpl<IMailService>("MailService");
            return service.getCustomerLanguageByCusnum(custnum, siteUseId);
        }

        [HttpDelete]
        public void delete(int id)
        {
            MailService service = SpringFactory.GetObjectImpl<MailService>("MailService");
            MailTemplate old = service.CommonRep.FindBy<MailTemplate>(id);
            if (old != null)
            {
                service.CommonRep.Remove(old);
                service.CommonRep.Commit();
            }

        }
    }
}