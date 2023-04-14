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
    public class OpenARController : ApiController
    {
        [HttpPost]
        [Route("api/openAR/GetOpenAR")]
        public IQueryable<OpenARDto> GetOpenAR(string region)
        {
            OpenARService service = SpringFactory.GetObjectImpl<OpenARService>("OpenARService");
            return service.GetOpenAR(region);
        }
    }
}