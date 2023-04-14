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
    [UserAuthorizeFilter(actionSet: "breakptp")]
    public class BreakPtpController : ApiController
    {

        [HttpGet]
        public HttpResponseMessage GetBPTP(ODataQueryOptions<CustomerCommon> queryOptions)
        {
            BreakPtpService service = SpringFactory.GetObjectImpl<BreakPtpService>("BreakPtpService");

            ODataQuerySettings setting = new ODataQuerySettings();
            IQueryable res;

            res = queryOptions.Filter.ApplyTo(service.GetBreakPTP().AsQueryable(), setting);

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
                return Request.CreateResponse<AlertExtention<CustomerCommon>[]>(
                    HttpStatusCode.OK,
                    new AlertExtention<CustomerCommon>[] { new AlertExtention<CustomerCommon>(res.Cast<CustomerCommon>(), count){
                         //TotalAmount = sumRes==null?0:sumRes.T1, 
                         //TotalPastDue = sumRes==null?0:sumRes.T2,
                         //TotalOver90Days = sumRes==null?0:sumRes.T3,
                         //TotalCreditLimit = sumRes==null?0:sumRes.T4
                    } });
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }

        [HttpGet]
        [Route("api/breakPtp/getBreakPTP")]
        public InvoiceLog GetBreakPTP()
        {
            InvoiceLog invoLog = new InvoiceLog();
            return invoLog;
        }

        [HttpPost]
        [Route("api/breakPtp/saveBreakPTP")]
        public void saveBreakPTP([FromBody]InvoiceLog invoLogInstance)
        {
           BreakPtpService service = SpringFactory.GetObjectImpl<BreakPtpService>("BreakPtpService");
           service.confirmBreakPTP(invoLogInstance);
        }

        [HttpGet]
        public MailTmp GetBreakPTPMailInstance(string customerNums)
        {
            BreakPtpService service = SpringFactory.GetObjectImpl<BreakPtpService>("BreakPtpService");
             return service.GetNewMailInstance(customerNums);
        }

        [HttpPost]
        public void SendBreakPTPLetter([FromBody]MailTmp mail)
        {
            BreakPtpService service = SpringFactory.GetObjectImpl<BreakPtpService>("BreakPtpService");
            service.SendBreakPTPLetter(mail);
        }

    }
}