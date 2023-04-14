using System;
using System.Collections.Generic;

namespace Intelligent.OTC.Domain.Dtos
{
   public class CaARViewDto
    {
        public string GroupType { get; set; }
        public string LegalEntity { get; set; }
        public string CustomerNum { get; set; }
        public string SiteUseId { get; set; }
        public string InvoiceNum { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string FuncCurrency { get; set; }
        public string InvCurrency { get; set; }
        public decimal? Amt { get; set; }
        public decimal? LocalAmt { get; set; }
        public string Ebname { get; set; }

    }

    public class CaARViewDtoPage
    {
        public List<CaARViewDto> list;

        public int listCount;
    }

    public class CaARViewDtoAndAmtTotal
    {
        public List<CaARViewDto> list;

        public decimal? amtTotal;
    }
}
