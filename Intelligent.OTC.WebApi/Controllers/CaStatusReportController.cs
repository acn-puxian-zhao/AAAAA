using Intelligent.OTC.Business.Interfaces;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.DomainModel;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Net.Http;

namespace Intelligent.OTC.WebApi.Controllers
{

    public class CaStatusReportController : ApiController
    {
        [HttpGet]
        [Route("api/statusReport/getStatusReport")]
        public List<CaStatusReportDto> getStatusReport(string valueDateF, string valueDateT)
        {
            ICaStatusReportService service = SpringFactory.GetObjectImpl<ICaStatusReportService>("CaStatusReportService");
            var res = service.getStatusReport(valueDateF, valueDateT);
            return res;
        }

        [HttpGet]
        [Route("api/statusReport/exporAll")]
        public HttpResponseMessage ExporAll(string valueDateF, string valueDateT)
        {
            ICaStatusReportService service = SpringFactory.GetObjectImpl<ICaStatusReportService>("CaStatusReportService");
            return service.exporAll(valueDateF, valueDateT);
        }
    }
}
