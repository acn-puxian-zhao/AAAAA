using System;
using System.Collections.Generic;

namespace Intelligent.OTC.Domain.Dtos
{
   public class CaReconGroupDto
    {
        public string RECONID { get; set; }
        public string LegalEntity { get; set; }
        public string TRANSACTION_NUMBER { get; set; }
        public DateTime? VALUE_DATE { get; set; }
        public string TRANSACTION_CURRENCY { get; set; }
        public decimal? TRANSACTION_AMOUNT { get; set; }
        public string CUSTOMER_NUM { get; set; }
        public string CUSTOMER_NAME { get; set; }
        public string FORWARD_NUM { get; set; }
        public string FORWARD_NAME { get; set; }
        public string GROUP_NO { get; set; }
        public string GROUP_TYPE { get; set; }
        public string INVOICE_SITEUSEID { get; set; }
        public string INVOICE_NUM { get; set; }
        public DateTime? INVOICE_DUEDATE { get; set; }
        public string INVOICE_CURRENCY { get; set; }
        public decimal? INVOICE_AMOUNT { get; set; }
        public string Ebname { get; set; }
        public int HasVAT { get; set; }
        public string MATCH_STATUS { get; set; }
        public bool isClosed { get; set; }

    }

    public class CaReconGroupDtoPage
    {
        public List<CaReconGroupDto> dataRows;

        public int count;
    }
}
