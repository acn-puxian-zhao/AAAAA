using Intelligent.OTC.Business.Interfaces;
using Intelligent.OTC.Common;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.DomainModel;
using Intelligent.OTC.Domain.Dtos;
using System.Collections.Generic;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{

    public class CustomerAssessmentController : ApiController
    {
        [HttpPost]
        [Route("api/customerAssessment/getCustomerAssessment")]
        public CustomerAssessmentModel getCustomerAssessment(VCustomerAssessmentDto vCustomerAssessmentDto)
        {
            ICustomerAssessmentService service = SpringFactory.GetObjectImpl<ICustomerAssessmentService>("CustomerAssessmentService");
            var res = service.getCustomerAssessment(vCustomerAssessmentDto);
            return res;
        }

        [HttpPost]
        [Route("api/customerAssessment/getCustomerAssessmentCount")]
        public int getCustomerAssessmentCount()
        {
            ICustomerAssessmentService service = SpringFactory.GetObjectImpl<ICustomerAssessmentService>("CustomerAssessmentService");
            return service.getCustomerAssessmentCount();
        }

        [HttpPost]
        [Route("api/customerAssessment/updateCustomerAssessment")]
        public string updateCustomerAssessment(List<T_CustomerAssessment> caList)
        {
            if (caList == null || caList.Count == 0)
            {
                return "no data for update";
            }
            ICustomerAssessmentService service = SpringFactory.GetObjectImpl<ICustomerAssessmentService>("CustomerAssessmentService");
            for (int i = 0; i < caList.Count; i++)
            {
                if (!service.updateCustomerAssessmentCount(caList[i]))
                {
                    return "update faild!";
                }
            }
            return "update success";
        }
        [HttpPost]
        [Route("api/customerAssessment/exportCustomerAssessment")]
        public string exportCustomerAssessment(VCustomerAssessmentDto vCustomerAssessmentDto)
        {
            ICustomerAssessmentService service = SpringFactory.GetObjectImpl<ICustomerAssessmentService>("CustomerAssessmentService");
            return service.exportCustomerAssessment(vCustomerAssessmentDto);
        }
    }
}
