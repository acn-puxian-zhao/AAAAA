using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.WebApi.Core;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "dataprepare")]
    public class InitAgingController : ApiController
    {

        [HttpGet]
        [PagingQueryable]
        public IEnumerable<CustomerAgingStaging> Get()
        {
            ICustomerService service = SpringFactory.GetObjectImpl<ICustomerService>("CustomerService");
            return service.GetCustomerAgingStaging().AsQueryable<CustomerAgingStaging>();
        }

        [HttpGet]
        [PagingQueryable]
        public IEnumerable<CustomerGroupCfgStaging> GetOneYears(string type)
        {
            ICustomerService service = SpringFactory.GetObjectImpl<ICustomerService>("CustomerService");
            return service.GetGroupStaing().AsQueryable<CustomerGroupCfgStaging>();
        }

        [HttpPut]
        public void Put([FromBody]CustomerAgingStaging cust)
        {
            ICustomerService service = SpringFactory.GetObjectImpl<ICustomerService>("CustomerService");
            CustomerAgingStaging old = service.CommonRep.FindBy<CustomerAgingStaging>(cust.Id);
            ObjectHelper.CopyObjectWithUnNeed(cust, old, new string[] { "Id" });
            service.CommonRep.Commit();
        }

        [HttpPost]
        public void Post([FromBody]List<int> custIds)
        {
            ICustomerService service = SpringFactory.GetObjectImpl<ICustomerService>("CustomerService");
            service.DeleteCustomerAging(custIds);
        }

        //***************************************

        [HttpGet]
        public void Submit(string custIds)
        {
            ICustomerService service = SpringFactory.GetObjectImpl<ICustomerService>("CustomerService");
            if (custIds == "1")
            {
                service.SubmitInitialAgingNew(AppContext.Current.User.Deal);
            }
            else if (custIds == "2")
            {
                service.SubmitInitialVAT(AppContext.Current.User.Deal);
            }
            else if (custIds == "3")
            {
                service.SubmitInitialInvDet(AppContext.Current.User.Deal);
            }
            else if (custIds == "6")
            {
                service.SubmitInitialSAPAging(AppContext.Current.User.Deal);
            }
            else
            {
                service.SubmitOneYearSales();
            }
        }

        [HttpGet]
        public List<UploadInfo> Get(int para1, int para2)
        {
            int iC;
            string strDeal = AppContext.Current.User.Deal.ToString();
            List<FileUploadHistory> fileHis = new List<FileUploadHistory>();
            List<Sites> sites = new List<Sites>();

            PeroidService service = SpringFactory.GetObjectImpl<PeroidService>("PeroidService");
            service.getListInfo(strDeal, out fileHis, out sites);
            List<UploadInfo> lst = service.getCurrentPeroidUploadTimes(out iC, fileHis, sites);

            UploadInfo info = new UploadInfo();
            info.LegalEntity = "";
            info.AccTimes = null;
            info.OneYearTimes = iC;
            lst.Add(info);

            return lst;

        }
    }
}