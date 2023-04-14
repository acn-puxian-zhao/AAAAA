using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    public class CashCollectedController : ApiController
    {
        [HttpPost]
        [Route("api/cashCollectedController/GetCashCollectedController")]
        public IQueryable<Object> GetCashCollectedController(int year, int month)
        {
            CashCollectedService service = SpringFactory.GetObjectImpl<CashCollectedService>("CashCollectedService");
            return service.GetCashCollected(year, month);
        }
    }
}