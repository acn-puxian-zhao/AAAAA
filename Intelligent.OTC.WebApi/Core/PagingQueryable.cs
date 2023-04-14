using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;
using System.Web.OData;
using System.Web.OData.Extensions;

namespace Intelligent.OTC.WebApi.Core
{
    public class QueryableAttribute : Attribute
    {

    }

    public class PagingQueryableAttribute : EnableQueryAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            var inlinecount = HttpUtility.ParseQueryString(actionExecutedContext.Request.RequestUri.Query).Get("$count");

            bool hasInLineCount = false;
            if (inlinecount == "true")
                hasInLineCount = true;

            base.OnActionExecuted(actionExecutedContext);

            if (ResponseIsValid(actionExecutedContext.Response))
            {
                object responseObject;

                actionExecutedContext.Response.TryGetContentValue(out responseObject);

                if (responseObject is IEnumerable<object> && hasInLineCount)
                {
                    var robj = responseObject as IEnumerable<object>;
                    long? count = actionExecutedContext.Request.ODataProperties().TotalCount;

                    if (hasInLineCount)
                    {
                        actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(HttpStatusCode.OK, new ODataMetadata<object>[] { new ODataMetadata<object>(robj, count) });
                    }
                }
            }
        }

        private bool ResponseIsValid(HttpResponseMessage response)
        {
            if (response == null || response.StatusCode != HttpStatusCode.OK || !(response.Content is ObjectContent)) return false;
            return true;
        }
    }
}