using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.WebApi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "contacthistory")]
    public class ContactHistoryController : ApiController
    {
        [HttpGet]
        [PagingQueryable]
        public IEnumerable<ContactHistory> Get()
        {
            ContactService service = SpringFactory.GetObjectImpl<ContactService>("ContactService");
            return service.GetContactHistory().AsQueryable<ContactHistory>();
        }

        [HttpGet]
        public DisputeInvoice GetDisputeInvoice(string type)
        {
            DisputeInvoice disInv = new DisputeInvoice();
            return disInv;
        }

        [HttpGet]
        [Route("api/contacthistory/find")]
        public ContactHistory Find(string contactId)
        {
            if (string.IsNullOrWhiteSpace(contactId)) return new ContactHistory();

            ContactService service = SpringFactory.GetObjectImpl<ContactService>("ContactService");
            var history = service.GetContactHistory(contactId);
            if (history == null)
            {
                history = new ContactHistory();
            }
            return history;
        }

        [HttpPost]
        [Route("api/contacthistory/create")]
        public void Create(ContactHistoryCreateDto createDto)
        {
            ContactService service = SpringFactory.GetObjectImpl<ContactService>("ContactService");

            service.CreateContactHistory(createDto);
        }

        [HttpPost]
        [Route("api/contacthistory/update")]
        public void Update(ContactHistoryUpdateDto updateDto)
        {
            ContactService service = SpringFactory.GetObjectImpl<ContactService>("ContactService");
            service.UpdateContactHistory(updateDto);
        }

        [HttpPost]
        public void saveInfo([FromBody]DisputeInvoice disInvInstance)
        {
            ContactService service = SpringFactory.GetObjectImpl<ContactService>("ContactService");
            service.insertInvoiceLogForDispute(disInvInstance);
        }
    }
}