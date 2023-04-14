using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.WebApi.Core;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    //[UserAuthorizeFilter(actionSet: "master")]
    public class CollectionReportController : ApiController
    {
        public const string strArchiveOneYearSalesKey = "ArchiveOneYearSalesPath";//OneYearSales路径的config保存名

        [HttpGet]
        [PagingQueryable]
        public IEnumerable<CustomerMasterData> Get()
        {
            ICustomerService service = SpringFactory.GetObjectImpl<ICustomerService>("CustomerService");
            var res = service.GetCustMasterData("");

            return res.AsQueryable();
        }

        [HttpGet]
        public Customer Get(string num)
        {
            ICustomerService service = SpringFactory.GetObjectImpl<ICustomerService>("CustomerService");
            Customer cust = service.GetOneCustomer(num);
            return cust;
        }

        [HttpGet]
        public List<Customer> GetCustomerByCustomerNum(string cusNum)
        {
            ICustomerService service = SpringFactory.GetObjectImpl<ICustomerService>("CustomerService");
            return service.GetCustomerByCustomerNum(cusNum);
        }

        [HttpPut]
        public string Put([FromBody]Customer cust)
        {
            CustomerService service = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
            string message = service.UpdateCustMasterData(cust);
            return message;

        }

        [HttpPost]
        public string ExpoertCust(string custnum, string custname, string status, string collector,
                                    string begintime, string endtime, string miscollector, string misgroup,
                                     string billcode, string country, string siteUseId, string legalEntity, string EbName)
        {
            CustomerService service = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
            return service.ExportCustomer(custnum, custname, status, collector, begintime, endtime,
                                miscollector, misgroup, billcode, country, siteUseId, legalEntity, EbName);
        }
        [HttpPost]
        public string ImportCust(string type)
        {
            CustomerService service = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
            return service.ImportCustHistory();
        }

        [HttpPost]
        public string Post([FromBody] Customer cust)
        {
            CustomerService service = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
            string message = service.AddCustMasterData(cust);
            return message;
        }

        [HttpGet]
        public Customer GetCustModel(string type)
        {
            if (type == "new")
            {
                Customer cust = new Customer();
                cust.Id = 0;
                return cust;
            }
            else
            {
                return null;
            }
        }

        [HttpPost]
        public void FinishSoa(string num, string site)
        {

        }

        [HttpGet]
        [PagingQueryable]
        public IEnumerable<CustomerMasterDto> GetAssign(string customers)
        {
            ICustomerService service = SpringFactory.GetObjectImpl<ICustomerService>("CustomerService");
            var res = service.GetCustMasterDataForAssign(customers);

            return res.AsQueryable();
        }
    }
}