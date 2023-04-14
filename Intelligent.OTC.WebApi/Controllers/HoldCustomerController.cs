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
    [UserAuthorizeFilter(actionSet: "holdcustomer")]
    public class HoldCustomerController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage GetHoldCustomer(ODataQueryOptions<HoldCustomerView> queryOptions)
        {
            HoldCustomerService service = SpringFactory.GetObjectImpl<HoldCustomerService>("HoldCustomerService");

            ODataQuerySettings setting = new ODataQuerySettings();
            IQueryable res;

            res = queryOptions.Filter.ApplyTo(service.GetHoldCustomer().AsQueryable(), setting);

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
                return Request.CreateResponse<AlertExtention<HoldCustomerView>[]>(
                    HttpStatusCode.OK,
                    new AlertExtention<HoldCustomerView>[] { new AlertExtention<HoldCustomerView>(res.Cast<HoldCustomerView>(), count){
                    } });
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }

        [HttpGet]
        [Route("api/holdCustomer/getHoldCustomer")]
        public InvoiceLog GetHoldCustomer()
        {
            InvoiceLog invoLog = new InvoiceLog();
            return invoLog;
        }
        [HttpPost]
        [Route("api/holdCustomer/saveHoldCustomer")]
        public void saveHoldCustomer([FromBody]InvoiceLog invoLogInstance)
        {
            HoldCustomerService service = SpringFactory.GetObjectImpl<HoldCustomerService>("HoldCustomerService");
            service.confirmHoldCustomer(invoLogInstance);
        }

        [HttpGet]
        public MailTmp GetHoldCustomerMailInstance(string customerNums)
        {
            HoldCustomerService service = SpringFactory.GetObjectImpl<HoldCustomerService>("HoldCustomerService");
            return service.GetNewMailInstance(customerNums);
        }

        [HttpPost]
        public void SendHoldMail([FromBody]MailTmp mail)
        {
            HoldCustomerService service = SpringFactory.GetObjectImpl<HoldCustomerService>("HoldCustomerService");
            service.SendHoldMail(mail);
        }

        [HttpGet]
        public void GetHoldCustomer(string customerNum,string legalEntity)
        {
            HoldCustomerService service = SpringFactory.GetObjectImpl<HoldCustomerService>("HoldCustomerService");
            service.cancelHoldCustomer(customerNum, legalEntity);
        }
    }
}