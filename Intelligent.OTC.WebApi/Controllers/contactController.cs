using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "master")]
    public class ContactController : ApiController
    {
        string dear = AppContext.Current.User.Deal.ToString();

        [HttpGet]
        public IEnumerable<Contactor> Get(string customerCode)
        {
            //strDeal = dear;
            ContactService service = SpringFactory.GetObjectImpl<ContactService>("ContactService");
            var collectorList = service.GetContactByCustomer(customerCode);
            return collectorList.AsQueryable<Contactor>();
        }

        [HttpGet]
        public IEnumerable<Contactor> GetContact(string customerNums)
        {
            Exception ex = new Exception("Obsolete Api (contactController.GetContact), Must input siteUseid");
            Helper.Log.Error(ex.Message, ex);
            throw ex;
        }

        [HttpGet]
        [Route("api/contact/getbysiteuseid")]
        public IEnumerable<Contactor> GetContacts(string siteUseId)
        {
            ContactService service = SpringFactory.GetObjectImpl<ContactService>("ContactService");
            var collectorList = service.GetContactBySiteUseId(siteUseId);
            return collectorList;
        }

        [HttpGet]
        [Route("api/contact/export")]
        public string Export(string custnum, string name, string siteUseId, string legalEntity)
        {
            ContactService service = SpringFactory.GetObjectImpl<ContactService>("ContactService");

            return service.Export(custnum, name, siteUseId, legalEntity);
        }

        [HttpGet]
        public IEnumerable<Contactor> GetContact(string customerNums,string siteUseid)
        {
            List<CustomerKey> customerKeys = new List<CustomerKey>();
            List<string> cusNums = customerNums.Split(',').ToList();
            List<string> siteUids = siteUseid.Split(',').ToList();

            ContactService service = SpringFactory.GetObjectImpl<ContactService>("ContactService");
            var collectorList = service.GetContactsByCustomers(cusNums, siteUids);
            return collectorList.AsQueryable<Contactor>();
        }

        [HttpPost]
        [Route("api/contact/CopyContactors")]
        public void CopyContactors(CopyContactDto dto)
        {
            ContactService service = SpringFactory.GetObjectImpl<ContactService>("ContactService");
            service.CopyContactors(dto);
        }

        [HttpPost]
        [Route("api/contact/batchupdate")]
        public int BatchUpdate(ContactBatchUpdateDto dto)
        {
            ContactService service = SpringFactory.GetObjectImpl<ContactService>("ContactService");
            return service.BatchUpdate(dto);
        }

        [HttpPost]
        public void delete(int id)
        {
            ContactService service = SpringFactory.GetObjectImpl<ContactService>("ContactService");
            Helper.Log.Info(id);
            service.DeleteContact(id);
        }

        [HttpPost]
        public void Post([FromBody] Contactor cust)
        {
            ContactService service = SpringFactory.GetObjectImpl<ContactService>("ContactService");
            try
            {
                service.AddOrUpdateContact(cust);
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

        [HttpPost]
        public void Post(string type, [FromBody] ContactorDomain cont)
        {
            ContactService service = SpringFactory.GetObjectImpl<ContactService>("ContactService");
            service.AddOrUpdateDomain(cont);
        }

        [HttpPost]
        public void deleteDomain(int domainid)
        {
            ContactService service = SpringFactory.GetObjectImpl<ContactService>("ContactService");
            Helper.Log.Info(domainid);
            service.DeleteDomain(domainid);
        }
    }
}