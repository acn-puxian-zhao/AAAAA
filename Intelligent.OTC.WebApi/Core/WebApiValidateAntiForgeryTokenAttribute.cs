using System;
using System.Linq;
using System.Net.Http;
using System.Web.Helpers;
using System.Web.Http.Filters;

namespace Intelligent.OTC.WebApi.Core
{
    public sealed class WebApiValidateAntiForgeryTokenAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(
            System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                Exception ex = new ArgumentNullException("actionContext");
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }

            if (actionContext.Request.Method.Method != "GET")
            {
                var headers = actionContext.Request.Headers;
                var tokenCookie = headers
                    .GetCookies()
                    .Select(c => c[AntiForgeryConfig.CookieName])
                    .FirstOrDefault();

                var tokenHeader = string.Empty;
                if (headers.Contains("X-XSRF-Token"))
                {
                    tokenHeader = headers.GetValues("X-XSRF-Token").FirstOrDefault();
                }

                AntiForgery.Validate(
                    tokenCookie != null ? tokenCookie.Value : null, tokenHeader);
            }

            base.OnActionExecuting(actionContext);
        }
    }
}