using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.Utils;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace Intelligent.OTC.WebApi.Core
{
    public class WebApiExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            var message = string.Empty;

            if(context.Exception.GetType() == typeof(OTCServiceException))
            {
                message = context.Exception.Message;

                context.Response = new HttpResponseMessage() { StatusCode = (context.Exception as OTCServiceException).StatusCode, Content = new StringContent(message) };
            }
            else if (context.Exception.GetType() == typeof(UserNotLoginException))
            {
                message = context.Exception.Message;

                //Redirect(ConfigurationManager.AppSettings["Xccelerator"] + "/User/Logout");
                context.Response = new HttpResponseMessage() { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent(message) };
            }
            else if (context.Exception.GetType() == typeof(NoPermissionException))
            {
                message = context.Exception.Message;

                context.Response = new HttpResponseMessage() { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent(message) };
            }
            else
            {
                message = "Error happened inside the server. Please contact administrator.";

                context.Response = new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.InternalServerError, Content = new StringContent(message) };
            }

            Helper.Log.Error(context.Exception.Message, context.Exception);
            
            base.OnException(context);
        }

        public override Task OnExceptionAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            Helper.Log.Error(actionExecutedContext.Exception.Message, actionExecutedContext.Exception);
            return base.OnExceptionAsync(actionExecutedContext, cancellationToken);
        }
    }
}
