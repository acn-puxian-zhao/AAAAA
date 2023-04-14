using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.OData;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "master")]
    public class customerPaymentBankController : ApiController
    {
        
        [HttpGet]
        [EnableQuery]
        public IEnumerable<CustomerPaymentBank> Get()
        {
            CustomerPaymentBankService service = SpringFactory.GetObjectImpl<CustomerPaymentBankService>("CustomerPaymentBankService");
            var cusPaymentBankList = service.CustomerPaymentBankGet();
            return cusPaymentBankList.AsQueryable<CustomerPaymentBank>();
        }

        [HttpPost]
        public void Post([FromBody] CustomerPaymentBank cust)
        {
            try
            {
                CustomerPaymentBankService service = SpringFactory.GetObjectImpl<CustomerPaymentBankService>("CustomerPaymentBankService");
                if (cust.Id == 0)
                {
                    cust.Deal = AppContext.Current.User.Deal.ToString();
                    cust.CreateDate = AppContext.Current.User.Now;
                    cust.UpdateDate = AppContext.Current.User.Now;
                    cust.CreatePersonId = AppContext.Current.User.EID;
                    service.CommonRep.Add(cust);
                }
                else
                {
                    CustomerPaymentBank old = service.CommonRep.FindBy<CustomerPaymentBank>(cust.Id);
                    ObjectHelper.CopyObjectWithUnNeed(cust, old, new string[] { "Id", "CustomerNum" });
                }
                service.CommonRep.Commit();
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message); 
            }
        }

        [HttpGet]
        [EnableQuery]
        public IEnumerable<CustomerPaymentBank> Get(string num)
        {
            CustomerPaymentBankService service = SpringFactory.GetObjectImpl<CustomerPaymentBankService>("CustomerPaymentBankService");
            var cusPaymentBankList = service.GetCustPaymentBank(num);
            return cusPaymentBankList.AsQueryable<CustomerPaymentBank>();
        }

        [HttpGet]
        public CustomerPaymentBank GetCustModel(string type)
        {
            if (type == "new")
            {
                CustomerPaymentBank cust = new CustomerPaymentBank();
                cust.Id = 0;
                return cust;
            }
            else
            {
                return null;
            }
        }

        [HttpPost]
        public void delete(int id)
        {
            try
            {
                CustomerPaymentBankService service = SpringFactory.GetObjectImpl<CustomerPaymentBankService>("CustomerPaymentBankService");
                CustomerPaymentBank old = service.CommonRep.FindBy<CustomerPaymentBank>(id);
                Helper.Log.Info(id);
                if (old != null)
                {
                    service.CommonRep.Remove(old);
                    service.CommonRep.Commit();
                }
            }  catch (Exception ex)
            {
                Helper.Log.Info(ex);
                throw new Exception(ex.Message);         
            }
        }

    }
}