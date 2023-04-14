using Intelligent.OTC.Business.Interfaces;
using Intelligent.OTC.Common;
using Intelligent.OTC.Domain.DomainModel;
using Intelligent.OTC.Domain.Dtos;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    public class CustomerAssessmentHistoryController : ApiController
    {
        [HttpPost]
        [Route("api/customerAssessmentHistory/getCustomerAssessmentHistory")]
        public CustomerAssessmentModel getCustomerAssessmentHistory(VCustomerAssessmentDto vCustomerAssessmentDto)
        {
            ICustomerAssessmentHistoryService service = SpringFactory.GetObjectImpl<ICustomerAssessmentHistoryService>("CustomerAssessmentHistoryService");
            var res = service.getCustomerAssessmentHistory(vCustomerAssessmentDto);
            return res;
        }
        [HttpPost]
        [Route("api/customerAssessmentHistory/exportCustomerAssessmentHistory")]
        public string exportCustomerAssessmentHistory(VCustomerAssessmentDto vCustomerAssessmentDto)
        {
            ICustomerAssessmentHistoryService service = SpringFactory.GetObjectImpl<ICustomerAssessmentHistoryService>("CustomerAssessmentHistoryService");
            
            return service.exportCustomerAssessmentHistory(vCustomerAssessmentDto);
        }
    }
}
