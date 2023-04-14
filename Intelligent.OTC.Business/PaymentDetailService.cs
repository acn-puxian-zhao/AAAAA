using Intelligent.OTC.Business.Interfaces;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Business
{
    public class PaymentDetailService : IPaymentDetailService
    {
        public CaBankStatementDto GetBankStatementByTranINC(string transactionNumber, string menuregion)
        {
            string sql = String.Format(@"select top 1 *
                                        from T_CA_BankStatement
                                        where TRANSACTION_NUMBER = '{0}'
                                        and MENUREGION = '{1}'
                                        and DEL_FLAG = 0", transactionNumber, menuregion);
            List<CaBankStatementDto> list = SqlHelper.GetList<CaBankStatementDto>(SqlHelper.ExcuteTable(sql, System.Data.CommandType.Text, null));
            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public InvoiceAgingDto GetInvoiceInfoByNum(string invoiceNum, string menuregion)
        {
            string sql = String.Format(@"select SiteUseId as SiteUseId, 
                                        INVOICE_NUM as InvoiceNum, 
                                        INVOICE_DATE as InvoiceDate,
                                        DUE_DATE as DueDate, INV_CURRENCY as Currency, 
                                        AMT as BalanceAmt
                                        from V_CA_AR where InvoiceNum = '{0}'", invoiceNum);
            List<InvoiceAgingDto> list = SqlHelper.GetList<InvoiceAgingDto>(SqlHelper.ExcuteTable(sql, System.Data.CommandType.Text, null));
            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }
    }
}
