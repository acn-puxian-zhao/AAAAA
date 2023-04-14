using Intelligent.OTC.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;

namespace Intelligent.OTC.WebApi.Core
{
    public class HttpWebApiControllerActionInvoker : ApiControllerActionInvoker
    {
        JsonMediaTypeFormatter formatter = new JsonMediaTypeFormatter();
        public override Task<HttpResponseMessage> InvokeActionAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            var responseMessage = base.InvokeActionAsync(actionContext, cancellationToken);

            if (responseMessage.Exception != null)
            {
                var baseException = responseMessage.Exception.InnerExceptions[0];
                var result = new OutResult() { code = "404", msg = "Invalid request" };

                if (baseException is TimeoutException)
                {
                    result.code = "404";
                    result.msg = "Invalid request";
                }

                return Task.Run(() => new HttpResponseMessage()
                {
                    Content = new ObjectContent(typeof(OutResult), result, formatter),
                    StatusCode = HttpStatusCode.OK
                }, cancellationToken);
            }
            return responseMessage;
        }
    }
}