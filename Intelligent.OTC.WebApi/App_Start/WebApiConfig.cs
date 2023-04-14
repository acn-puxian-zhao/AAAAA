using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Web;
using Intelligent.OTC.WebApi.Core;
using Microsoft.OData.Edm;
using System.Web.OData.Builder;
using Intelligent.OTC.Domain.DataModel;
using System.Web.OData.Routing;
using System.Web.OData.Extensions;
using System.Web.Http;
using Intelligent.OTC.Domain.Proxy.Permission;
using System.Net.Http.Headers;
using Intelligent.OTC.Common.Attr;
using System.Web.Http.Filters;

namespace Intelligent.OTC.WebApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes(); 

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional, action = RouteParameter.Optional }
            );
            //config.HttpPreRoute();

            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            

            ODataRoute route = config.MapODataServiceRoute("odata", "odata", GetModel());
            
            config.Filters.Add(new WebApiExceptionFilter());
            config.Filters.Add(new UserAuthorizeFilterAttribute());
            //config.Filters.Add(new AntiSqlInjectFilter());

            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(HttpContext.Current.Server.MapPath("log4net.config")));

        }

        public static IEdmModel GetModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();

            builder.EntitySet<UserPermission>("permission");
            builder.EntitySet<CustomerMasterData>("customer");
            builder.EntitySet<V_CustomerAssessment>("customerAssessment");
            builder.EntitySet<CustomerAging>("collection");
            builder.EntitySet<SoaDto>("collectorSoa");
            builder.EntitySet<ContactCustomerDto>("contactCustomer");
            builder.EntitySet<ContactHistory>("contactHistory");
            builder.EntitySet<DisputeTrackingView>("disputeTracking");
            builder.EntitySet<CustomerAgingStaging>("initAging");
            builder.EntitySet<CustomerGroupCfgStaging>("customerGroupCfg"); //
            builder.EntitySet<InvoiceAging>("invoice");
            builder.EntitySet<MailTmp>("mail");
            builder.EntitySet<PeroidReport>("peroid");
            builder.EntitySet<CustomerCommon>("breakPtp");
            builder.EntitySet<HoldCustomerView>("holdCustomer");
            builder.EntitySet<UnHoldCustomer>("unholdCustomer");
            builder.EntitySet<DunningReminderDto>("dunning");
            builder.EntitySet<CustomerPaymentCircle>("customerPaymentcircle");
            builder.EntitySet<AllAccountInfo>("allinfo");
            builder.EntitySet<FileDownloadHistory>("dailyReport");
            builder.EntitySet<MyinvoicesDto>("myinvoices");
            
            return builder.GetEdmModel();
        }
    }
}
