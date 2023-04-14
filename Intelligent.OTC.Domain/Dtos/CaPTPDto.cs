using System;
using System.Collections.Generic;

namespace Intelligent.OTC.Domain.Dtos
{
   public class CaPTPDto
    {
        public string LegalEntity { get; set; }
        public string CUSTOMER_NAME { get; set; }
        public string LEGAL_ENTITY { get; set; }
        public string CUSTOMER_NUM { get; set; }
        public string SiteUseId { get; set; }
        public string INVOICE_NUM { get; set; }
        public DateTime PTP_DATE { get; set; }
        public string FUNC_CURRENCY { get; set; }
        public decimal AMT { get; set; }
        public string INV_CURRENCY { get; set; }
        public decimal Local_AMT { get; set; }
    }

    public class CaPTPDtoPage
    {
        public List<CaPTPDto> dataRows;

        public int count;
    }
}
