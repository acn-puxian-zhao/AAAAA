using System;

namespace Intelligent.OTC.Domain.Dtos
{
    public class ReportUnApplySumItem
    {
        public string Region { get; set; }

        public string Type { get; set; }

        public string Currency { get; set; }

        public decimal Amount { get; set; }

        public decimal TotalAR { get; set; }

        public decimal Rate
        {
            get
            {
                if (TotalAR == 0) return 0;
                return decimal.Parse((Amount * 100 / TotalAR).ToString("#0.00"));
            }
        }
    }

    public class ReportUnApplyDetailItem
    {
        public string Collector { get; set; }

        public string Region { get; set; }

        public string Type { get; set; }

        public string CustomerName { get; set; }

        public string CustomerNum { get; set; }

        public string SiteUseId { get; set; }

        public string Class { get; set; }

        public string InvoiceNum { get; set; }

        public DateTime InvoiceDate { get; set; }

        public DateTime DueDate { get; set; }

        public string FuncCurrCode { get; set; }

        public string Currency { get; set; }

        public int DueDays { get; set; }

        public decimal InvoiceAmount { get; set; }

        public string AgingBucket { get; set; }

        public string CreditTrem { get; set; }

        public string EbName { get; set; }

        public string Lsr { get; set; }

        public string Sales { get; set; }

        public string LegalEntity { get; set; }

        public string Cmpinv { get; set; }

        public string SONum { get; set; }

        public string PONum { get; set; }

        public DateTime? PtpDate { get; set; }

        public string OverdueReason { get; set; }

        public string Comments { get; set; }
    }
}
