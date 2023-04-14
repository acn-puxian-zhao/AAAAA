using System;

namespace Intelligent.OTC.Domain.DataModel
{
    public class InvoicesStatusDto
    {
        public string SiteUseId { get; set; }
        public DateTime? INVOICE_DATE { get; set; }
        public string INVOICE_NO { get; set; }
        public decimal? INVOICE_AMOUNT { get; set; }
        public string INVOICE_LineNo { get; set; }
        public string INVOICE_MaterialNo { get; set; }
        public decimal? INVOICE_MaterialAmount { get; set; }
        public string INVOICE_BalanceStatus { get; set; }
        public string INVOICE_BalanceMemo { get; set; }
        public string INVOICE_Comments { get; set; }
        public DateTime? INVOICE_DUEDATE { get; set; }
        public DateTime? INVOICE_PTPDATE { get; set; }
        public DateTime? INVOICE_PTPDATE_OLD { get; set; }
        public string INVOICE_DISPUTE { get; set; }
        public string INVOICE_DISPUTE_OLD { get; set; }
        public string INVOICE_OTHER { get; set; }
        public string INVOICE_IsForwarder { get; set; }
        public string INVOICE_Forwarder { get; set; }
        public string INVOICE_Class { get; set; }
        public string INVOICE_CurrencyCode { get; set; }
        public string INVOICE_DueReason { get; set; }
        public string INVOICE_DueReason_OLD { get; set; }
        public string INVOICE_Status { get; set; }
        public DateTime? MemoExpirationDate { get; set; }
    }
}
