using System;
using System.Collections.Generic;

namespace Intelligent.OTC.Domain.Dtos
{
   public class CaARDto
    {
        public string LegalEntity { get; set; }
        public string CUSTOMER_NUM { get; set; }
        public string SiteUseId { get; set; }
        public string INVOICE_NUM { get; set; }
        public DateTime DUE_DATE { get; set; }
        public DateTime INVOICE_DATE { get; set; }
        public string func_currency { get; set; }
        public string INV_CURRENCY { get; set; }
        public decimal AMT { get; set; }
        public decimal Local_AMT { get; set; }

        public string EbName { get; set; }

        public string OrderNumber { get; set; }
        public int pmtCount { get; set; }
        public int HasVAT { get; set; }

    }

    public class CaARDtoPage
    {
        public List<CaARDto> dataRows;

        public int count;
    }

}
