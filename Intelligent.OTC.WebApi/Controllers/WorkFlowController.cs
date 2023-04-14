using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "common")]
    public class WorkFlowController : ApiController
    {
        [HttpPost]
        public void Wfchange(string referenceNo, string status,string type) {
            IWorkflowService service = SpringFactory.GetObjectImpl<IWorkflowService>("WorkflowService");
            
            if (type == "start")
            {
                service.Wfchange("4", referenceNo, type);
            }
            else
            {
                service.Wfchange("4", referenceNo, type);
            }
            
        }
    }
}