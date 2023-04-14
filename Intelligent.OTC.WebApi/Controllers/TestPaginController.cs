using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using OTC.POC.Common.UnitOfWork;
using OTC.POC.Repository;
using OTC.POC.Business;
using OTC.POC.Repository.Repositories;
using OTC.POC.Repository.DataModel;
using OTC.POC.WebApi.Core;
using OTC.POC.Common.Utils;
using OTC.POC.Common.Factory;

namespace OTC.POC.WebApi.Controllers
{
    public class TestPagingController : ApiController
    {
        // GET api/<controller>
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}
        [HttpGet]
        [PagingQueryable]
        //public IEnumerable<TestContact> Get()
        public IEnumerable<CustomerAgingStaging> Get()
        {
            //IUnitOfWork uow = new EFUnitOfWork();
            ICustomerService service = SpringFactory.GetObjectImpl<ICustomerService>("CustomerService");
            return service.GetCustomerAgingStaging().AsQueryable<CustomerAgingStaging>();
        }

        [HttpPut]
        public void Put([FromBody]CustomerAgingStaging cust)
        {
            CustomerService service = new CustomerService();
            CustomerAgingStaging old = service.CommonRep.FindBy<CustomerAgingStaging>(cust.Id);
            ObjectHelper.CopyObjectWithUnNeed(cust, old, new string[] { "Id" });
            //service.CommonRep.Remove(old);
            //service.CommonRep.Add(cust);
            service.UOW.Commit();
        }

        [HttpDelete]
        public void Delete(int id)
        {
            CustomerService service = new CustomerService();
            CustomerAgingStaging old = service.CommonRep.FindBy<CustomerAgingStaging>(id);
            if (old != null)
            {
                List<InvoiceAgingStaging> invoice =
    service.CommonRep.GetQueryable<InvoiceAgingStaging>().ToList().FindAll(m => m.CustomerNum == old.CustomerNum && m.SiteCode == old.SiteCode && m.Operator == old.SiteCode);
                if (invoice != null)
                {
                    //delete invoice informations
                    foreach (var inv in invoice) {
                        service.CommonRep.Remove(inv);
                    }
                }
                //delete aging informations
                service.CommonRep.Remove(old);
                service.UOW.Commit();
            }
        }

        // GET api/<controller>/5
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // POST api/<controller>
        //public void Post([FromBody]string value)
        //{
        //}

        // PUT api/<controller>/5
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        // DELETE api/<controller>/5
        //public void Delete(int id)
        //{

        //}

        public class TestContact
        {
            public string Id { get; set; }
            public string ContactName { get; set; }
            public string ContactCode { get; set; }
            public string CustomerName { get; set; }
        }
    }
}
//return new TestContact[]{
//    new TestContact(){ ContactName = "alex1", ContactCode = "001", CustomerName = "ACN", Id = "1"}, 
//    new TestContact(){ ContactName = "ben", ContactCode = "002", CustomerName = "ACN", Id = "2"},
//    new TestContact(){ ContactName = "celia", ContactCode = "003", CustomerName = "ACN", Id = "3"}, 
//    new TestContact(){ ContactName = "david", ContactCode = "004", CustomerName = "ACN", Id = "4"},
//    new TestContact(){ ContactName = "ellen", ContactCode = "005", CustomerName = "ACN", Id = "5"}, 
//    new TestContact(){ ContactName = "frank", ContactCode = "006", CustomerName = "ACN", Id = "6"},
//    new TestContact(){ ContactName = "gum", ContactCode = "007", CustomerName = "ACN", Id = "7"}, 
//    new TestContact(){ ContactName = "hiber", ContactCode = "008", CustomerName = "ACN", Id = "8"},
//    new TestContact(){ ContactName = "issue", ContactCode = "009", CustomerName = "ACN", Id = "9"},
//    new TestContact(){ ContactName = "jack", ContactCode = "010", CustomerName = "ACN", Id = "10"},
//    new TestContact(){ ContactName = "kevin", ContactCode = "011", CustomerName = "NTT", Id = "11"},
//    new TestContact(){ ContactName = "liv", ContactCode = "012", CustomerName = "NTT", Id = "12"},
//    new TestContact(){ ContactName = "moon", ContactCode = "013", CustomerName = "NTT", Id = "13"}, 
//    new TestContact(){ ContactName = "numb", ContactCode = "014", CustomerName = "NTT", Id = "14"},
//    new TestContact(){ ContactName = "oppo", ContactCode = "015", CustomerName = "NTT", Id = "15"}, 
//    new TestContact(){ ContactName = "peter", ContactCode = "016", CustomerName = "NTT", Id = "16"},
//    new TestContact(){ ContactName = "queen", ContactCode = "017", CustomerName = "NTT", Id = "17"}, 
//    new TestContact(){ ContactName = "rose", ContactCode = "018", CustomerName = "NTT", Id = "18"},
//    new TestContact(){ ContactName = "susan", ContactCode = "019", CustomerName = "NTT", Id = "19"},
//    new TestContact(){ ContactName = "tween", ContactCode = "020", CustomerName = "NTT", Id = "20"},
//    new TestContact(){ ContactName = "uban", ContactCode = "021", CustomerName = "IBM", Id = "21"},
//    new TestContact(){ ContactName = "vivan", ContactCode = "022", CustomerName = "IBM", Id = "22"},
//    new TestContact(){ ContactName = "white", ContactCode = "023", CustomerName = "IBM", Id = "23"}, 
//    new TestContact(){ ContactName = "xray", ContactCode = "024", CustomerName = "IBM", Id = "24"},
//    new TestContact(){ ContactName = "yepp", ContactCode = "025", CustomerName = "IBM", Id = "25"}, 
//    new TestContact(){ ContactName = "zippo", ContactCode = "026", CustomerName = "IBM", Id = "26"},
//    new TestContact(){ ContactName = "allen", ContactCode = "027", CustomerName = "IBM", Id = "27"}, 
//    new TestContact(){ ContactName = "benef", ContactCode = "028", CustomerName = "IBM", Id = "28"},
//    new TestContact(){ ContactName = "cru", ContactCode = "029", CustomerName = "IBM", Id = "29"},
//    new TestContact(){ ContactName = "dacy", ContactCode = "030", CustomerName = "IBM", Id = "30"},
//    new TestContact(){ ContactName = "every", ContactCode = "031", CustomerName = "NEC", Id = "31"}
//}.AsQueryable<TestContact>();
