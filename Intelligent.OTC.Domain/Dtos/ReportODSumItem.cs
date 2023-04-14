using System;

namespace Intelligent.OTC.Domain.Dtos
{
    public class ReportODSumItem
    {
        public string Region { get; set; }

        public string OverdueReason { get; set; }

        public string Currency { get; set; }

        public decimal ODAmount { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal Rate
        {
            get
            {
                return decimal.Parse((ODAmount * 100 / TotalAmount).ToString("#0.00"));
            }
        }
    }

    public class ReportODDetailItem
    {
        public string Region { get; set; }

        public string Organization { get; set; }

        public string CustomerName { get; set; }

        public string CustomerNum { get; set; }

        public string SiteUseId { get; set; }

        public string EbName { get; set; }

        public string CreditTrem { get; set; }

        public string FuncCurrency { get; set; }

        public string InvoiceNum { get; set; }

        public DateTime InvoiceDate { get; set; }

        public DateTime DueDate { get; set; }

        public DateTime? PtpDate { get; set; }

        public int DueDays { get; set; }

        public string AgingBucket { get; set; }

        public string InvoiceType { get; set; }

        public string Currency { get; set; }

        public string PONum { get; set; }

        public string SONum { get; set; }

        public string Cmpinv { get; set; }

        public string OverdueReason { get; set; }

        public decimal ODAmount { get; set; }

        public string LsrNameHist { get; set; }

        public string FsrNameHist { get; set; }

        public string Remark { get; set; }
    }
}
