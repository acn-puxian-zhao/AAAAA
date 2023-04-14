using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.OData.Query;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "unholdcustomer")]
    public class UnholdCustomerController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage GetUnHoldCustomer(ODataQueryOptions<UnHoldCustomer> queryOptions)
        {
            HoldCustomerService service = SpringFactory.GetObjectImpl<HoldCustomerService>("HoldCustomerService");

            ODataQuerySettings setting = new ODataQuerySettings();


            IQueryable res = queryOptions.Filter.ApplyTo(service.GetUnHoldCustomer().AsQueryable(), setting);

            long? count = 0;
            if (queryOptions.Count != null)
            {
                count = queryOptions.Count.GetEntityCount(res);
            }
            if (queryOptions.OrderBy != null)
            {
                res = queryOptions.OrderBy.ApplyTo(res, setting);
            }

            if (queryOptions.Skip != null)
            {
                res = queryOptions.Skip.ApplyTo(res, setting);
            }

            if (queryOptions.Top != null)
            {
                res = queryOptions.Top.ApplyTo(res, setting);
            }

            if (queryOptions.Count != null)
            {
                return Request.CreateResponse<AlertExtention<UnHoldCustomer>[]>(
                    HttpStatusCode.OK,
                    new AlertExtention<UnHoldCustomer>[] { new AlertExtention<UnHoldCustomer>(res.Cast<UnHoldCustomer>(), count){
                    } });
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }
        //unholdCustomer
        [HttpGet]
        public void UnholdCustomer(string customerNum, string legalEntity,string reMailId)
        {
            HoldCustomerService service = SpringFactory.GetObjectImpl<HoldCustomerService>("HoldCustomerService");
            service.unHoldCustomer(customerNum, legalEntity, reMailId);
        }
    }
}