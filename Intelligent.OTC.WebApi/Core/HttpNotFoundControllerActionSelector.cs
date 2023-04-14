using Intelligent.OTC.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Intelligent.OTC.WebApi.Core
{
    public class HttpNotFoundControllerActionSelector : ApiControllerActionSelector
    {
        JsonMediaTypeFormatter formatter = new JsonMediaTypeFormatter();
        public override HttpActionDescriptor SelectAction(HttpControllerContext controllerContext)
        {
            HttpActionDescriptor decriptor = null;
            try
            {
                decriptor = base.SelectAction(controllerContext);
            }
            catch (HttpResponseException ex)
            {
                var code = ex.Response.StatusCode;
                var result = new OutResult() { code = "404", msg = "Invalid request" };
                if (code == HttpStatusCode.NotFound || code == HttpStatusCode.MethodNotAllowed)
                {
                    ex.Response.Content = new ObjectContent(typeof(OutResult), result, formatter);
                }
                ex.Response.StatusCode = HttpStatusCode.OK;
                throw ex;
            }
            return decriptor;
        }
    }
}