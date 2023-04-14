using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.WebApi.Core;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Transactions;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "allinfo")]
    public class allinfoController : ApiController
    {
        [HttpGet]
        [PagingQueryable]
        public IQueryable<T_ALLACCOUNT_TMP> Get(string isPTPOverDue)
        {
            ContactindexService service = SpringFactory.GetObjectImpl<ContactindexService>("ContactindexService");
            return service.getAllInvoiceByUserForArrow(isPTPOverDue);
        }

        [HttpGet]
        public IEnumerable<SendSoaHead> CreateSoa(string ColSoa)
        {
            ContactindexService service = SpringFactory.GetObjectImpl<ContactindexService>("ContactindexService");
            return service.CreateSendMailForArrow(ColSoa).AsQueryable();
        }

        [HttpGet]
        public IEnumerable<ContactHistory> GetContactHistory(string CustNumsFCH)
        {
            ContactindexService service = SpringFactory.GetObjectImpl<ContactindexService>("ContactindexService");
            return service.GetContactHistory(CustNumsFCH);
        }

        [HttpGet]
        public HttpResponseMessage ExpoertInvoiceList(string cCode, string cName, string level, string bCode, string bName, string legal,
                                                       string state, string tstate, string iNum, string pNum, string sNum, string memo,string oper)                                              
        {
            ContactindexService service = SpringFactory.GetObjectImpl<ContactindexService>("ContactindexService");
            return service.ExportAccountList(cCode, cName, level, bCode, bName, legal, 
                                              state, tstate, iNum, pNum, sNum, memo,oper);
        }

        [HttpPost]
        [Route("api/PTPPayment/query")]
        public List<T_PTPPayment> getPTPPayment(string custNum, string siteUseID, int pageindex, int pagesize)
        {
            ContactindexService service = SpringFactory.GetObjectImpl<ContactindexService>("ContactindexService");
           
            var result = service.getPTPPayment(custNum, siteUseID);
            var list = result.OrderByDescending(p => p.Id).Skip((pageindex - 1) * pagesize).Take(pagesize).ToList();
            if (list != null && list.Count > 0)
            {
                list[0].CollectorId = result.Count().ToString();
            }
            return list;
        }

        [HttpPost]
        [Route("api/PTPPayment/getPayer")]
        public List<string> getPayer(string siteUseID)
        {
            ContactindexService service = SpringFactory.GetObjectImpl<ContactindexService>("ContactindexService");
            return service.getPayerList(siteUseID); ;
        }

        [HttpPost]
        [Route("api/PTPPayment/update")]
        public string updatePTPPayment(T_PTPPayment model)
        {
            ContactindexService service = SpringFactory.GetObjectImpl<ContactindexService>("ContactindexService");
            if (service.updatePTPPayment(model))
            {
                return "update success";
            }
            else
            {
                return "update fail";
            }
            
        }

    }
}