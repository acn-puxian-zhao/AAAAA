using Intelligent.OTC.Business.Interfaces;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    public class CustomerAccountPeriodController : ApiController
    {
        [HttpPost]
        [Route("api/customerAccountPeriod/GetByNumAndSiteUseId")]
        public IQueryable<T_Customer_AccountPeriod> GetByNumAndSiteUseId(T_Customer_AccountPeriod customerAccountPeriod)
        {
            ICustomerAccountPeriodService service = SpringFactory.GetObjectImpl<ICustomerAccountPeriodService>("CustomerAccountPeriodService");
            return service.GetByNumAndSiteUseId(customerAccountPeriod);
        }

        [HttpPost]
        [Route("api/customerAccountPeriod/SaveOrUpdateAccountPeriod")]
        public string SaveAccountPeriod(T_Customer_AccountPeriod customerAccountPeriod)
        {
            HttpRequest request = HttpContext.Current.Request;
            string isAdd = request["isAdd"];
            ICustomerAccountPeriodService service = SpringFactory.GetObjectImpl<ICustomerAccountPeriodService>("CustomerAccountPeriodService");
            return service.SaveAccountPeriod(customerAccountPeriod, isAdd);
        }

        [HttpPost]
        public void delete(int id)
        {
            ICustomerAccountPeriodService service = SpringFactory.GetObjectImpl<ICustomerAccountPeriodService>("CustomerAccountPeriodService");
            Helper.Log.Info(id);
            service.DeleteAccountPeriod(id);
        }

        [HttpPost]
        public string ImportAP(string type)
        {
            ICustomerAccountPeriodService service = SpringFactory.GetObjectImpl<ICustomerAccountPeriodService>("CustomerAccountPeriodService");
            return service.ImportAccountPeriod();
        }
    }
}
