using Intelligent.OTC.Common;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.WebApi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Intelligent.OTC.Business.Collection;

namespace Intelligent.OTC.WebApi.Controllers
{
    public class AssessmentTypeController : ApiController
    {
        [HttpGet]
        public IEnumerable<T_AssessmentType> Get()
        {
            CollectionService service = SpringFactory.GetObjectImpl<CollectionService>("CollectionService");
            var res = service.GetAllAssessmentType();
            return res.AsQueryable();
        }
    }
}
