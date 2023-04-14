using Intelligent.OTC.Business.Interfaces;
using Intelligent.OTC.Common;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.WebApi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    public class CustomerAssessmentLogController : ApiController
    {

        [HttpGet]
        [PagingQueryable]
        public IEnumerable<LegalEntityAssessmentDateDto> Get(string assessmentLogDate)
        {
            if (string.IsNullOrEmpty(assessmentLogDate)) { return null; }
            ICustomerAssessmentLogService service = SpringFactory.GetObjectImpl<ICustomerAssessmentLogService>("CustomerAssessmentLogService");
            var res = service.GetAllCustomerAssessmentLog(assessmentLogDate);
            return res.AsQueryable();
        }

        [HttpGet]
        [PagingQueryable]
        public IEnumerable<DateTime> Get()
        {
            ICustomerAssessmentLogService service = SpringFactory.GetObjectImpl<ICustomerAssessmentLogService>("CustomerAssessmentLogService");
            var res = service.GetAllAssessmentDate();
            return res.AsQueryable();
        }

        [HttpPost]
        [Route("api/customerAssessmentLog/getAssessmentLogCount")]
        public int GetAssessmentLogCount()
        {
            ICustomerAssessmentLogService service = SpringFactory.GetObjectImpl<ICustomerAssessmentLogService>("CustomerAssessmentLogService");
            return service.GetAssessmentLogCount();
        }
    }
}
