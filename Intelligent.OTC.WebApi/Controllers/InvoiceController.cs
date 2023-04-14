using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.WebApi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "common")]
    public class InvoiceController : ApiController
    {
        private string strDeal = AppContext.Current.User.Deal.ToString();
        // Get CustomerAging Single
        [HttpGet]
        public CustomerDetail Get(int id)
        {
            CustomerService service = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
            CustomerDetail cust = service.GetCustomerDetail(id);
            return cust;
        }

        [HttpGet]
        public List<InvoiceAging> GetBy(string num)
        {
            InvoiceService service = SpringFactory.GetObjectImpl<InvoiceService>("InvoiceService");
            return service.invoiceInfoGetByNum(num);
        }


        [HttpPut]
        public void Put([FromBody]InvoiceAging invoice)
        {
            InvoiceService service = SpringFactory.GetObjectImpl<InvoiceService>("InvoiceService");
            InvoiceAging old = service.CommonRep.FindBy<InvoiceAging>(invoice.Id);
            ObjectHelper.CopyObjectWithUnNeed(invoice, old, new string[] { "Id" });
            service.CommonRep.Commit();
        }

        #region GetInvoiceList
        [HttpGet]
        [PagingQueryable]
        public IEnumerable<InvoiceAging> Get()
        {
            InvoiceService service = SpringFactory.GetObjectImpl<InvoiceService>("InvoiceService");
            return service.invoiceInfoGet().AsQueryable<InvoiceAging>();
        }
        #endregion

        [HttpGet]
        public IEnumerable<InvoiceAging> Get(string num, string site)
        {
            InvoiceService service = SpringFactory.GetObjectImpl<InvoiceService>("InvoiceService");
            List<InvoiceAging> invoice = new List<InvoiceAging>();
            invoice = service.invoiceInfoGet().ToList<InvoiceAging>();
            invoice = invoice.FindAll(m => m.CustomerNum == num && m.Deal == strDeal && m.LegalEntity == site);

            return invoice.AsQueryable<InvoiceAging>();
        }

        [HttpGet]
        public IEnumerable<InvoiceAging> Get(string num, string site, string act, string cls)
        {
            if (act == "collectorsoa")
            {
                InvoiceService service = SpringFactory.GetObjectImpl<InvoiceService>("InvoiceService");
                List<InvoiceAging> invoice = new List<InvoiceAging>();
                invoice = service.invoiceInfoGet().ToList<InvoiceAging>();
                invoice = invoice.FindAll(m => m.CustomerNum == num && m.Deal == strDeal && m.LegalEntity == site && m.Class == cls);

                return invoice.AsQueryable<InvoiceAging>();
            }
            else
            {
                List<InvoiceAging> invoice = new List<InvoiceAging>();
                return invoice;
            }
        }

        [HttpGet]
        public void UpdateInvoiceState(string invids, string status, string act)
        {
            if (act == "updatestatus")
            {
                InvoiceService service = SpringFactory.GetObjectImpl<InvoiceService>("InvoiceService");

                string[] arr = invids.Split(',');
                foreach (var item in arr)
                {
                    InvoiceAging old = service.CommonRep.FindBy<InvoiceAging>(Convert.ToInt32(item));
                    InvoiceAging invoice = old;
                    invoice.States = status;
                    ObjectHelper.CopyObjectWithUnNeed(invoice, old, new string[] { "Id" });
                }
                service.CommonRep.Commit();
            }
        }

        [HttpGet]
        [Route("api/Invoice/overduereason")]
        public OverdueReasonDto GetOverdueReason(string invoiceNum)
        {
            OverdueReasonDto result = new OverdueReasonDto();
            result.InvoiceNums = new List<string>();

            string[] nums = invoiceNum.Split(',');
            result.InvoiceNums.AddRange(nums);

            if (nums.Length == 1)
            {
                InvoiceService service = SpringFactory.GetObjectImpl<InvoiceService>("InvoiceService");
                var invoiceInfo = service.GetInvoiceInfo(invoiceNum);
                if (invoiceInfo != null)
                {
                    result.Reason = invoiceInfo.OverdueReason;
                }
            }

            return result;
        }

        [HttpPost]
        [Route("api/Invoice/overduereason")]
        public void SaveOverdueReason(OverdueReasonDto dto)
        {
            InvoiceService service = SpringFactory.GetObjectImpl<InvoiceService>("InvoiceService");
            service.UpdateOverdueReason(dto.InvoiceNums, dto.Reason, dto.Comments);
        }

        [HttpPost]
        [Route("api/Invoice/clearPTP")]
        public string clearPTP(List<string> idList)
        {
            InvoiceService service = SpringFactory.GetObjectImpl<InvoiceService>("InvoiceService");
            return service.clearPTP(idList);
        }

        [HttpPost]
        [Route("api/Invoice/setNotClear")]
        public string setNotClear(List<string> idList)
        {
            InvoiceService service = SpringFactory.GetObjectImpl<InvoiceService>("InvoiceService");
            List<string> idListFact = new List<string>();
            for (int i = 1; i < idList.Count; i++)
            {
                idListFact.Add(idList[i]);
            }
            return service.setNotClear(idList[0], idListFact);
        }

        [HttpPost]
        [Route("api/Invoice/clearOverdueReason")]
        public string clearOverdueReason(List<string> idList)
        {
            InvoiceService service = SpringFactory.GetObjectImpl<InvoiceService>("InvoiceService");
            return service.clearOverdueReason(idList);
        }

        [HttpPost]
        [Route("api/Invoice/clearComments")]
        public string clearComments(List<string> idList)
        {
            InvoiceService service = SpringFactory.GetObjectImpl<InvoiceService>("InvoiceService");
            return service.clearComments(idList);
        }

        [HttpPost]
        [Route("api/invoice/exportfiles")]
        public string Post([FromBody]List<int> intIds, string customerNum, string siteUseId, string fileType)
        {
            InvoiceService service = SpringFactory.GetObjectImpl<InvoiceService>("InvoiceService");
            string fileid=service.exportSoafiles(intIds,  customerNum,  siteUseId, fileType);
            return fileid;
        }
    }
}