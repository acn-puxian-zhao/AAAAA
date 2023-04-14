using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    public class DisputReasonController : ApiController
    {
        [HttpPost]
        [Route("api/disputReason/GetDisputReason")]
        public IQueryable<DisputReasonDto> GetDisputReason(string region)
        {
            DisputReasonService service = SpringFactory.GetObjectImpl<DisputReasonService>("DisputReasonService");
            return service.GetDisputReason(region);
        }
    }
}