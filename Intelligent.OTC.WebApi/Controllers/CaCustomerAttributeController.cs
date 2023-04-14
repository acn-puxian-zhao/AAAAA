using Intelligent.OTC.Business.Interfaces;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.DomainModel;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Net.Http;

namespace Intelligent.OTC.WebApi.Controllers
{

    public class CaCustomerAttributeController : ApiController
    {
        [HttpGet]
        [Route("api/customerAttribute/getCustomerAttribute")]
        public CaCustomerAttributeDtoPage getCustomerAttribute(int page,int pageSize, string legalEntity, string customerNum)
        {
            ICaCustomerAttributeService service = SpringFactory.GetObjectImpl<ICaCustomerAttributeService>("CaCustomerAttributeService");
            var res = service.getCaCustomerAttribute(page, pageSize, legalEntity, customerNum);
            return res;
        }

        [HttpGet]
        [Route("api/customerAttribute/getBankCharge")]
        public CaCustomerAttributeDto getBankCharge(string customerNum,string legalEntity)
        {
            ICaCustomerAttributeService service = SpringFactory.GetObjectImpl<ICaCustomerAttributeService>("CaCustomerAttributeService");
            var res = service.getCaCustomerAttributeByCustomerNum(customerNum, legalEntity);
            return res;
        }

        [HttpPost]
        [Route("api/customerAttribute/attribute")]
        public void Post(CaCustomerAttributeDto model)
        {
            ICaCustomerAttributeService service = SpringFactory.GetObjectImpl<ICaCustomerAttributeService>("CaCustomerAttributeService");
            try
            {
                service.AddOrUpdate(model);
            }
            catch (Common.Exceptions.OTCServiceException ex)
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
        [Route("api/customerAttribute/attribute")]
        public void Delete(string id)
        {
            ICaCustomerAttributeService service = SpringFactory.GetObjectImpl<ICaCustomerAttributeService>("CaCustomerAttributeService");
            service.Remove(id);
        }


        [HttpGet]
        [Route("api/customerAttribute/exporAll")]
        public HttpResponseMessage ExporAll()
        {
            ICaCustomerAttributeService service = SpringFactory.GetObjectImpl<ICaCustomerAttributeService>("CaCustomerAttributeService");
            return service.exporAll();
        }
    }
}
