using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.Mvc;

namespace Intelligent.OTC.MailService
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes) 
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { id = UrlParameter.Optional }
            );

            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(HttpContext.Current.Server.MapPath("log4net.config")));
        }
    }
}