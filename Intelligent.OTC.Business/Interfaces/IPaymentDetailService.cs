using Intelligent.OTC.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Business.Interfaces
{
    public interface IPaymentDetailService
    {
        CaBankStatementDto GetBankStatementByTranINC(string transactionNumber, string menuregion);

        InvoiceAgingDto GetInvoiceInfoByNum(string invoiceNum, string menuregion);
    }
}
