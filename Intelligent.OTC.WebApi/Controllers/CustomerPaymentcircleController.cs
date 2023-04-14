using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.WebApi.Core;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;


namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "master")]
    public class CustomerPaymentcircleController : ApiController
    {

        [HttpGet]
        [PagingQueryable]
        public IEnumerable<CustomerPaymentCircle> Get()
        {
            CustomerPaymentCircleService service = SpringFactory.GetObjectImpl<CustomerPaymentCircleService>("CustomerPaymentCircleService");
            var cusPaymentcircleList = service.GetCustomerPaymentCircle();
            return cusPaymentcircleList.AsQueryable<CustomerPaymentCircle>();
        }


       [HttpGet]
        public IEnumerable<CustomerPaymentCircle> Get(string num)
        {
            string custNum = "";
            string siteUseId = "";
            if (num.Equals("newCust"))
            {
                custNum = "";
                siteUseId = "";
            }
            else
            {
                string[] paramsList = num.Split(',');
                custNum = paramsList[0];
                siteUseId = paramsList[1];
            }
            
            CustomerPaymentCircleService service = SpringFactory.GetObjectImpl<CustomerPaymentCircleService>("CustomerPaymentCircleService");
            var cusPaymentcircleList = service.GetCustPaymentCircle(custNum, siteUseId);
            List<CustomerPaymentCircleP> CustomerPaymentcircleList = new List<CustomerPaymentCircleP>();
            CustomerPaymentCircleP cuspaycircle = new CustomerPaymentCircleP();
            int ind = 0;
            foreach (var item in cusPaymentcircleList) {
                ind++;
                cuspaycircle = new CustomerPaymentCircleP();
                cuspaycircle.Id = item.Id;
                cuspaycircle.PaymentDay = item.PaymentDay;
                cuspaycircle.Reconciliation_Day = item.Reconciliation_Day;
                cuspaycircle.weekDay = (item.PaymentDay.HasValue ? item.PaymentDay.Value.DayOfWeek.ToString() : item.Reconciliation_Day.Value.DayOfWeek.ToString());
                cuspaycircle.Flg = item.Flg;
                cuspaycircle.Description = item.Description;
                cuspaycircle.CustomerNum = item.CustomerNum;
                cuspaycircle.CreatePersonId = item.CreatePersonId;
                cuspaycircle.CreateDate = item.CreateDate;
                cuspaycircle.LegalEntity = item.LegalEntity;
                cuspaycircle.sortId = ind;
                CustomerPaymentcircleList.Add(cuspaycircle);
            }
            return CustomerPaymentcircleList.AsQueryable<CustomerPaymentCircle>();
        }


        [HttpPost]
        public string Post([FromBody] List<string> pay)
        {
            CustomerPaymentCircleService service = SpringFactory.GetObjectImpl<CustomerPaymentCircleService>("CustomerPaymentCircleService");
            return service.AddPamentCircle(pay);
        }

        [HttpPost]
        [Route("api/CustomerPaymentcircle/deleteAll")]
        public string deleteAll(string customerNum, string siteUseId)
        {
            CustomerPaymentCircleService service = SpringFactory.GetObjectImpl<CustomerPaymentCircleService>("CustomerPaymentCircleService");
            return service.DelAllPamentCircle(customerNum, siteUseId);
        }


        [HttpPost]
        public string UploadCircle(string customerNum,string siteUseId, string legal)
        {
                CustomerPaymentCircleService service = SpringFactory.GetObjectImpl<CustomerPaymentCircleService>("CustomerPaymentCircleService");
                return service.AddUploadCircle(customerNum, siteUseId, legal);
        }


        [HttpPost]
        public void delete(int id)
        {
            CustomerPaymentCircleService service = SpringFactory.GetObjectImpl<CustomerPaymentCircleService>("CustomerPaymentCircleService");
            CustomerPaymentCircle old = service.CommonRep.FindBy<CustomerPaymentCircle>(id);
            if (old != null)
            {
                service.CommonRep.Remove(old);
                service.CommonRep.Commit();
            }
        }

        [HttpGet]
        [PagingQueryable]
        public IQueryable<CustomerPaymentCircle> GetLegalEntity(string custNum, string legal)
        {
            string custNumber = "";
            string siteUseId = "";
            if (custNum.Equals("newCust"))
            {

            }
            else
            {
                string[] paramsList = custNum.Split(',');
                custNumber = paramsList[0];
                siteUseId = paramsList[1];
            }
            CustomerPaymentCircleService service = SpringFactory.GetObjectImpl<CustomerPaymentCircleService>("CustomerPaymentCircleService");
            var cusPaymentcircleList = service.GetCircleByCondtion(custNumber, siteUseId, legal);
            return cusPaymentcircleList.AsQueryable<CustomerPaymentCircle>();
        }
    }
}