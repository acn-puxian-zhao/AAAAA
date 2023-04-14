using Intelligent.OTC.Business.Interfaces;
using Intelligent.OTC.Common;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.DomainModel;
using Intelligent.OTC.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{

    public class CaPTPController : ApiController
    {
        [HttpGet]
        [Route("api/caPTPController/getCaPTPList")]
        public CaPTPDtoPage getCaPTPList(string customerNum,string legalEntity, string customerCurrency, string invCurrency, string amt, string localAmt, string ptpDateF, string ptpDateT)
        {
            ICaPTPService service = SpringFactory.GetObjectImpl<ICaPTPService>("CaPTPService");
            var res = service.getCaPTPList(customerNum, legalEntity,customerCurrency, invCurrency, amt, localAmt, ptpDateF, ptpDateT);
            return res; 
        }

        
    }
}
