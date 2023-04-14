using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using System;
using System.Linq;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    public class CustomerContactCoverageController : ApiController
    {
        [HttpPost]
        [Route("api/customerContactCoverage/GetCustomerContactCount")]
        public IQueryable<Object> GetCustomerContactCount(int year,int month)
        {
            CustomerContactCoverageService service = SpringFactory.GetObjectImpl<CustomerContactCoverageService>("CustomerContactCoverageService");
            return service.GetCustomerContactCount(year,month);
        }

        [HttpPost]
        [Route("api/customerContactCoverage/GetCustomerCountPercent")]
        public IQueryable<Object> GetCustomerCountPercent(int year, int month)
        {
            CustomerContactCoverageService service = SpringFactory.GetObjectImpl<CustomerContactCoverageService>("CustomerContactCoverageService");
            return service.GetCustomerCountPercent(year, month);
        }
    }
}