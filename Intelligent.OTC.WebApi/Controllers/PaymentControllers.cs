using Intelligent.OTC.Business.Interfaces;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    public class PaymentControllers : ApiController
    {
        [HttpPost]
        [Route("api/paymentController/savePMT")]
        public string savePMT(CaPMTDto caPMT)
        {
            string strMessage = string.Empty;
            try
            {
                IPaymentDetailService service = SpringFactory.GetObjectImpl<IPaymentDetailService>("PaymentDetailService");



                strMessage = "Save succes.";
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new OTCServiceException("Save faild." + ex.Message);
            }
            return strMessage;
        }

        [HttpGet]
        [Route("api/paymentController/queryPMTByID")]
        public CaPMTDto queryPMTByID(string pmtID)
        {
            return null;
        }

        [HttpGet]
        [Route("api/paymentController/GetBankStatementByTranINC")]
        public CaBankStatementDto GetBankStatementByTranINC(string transactionNumber, string menuregion)
        {
            IPaymentDetailService service = SpringFactory.GetObjectImpl<IPaymentDetailService>("PaymentDetailService");
            return service.GetBankStatementByTranINC(transactionNumber, menuregion);
        }

        [HttpGet]
        [Route("api/paymentController/GetInvoiceInfoByNum")]
        public InvoiceAgingDto GetInvoiceInfoByNum(string invoiceNum, string menuregion)
        {
            IPaymentDetailService service = SpringFactory.GetObjectImpl<IPaymentDetailService>("PaymentDetailService");
            return service.GetInvoiceInfoByNum(invoiceNum, menuregion);
        }

    }
}