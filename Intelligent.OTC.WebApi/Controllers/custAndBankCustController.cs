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

    public class CustAndBankCustController : ApiController
    {
        [HttpGet]
        [Route("api/custAndBankCust/getCustomerMapping")]
        public CACustomerMappingDtoPage getCustomerMapping(int page,int pageSize, string legalEntity, string customerNum, string bankCustomerName)
        {
            ICustAndBankCustService service = SpringFactory.GetObjectImpl<ICustAndBankCustService>("CustAndBankCustService");
            var res = service.getCustomerMapping(page,pageSize, legalEntity, customerNum, bankCustomerName);
            return res;
        }

        [HttpGet]
        [Route("api/custAndBankCust/getCustomerName")]
        public CACustomerMappingDto getCustomerName(string customerNum, string legalEntity)
        {
            ICustAndBankCustService service = SpringFactory.GetObjectImpl<ICustAndBankCustService>("CustAndBankCustService");
            var res = service.getCustomerName(customerNum, legalEntity);
            return res;
        }

        [HttpPost]
        [Route("api/custAndBankCust/customerMapping")]
        public void Post(CACustomerMappingDto model)
        {
            ICustAndBankCustService service = SpringFactory.GetObjectImpl<ICustAndBankCustService>("CustAndBankCustService");
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
        [Route("api/custAndBankCust/customerMapping")]
        public void Delete(string id)
        {
            ICustAndBankCustService service = SpringFactory.GetObjectImpl<ICustAndBankCustService>("CustAndBankCustService");
            service.Remove(id);
        }


        [HttpGet]
        [Route("api/custAndBankCust/exporAll")]
        public HttpResponseMessage ExporAll()
        {
            ICustAndBankCustService service = SpringFactory.GetObjectImpl<ICustAndBankCustService>("CustAndBankCustService");
            return service.exporAll();
        }
    }
}
