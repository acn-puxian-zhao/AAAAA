using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;

namespace Intelligent.OTC.WebApi.Core
{
    public static class PreRouteHandler
    {
        public static void HttpPreRoute(this HttpConfiguration config)
        {
            config.Services.Replace(typeof(IHttpActionInvoker), new HttpWebApiControllerActionInvoker());
            config.Services.Replace(typeof(IHttpControllerSelector), new HttpNotFoundDefaultHttpControllerSelector(config));
            config.Services.Replace(typeof(IHttpActionSelector), new HttpNotFoundControllerActionSelector());
        }
    }
}