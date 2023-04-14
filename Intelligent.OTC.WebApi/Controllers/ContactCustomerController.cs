using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.WebApi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "contactcustomer")]
    public class ContactCustomerController : ApiController
    {
        [HttpGet]
        [PagingQueryable]
        public IEnumerable<ContactCustomerDto> GetContactCustomer(string invoiceState, string invoiceTrackState, string legalEntity, string invoiceNum, string soNum, string poNum, string invoiceMemo)
        {
            IContactCustomerService service = SpringFactory.GetObjectImpl<IContactCustomerService>("ContactCustomerService");
            return service.GetContactCustomer(invoiceState, invoiceTrackState, legalEntity,invoiceNum,soNum,poNum,invoiceMemo).AsQueryable<ContactCustomerDto>();
        }

        [HttpGet]
        public string Get(string customerNum, string customerStatus)
        {
            ContactService service = SpringFactory.GetObjectImpl<ContactService>("ContactService");
            var customerName = service.customerNameGet(customerNum);
            return customerName;
        }

        [HttpGet]
        [PagingQueryable]
        public IEnumerable<MailDto> GetCustomerMail(string strCustNum, string status, string type)
        {
            Exception ex = new NotImplementedException();
            Helper.Log.Error(ex.Message, ex);
            throw new Exception("Get mail failed");
        }

        [HttpGet]
        [PagingQueryable]
        public IEnumerable<SendSoaHead> InvoiceGet(string custNum, string type, string legalEntity)
        {
            ContactService service = SpringFactory.GetObjectImpl<ContactService>("ContactService");
            return service.invoiceAgingGet(custNum, legalEntity,type).AsQueryable();
        }

        [HttpGet]
        [PagingQueryable]
        public IEnumerable<ContactHistory> GetContactList(string strCusNum)
        {
            ContactCustomerService service = SpringFactory.GetObjectImpl<ContactCustomerService>("ContactCustomerService");
            var conHistoryList = service.GetContactList(strCusNum);
            List<ContactHistory> ContactHistoryList = new List<ContactHistory>();
            ContactHistory conHistory = new ContactHistory();
            int countId = 1;
            foreach (var item in conHistoryList) {

                conHistory = new ContactHistory();
                conHistory.Id = item.Id;
                conHistory.AlertId = item.AlertId;
                conHistory.CollectorId = item.CollectorId;
                conHistory.Comments = item.Comments;
                conHistory.ContactDate = item.ContactDate;
                conHistory.ContacterId = item.ContacterId;
                conHistory.ContactType = item.ContactType;
                conHistory.CustomerNum = item.CustomerNum;
                conHistory.Deal = item.Deal;
                conHistory.LegalEntity = item.LegalEntity;
                conHistory.sortId = countId;
                //added by zhangYu get contact detail
                conHistory.ContactId = item.ContactId;
                ContactHistoryList.Add(conHistory);

                countId++;
            }

            return ContactHistoryList.AsQueryable<ContactHistory>();
        }

        [HttpGet]
        [PagingQueryable]
        public IEnumerable<Dispute> GetDisputeList(string strCusNumber)
        {
            ContactCustomerService service = SpringFactory.GetObjectImpl<ContactCustomerService>("ContactCustomerService");
            var disputeList = service.GetDisputeList(strCusNumber);
            List<Dispute> DisputeList = new List<Dispute>();
            Dispute dispute = new Dispute();
            int countId = 1;
            foreach (var item in disputeList)
            {
                dispute = new Dispute();
                dispute.Id = item.Id;
                dispute.Deal = item.Deal;
                dispute.Eid = item.Eid;
                dispute.CloseDate = item.CloseDate;
                dispute.Comments = item.Comments;
                dispute.ContactId = item.ContactId;
                dispute.CreateDate = item.CreateDate;
                dispute.CreatePerson = item.CreatePerson;
                dispute.CustomerNum = item.CustomerNum;
                dispute.IssueReason = Helper.CodeToEnum<DisputeReason>(item.IssueReason).ToString();
                dispute.Status = Helper.CodeToEnum<DisputeStatus>(item.Status).ToString();
                dispute.sortId = countId;
                DisputeList.Add(dispute);

                countId++;
            }

            return DisputeList.AsQueryable<Dispute>();
        }

        //added by zhangYu contactCustomer contactHistory call detail
        [HttpGet]
        public Call GetCallInfoByContactId(string contactId)
        {
            ContactCustomerService service = SpringFactory.GetObjectImpl<ContactCustomerService>("ContactCustomerService");
            Call ca = new Call();
            if (contactId != "0")
            {
                ca = service.GetCallInfoByContactId(contactId);
            }
            return ca;
        }

        //added by zhangYu contactCustomer contactHistory call detail
        [HttpPost]
        public void saveCallInfo([FromBody]Call callInstance)
        {
            ContactCustomerService service = SpringFactory.GetObjectImpl<ContactCustomerService>("ContactCustomerService");
            
            //create
            if(string.IsNullOrEmpty (callInstance.ContactId))
            {
                service.WriteCallLog(callInstance);
            }
            else//update
            {
                service.UpdateCallLog(callInstance);
            }

        }

        [HttpGet]
        public HttpResponseMessage ExpoertInvoiceList(string exportlist)
        {
            ContactCustomerService service = SpringFactory.GetObjectImpl<ContactCustomerService>("ContactCustomerService");
            return service.ExpoertInvoiceList();
        }
    }

}