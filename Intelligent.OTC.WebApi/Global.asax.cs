using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.Http;
using System.Web.Routing;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Common;
using System.Timers;
using Intelligent.OTC.Business;
using System.Collections;
using Intelligent.OTC.Domain.Partials;
using System.Net;
using Intelligent.OTC.Business.Collection;
using System.Configuration;
using HibernatingRhinos.Profiler.Appender.EntityFramework;
using Intelligent.OTC.Common.Repository;
using System.Web.Mvc;

namespace Intelligent.OTC.WebApi
{
    public class Global : System.Web.HttpApplication
    {
        public override void Init()
        {
            base.Init();
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            EntityFrameworkExtensionsConfig.Register();
            //automapper
            CustomDtoMapper.Configure();
            DbProfilerConfig.RegisterEntityFrameworkProfiler();
            QuartzConfig.RegisterQuartz();
            CertificateValidationConfig.RegisterCertificateValidation();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling= Newtonsoft.Json.ReferenceLoopHandling.Ignore; ;
            MvcHandler.DisableMvcResponseHeader = true;

        }
        protected void Application_PreSendRequestHeaders(object sender, EventArgs e)
        {
            var app = sender as HttpApplication;
            if (app == null || app.Context == null)
            {
                return;
            }

            // 移除 Server
            app.Context.Response.Headers.Remove("Server");

            //移除X-AspNet-Version，和上面效果一样
            app.Context.Response.Headers.Remove("X-AspNet-Version");

            //移除X-AspNetMvc-Version，和上面效果一样
            app.Context.Response.Headers.Remove("X-AspNetMvc-Version");
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_PostAuthorizeRequest()
        {
            if (HttpContext.Current.Request != null 
                && ((HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath.StartsWith("~/api"))
                || (HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath.StartsWith("~/odata"))))
            {
                bool isDev = false;
                if (bool.TryParse(System.Configuration.ConfigurationManager.AppSettings["IsDev"], out isDev) && isDev)
                {
                    HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.Required);
                    return;
                }

                if (HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath.Contains("~/api/Collection"))
                {
                    HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.ReadOnly);
                }
                else
                {
                    HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.ReadOnly);
                }
            }
        }

        protected void Application_Error(object sender, EventArgs e)
        {
        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {
        }
    }
}