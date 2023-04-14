using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Attr;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.WebApi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    [UserAuthorizeFilter(actionSet: "master")]
    public class CustomerController : ApiController
    {
        public const string strArchiveOneYearSalesKey = "ArchiveOneYearSalesPath";//OneYearSales路径的config保存名

        [HttpGet]
        [PagingQueryable]
        public IEnumerable<CustomerMasterData> Get(string Contacter)
        {
            ICustomerService service = SpringFactory.GetObjectImpl<ICustomerService>("CustomerService");
            var res = service.GetCustMasterData(Contacter);
            int count = res.Count();
            return res.AsQueryable();
        }

        [HttpGet]
        public Customer Get(string num,string siteUseId) {

            //ISoaService soaService = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
            //List<int> invoiceIdList = soaService.GetAlertAutoSendInvoice("sirui.sun", "Arrow", "295", "1023186", "", "1", "Sales", "Zhou, William;Qin, Alan", "001");
            //soaService.GetNewMailInstance("1023186", "", "001", "001", invoiceIdList, "sirui.sun", "Sales", "Zhou, William;Qin, Alan", "CS,Credit Officer,Finance Manager,Collector", ("2020-11-15" == null ? "" : Convert.ToDateTime("2020-11-15").ToString("yyyy-MM-dd")), "NC");
            //return null;
            ICustomerService service = SpringFactory.GetObjectImpl<ICustomerService>("CustomerService");
            Customer cust = service.GetOneCustomer(num, siteUseId);
            return cust;
        }

        [HttpGet]
        public List<Customer> GetCustomerByCustomerNum(string cusNum)
        {
            var paramsList = cusNum.Split(',');
            ICustomerService service = SpringFactory.GetObjectImpl<ICustomerService>("CustomerService");
            return service.GetCustomerByCustomerNum(paramsList[0]);
        }

        [HttpPost]
        [Route("api/customer/saveCustomer")]
        public string saveCustomer(Customer cust)
        {
            CustomerService service = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
            string message = service.UpdateCustMasterData(cust);
            return message;

        }

        [HttpGet]
        public CustomerCommentsDto GetCustomerComments(string id)
        {
            CustomerCommentsDto dto = new CustomerCommentsDto();
            dto.ID = new Guid(id);
            CustomerService service = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
            CustomerCommentsDto cust = service.getCustomerCommentsByID(dto);
            return cust;
        }

        [HttpGet]
        [Route("api/customer/searchCustomerComments")]
        public List<CustomerCommentsDto> SearchCustomerComments(string cusNum)
        {
            var paramsList = cusNum.Split(',');
            CustomerCommentsDto dto = new CustomerCommentsDto();
            dto.CUSTOMER_NUM = paramsList[0];
            dto.SiteUseId = paramsList[1];
            CustomerService service = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
            return service.searchCustomerCommentsByDTO(dto);
        }

        [HttpPost]
        [Route("api/customer/saveCustomerComments")]
        public string saveCustomerComments(CustomerCommentsDto cust)
        {
            //判断数据是否有效
            if (string.IsNullOrEmpty(cust.AgingBucket))
            {
                return "AgingBucket 不能为空!";
            }
            if (cust.PTPAmount == null && cust.PTPDATE == null && string.IsNullOrEmpty(cust.OverdueReason) && string.IsNullOrEmpty(cust.Comments))
            {
                return "PTPAmount | PTPDate | OverdueReason | Comments 不能同时为空!";
            }
            string message = string.Empty;
            CustomerService service = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
            int ret = service.updateCustomerCommentsDto(cust);
            if (ret == -100)
            {
                message = "AgingBucket已经存在！";
            }
            else if (ret > 0)
            {
                message = "";
            }
            else
            {
                message = "更新失败！";
            }
            return message;

        }

        [HttpPost]
        [Route("api/customer/addCustomerComments")]
        public string addCustomerComments(CustomerCommentsDto dto)
        {
            string message = string.Empty;

            //判断数据是否有效
            if (string.IsNullOrEmpty(dto.AgingBucket)) {
                return "AgingBucket 不能为空!";
            }
            if (dto.PTPAmount == null && dto.PTPDATE == null && string.IsNullOrEmpty(dto.OverdueReason) && string.IsNullOrEmpty(dto.Comments))
            {
                return "PTPAmount | PTPDate | OverdueReason | Comments 不能同时为空!";
            }

            CustomerService service = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
            List<CustomerCommentsDto> dtos = new List<CustomerCommentsDto>();
            dtos.Add(dto);
            int ret = service.addCustomerCommentsDto(dtos);
            if (ret == -100)
            {
                message = "AgingBucket已经存在！";
            }
            else if (ret > 0)
            {
                message = "";
            }
            else
            {
                message = "添加失败！";
            }
            return message;

        }

        [HttpPost]
        [Route("api/customer/delCustomerComments")]
        public string delCustomerComments(string id)
        {
            string message = string.Empty;
            CustomerCommentsDto dto = new CustomerCommentsDto();
            dto.ID = new Guid(id);
            CustomerService service = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
            int ret = service.updateCustomerCommentsDto(dto,true);
            if (ret>0)
            {
                message = "删除成功！";
            }
            else
            {
                message = "删除失败！";
            }
            return message;

        }

        [HttpPost]
        [Route("api/customer/delCustomer")]
        public string delCustomer(Customer cust)
        {
            CustomerService service = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
            string message = service.delCustMasterData(cust);
            return message;

        }

        [HttpPost]
        public string ExpoertCust(string custnum, string custname, string status, string collector,
                                    string begintime, string endtime, string miscollector, string misgroup, 
                                     string billcode,string country,string siteUseId, string legalEntity, string ebName)
        {
            CustomerService service = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
            return service.ExportCustomer(custnum, custname, status, collector,  begintime, endtime,
                                miscollector, misgroup, billcode,country, siteUseId, legalEntity, ebName);
        }

        [HttpPost]
        [Route("api/customer/exportComment")]
        public string ExpoertComment()
        {
            CustomerService service = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
            return service.ExpoertComment();
        }

        [HttpPost]
        [Route("api/customer/exportCommentSales")]
        public string ExpoertCommentSales()
        {
            CustomerService service = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
            return service.ExpoertCommentSales();
        }



        [HttpPost]
        public string ImportCust(string type)
        {
            var returnResult = "";
            if (type.ToLower() == "import")
            {
                CustomerService service = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
                returnResult = service.ImportCustHistory();
            }
            else if (type.ToLower() == "importpayment")
            {
                CustomerService service = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
                returnResult = service.ImportCustPayment();
            }
            else if (type.ToLower() == "importcontactor")
            {
                CustomerService service = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
                returnResult = service.ImportCustContactor();
            }
            else if (type.ToLower() == "importcomment")
            {
                CustomerService service = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
                returnResult = service.ImportCustComment();
            }
            else if (type.ToLower() == "importebbranch")
            {
                CustomerService service = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
                returnResult = service.ImportEBBranch();
            }
            else if (type.ToLower() == "importlitigation")
            {
                CustomerService service = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
                returnResult = service.ImportLitigation();
            }
            else if (type.ToLower() == "importbaddebt")
            {
                CustomerService service = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
                returnResult = service.ImportBadDebt();
            }
            else if (type.ToLower() == "importcommentfromcssales")
            {
                CustomerService service = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
                returnResult = service.ImportCustCommentSales();
            }

            return returnResult;
        }

        [HttpPost]
        public string Post([FromBody] Customer cust)
        {
            CustomerService service = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
            string message = service.AddCustMasterData(cust);
            return message;
        }

        [HttpGet]
        public Customer GetCustModel(string type)
        {
            if (type == "new")
            {
                Customer cust = new Customer();
                cust.Id = 0;
                return cust;
            }
            else {
                return null;
            }
        }

      
        [HttpPost]
        public void FinishSoa(string num, string site) 
        { 
        
        }

        [HttpPost]
        [Route("api/Customer/UploadCustomerLocalize")]
        public string UploadCustomerLocalize()
        {
            CustomerService service = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
            string result = service.ImportCustLocalize();
            return result;
        }

        [HttpPost]
        [Route("api/Customer/UploadCreditHold")]
        public string UploadCreditHold()
        {
            CustomerService service = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
            string result = service.ImportCreditHold();
            return result;
        }

        [HttpPost]
        [Route("api/Customer/UploadCurrencyAmount")]
        public string UploadCurrencyAmount()
        {
            CustomerService service = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
            string result = service.ImportCurrencyAmount();
            return result;
        }

        [HttpPost]
        [Route("api/Customer/UploadTWCurrencyAmount")]
        public string UploadTWCurrencyAmount()
        {
            CustomerService service = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
            string result = service.ImportTWCurrencyAmount();
            return result;
        }

        [HttpPost]
        [Route("api/Customer/UploadATMCurrencyAmount")]
        public string UploadATMCurrencyAmount()
        {
            CustomerService service = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
            string result = service.ImportATMCurrencyAmount();
            return result;
        }

        [HttpPost]
        [Route("api/Customer/UploadVatOnly")]
        public string UploadVatOnly()
        {
            CustomerService service = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
            string result = service.ImportVatOnly();
            return result;
        }

        [HttpPost]
        [Route("api/Customer/UploadInvoiceDetailOnly")]
        public string UploadInvoiceDetailOnly()
        {
            CustomerService service = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
            string result = service.ImportInvoiceDetailOnly();
            return result;
        }

        [HttpPost]
        [Route("api/Customer/UploadSAPInvoiceOnly")]
        public string UploadSAPInvoiceOnly()
        {
            CustomerService service = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
            string result = service.ImportSAPInvoiceOnly();
            return result;
        }

        [HttpPost]
        [Route("api/Customer/UploadVarData")]
        public string UploadVarData()
        {
            CustomerService service = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
            string result = service.ImportVarDataOnly();
            return result;
        }

        [HttpPost]
        [Route("api/Customer/UploadConsignmentNumber")]
        public string UploadConsignmentNumber()
        {
            CustomerService service = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
            string result = service.ImportConsigmentNumber();
            return result;
        }

        [HttpGet]
        [PagingQueryable]
        public IEnumerable<CustomerMasterDto> GetAssign(string customers)
        {
            ICustomerService service = SpringFactory.GetObjectImpl<ICustomerService>("CustomerService");
            var res = service.GetCustMasterDataForAssign(customers);

            return res.AsQueryable();
        }
    }
}