using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "master")]
    public class ContactReplaceController : ApiController
    {
        [HttpGet]
        [Route("api/contactreplace")]
        public IEnumerable<T_CONTACTOR_REPLACE> Get()
        {
            //strDeal = dear;
            ContactReplaceService service = SpringFactory.GetObjectImpl<ContactReplaceService>("ContactReplaceService");
            return service.GetAll();
        }

        [HttpPost]
        [Route("api/contactreplace")]
        public void Post(T_CONTACTOR_REPLACE model)
        {
            ContactReplaceService service = SpringFactory.GetObjectImpl<ContactReplaceService>("ContactReplaceService");
            try
            {
                service.AddOrUpdate(model);
            }
            catch (OTCServiceException ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException(ex.Message);
            }
            catch
            {
                Exception ex = new OTCServiceException("Add Or Update Error!");
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }

        }

        [HttpDelete]
        [Route("api/contactreplace/delete")]
        public void Delete(int id)
        {
            ContactReplaceService service = SpringFactory.GetObjectImpl<ContactReplaceService>("ContactReplaceService");
            service.Remove(id);
        }

        [HttpDelete]
        [Route("api/contactreplace")]
        public void Delete()
        {
            ContactReplaceService service = SpringFactory.GetObjectImpl<ContactReplaceService>("ContactReplaceService");
            service.Remove();
        }

        [HttpGet]
        [Route("api/contactreplace/export")]
        public string Export()
        {
            ContactReplaceService service = SpringFactory.GetObjectImpl<ContactReplaceService>("ContactReplaceService");

            return service.Export();
        }

        [HttpPost]
        [Route("api/contactreplace/import")]
        public string Import()
        {
            ContactReplaceService service = SpringFactory.GetObjectImpl<ContactReplaceService>("ContactReplaceService");

            return service.Import();
        }
    }
}