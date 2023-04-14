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

    public class CaCustomerForwardController : ApiController
    {
        [HttpGet]
        [Route("api/forwarder/getForwarder")]
        public CAForwarderListDtoPage GetForwarder(int page,int pageSize,string legalEntity,string customerNum,string forwardNum,string forwardName)
        {
            ICaCustomerForwardService service = SpringFactory.GetObjectImpl<ICaCustomerForwardService>("CaCustomerForwardService");
            var res = service.getForwarder(page,pageSize, legalEntity, customerNum, forwardNum, forwardName);
            return res;
        }

        [HttpGet]
        [Route("api/forwarder/getCustomerName")]
        public CACustomerMappingDto getCustomerName(string customerNum)
        {
            ICaCustomerForwardService service = SpringFactory.GetObjectImpl<ICaCustomerForwardService>("CaCustomerForwardService");
            var res = service.getCustomerName(customerNum);
            return res;
        }

        [HttpPost]
        [Route("api/forwarder/addForwarder")]
        public void Post(CAForwarderListDto model)
        {
            ICaCustomerForwardService service = SpringFactory.GetObjectImpl<ICaCustomerForwardService>("CaCustomerForwardService");
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
        [Route("api/forwarder/removeForwarder")]
        public void Delete(string id)
        {
            ICaCustomerForwardService service = SpringFactory.GetObjectImpl<ICaCustomerForwardService>("CaCustomerForwardService");
            service.Remove(id);
        }


        [HttpGet]
        [Route("api/forwarder/exporAll")]
        public HttpResponseMessage ExporAll()
        {
            ICaCustomerForwardService service = SpringFactory.GetObjectImpl<ICaCustomerForwardService>("CaCustomerForwardService");
            return service.exporAll();
        }
    }
}
