using Intelligent.OTC.Business.Interfaces;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.Domain.Repositories;
using System.Collections.Generic;
using System;
using Intelligent.OTC.Domain.DomainModel;
using System.Linq;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Common;
using System.Data.SqlClient;
using System.Transactions;
using Intelligent.OTC.Common.Exceptions;

namespace Intelligent.OTC.Business
{
    public class CaPTPService : ICaPTPService
    {

        public OTCRepository CommonRep { get; set; }

        public CaPTPDtoPage getCaPTPList(string customerNum,string legalEntity, string customerCurrency, string invCurrency, string amt, string localAmt, string ptpDateF, string ptpDateT)
        {
            CaPTPDtoPage result = new CaPTPDtoPage();

            if (string.IsNullOrEmpty(customerNum) || customerNum == "undefined")
            {
                customerNum = "";
            } 
            if (string.IsNullOrEmpty(legalEntity) || legalEntity == "undefined")
            {
                legalEntity = "";
            }
            if (string.IsNullOrEmpty(customerCurrency) || customerCurrency == "undefined")
            {
                customerCurrency = "";
            }
            if (string.IsNullOrEmpty(amt) || amt == "undefined" || amt == "null")
            {
                amt = "";
            } 
            if (string.IsNullOrEmpty(localAmt) || localAmt == "undefined" || localAmt == "null")
            {
                localAmt = "";
            }
            if (string.IsNullOrEmpty(invCurrency) || invCurrency == "undefined")
            {
                invCurrency = "";
            }
            if (string.IsNullOrEmpty(ptpDateF) || ptpDateF == "undefined")
            {
                ptpDateF = "";
            }
            if (string.IsNullOrEmpty(ptpDateT) || ptpDateT == "undefined")
            {
                ptpDateT = "";
            }

            string sql = string.Format(@"SELECT
                            LegalEntity,
	                        CUSTOMER_NAME,
	                        CUSTOMER_NUM,
	                        PTP_DATE,
	                        FUNC_CURRENCY,
	                        SUM (AMT) AS AMT,
	                        INV_CURRENCY,
	                        SUM (Local_AMT) AS Local_AMT
                        FROM
	                        V_CA_PTP with (nolock)
                        WHERE ((CUSTOMER_NUM like '%{0}%') OR '' = '{0}')
                        AND ((FUNC_CURRENCY like '%{1}%') OR '' = '{1}')
                        AND ((LegalEntity like '%{7}%') OR '' = '{7}')
                        AND ((INV_CURRENCY like '%{2}%') OR '' = '{2}')
                        AND (PTP_DATE >= '{5} 00:00:00' OR '' = '{5}')
                        AND (PTP_DATE <= '{6} 23:59:59' OR '' = '{6}')
                        GROUP BY
                            LegalEntity,
	                        CUSTOMER_NAME,
	                        CUSTOMER_NUM,
	                        PTP_DATE,
	                        FUNC_CURRENCY,
	                        INV_CURRENCY
                        HAVING
                          (SUM (AMT) = '{3}' OR '' = '{3}')
                          AND (SUM (Local_AMT) = '{4}' OR '' = '{4}')
                        ORDER BY
                            LegalEntity,
	                        CUSTOMER_NAME,
	                        CUSTOMER_NUM,
	                        PTP_DATE,
	                        FUNC_CURRENCY", customerNum, customerCurrency, invCurrency, amt, localAmt, ptpDateF, ptpDateT,legalEntity);

            List<CaPTPDto> dto = CommonRep.ExecuteSqlQuery<CaPTPDto>(sql).ToList();
            
            result.dataRows = dto;

            return result;
        }

    }
}
