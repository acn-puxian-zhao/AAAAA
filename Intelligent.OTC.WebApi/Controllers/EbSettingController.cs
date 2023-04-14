using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "ebsetting")]
    public class EbSettingController : ApiController
    {
        [HttpGet]
        [Route("api/ebsetting/getlist")]
        public LegalEBDtoPage getEBSettinglist(string region, string legalEntity, string ebname,string collector, int page, int pagesize)
        {
            EBSettingService service = SpringFactory.GetObjectImpl<EBSettingService>("EBSettingService");
            return service.getEBSettinglist(region, legalEntity, ebname, collector, page, pagesize);
        }

        [HttpGet]
        [Route("api/ebsetting/downloadEBList")]
        public string downloadEBList(string region, string legalEntity, string ebname, string collector)
        {
            EBSettingService service = SpringFactory.GetObjectImpl<EBSettingService>("EBSettingService");
            return service.downloadEBList(region, legalEntity, ebname, collector);
        }

        [HttpPost]
        [Route("api/ebsetting/updateLegalEB")]
        public void updateLegalEB(LegalEBDto model)
        {
            EBSettingService service = SpringFactory.GetObjectImpl<EBSettingService>("EBSettingService");
            try
            {
                service.AddOrUpdateLegalEb(model);
            }
            catch (Common.Exceptions.OTCServiceException ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException(ex.Message);
            }
            catch
            {
                Exception ex = new OTCServiceException("Update Error!");
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }

        }


        [HttpDelete]
        [Route("api/ebsetting/deleteLegalEB")]
        public void deleteLegalEB(string id)
        {
            try
            {
                EBSettingService service = SpringFactory.GetObjectImpl<EBSettingService>("EBSettingService");
                service.deleteLegalEB(id);
            }
            catch (Exception ex) {
                throw new Exception("Delete legal eb faild." + ex.Message);
            }
        }


        [HttpPost]
        [Route("api/ebsetting/import")]
        public string Import()
        {
            EBSettingService service = SpringFactory.GetObjectImpl<EBSettingService>("EBSettingService");

            return service.Import();
        }

    }
}
