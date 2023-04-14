using System;

namespace Intelligent.OTC.Domain.Dtos
{
    public class ReportFeedbackSumItem
    {
        public string Region { get; set; }

        public int TotalCount { get; set; }

        public int FeedbackCount { get; set; }

        public decimal Rate
        {
            get
            {
                return decimal.Parse((FeedbackCount * 100 / TotalCount).ToString("#0.00"));
            }
        }
    }

    public class ReportNotFeedbackItem {
        public string Region { get; set; }
        public string Collector { get; set; }
        public string Organization { get; set; }
        public string CustomerName { get; set; }
        public string CreditTerm { get; set; }
        public string CustomerNum { get; set; }
        public string SiteUseId { get; set; }
        public string Class { get; set; }
        public string InvoiceNum { get; set; }
        public string InvoiceDate { get; set; }
        public string DueDate { get; set; }
        public string FuncCurrCode { get; set; }
        public string Currency { get; set; }
        public int DaysLateSys { get; set; }
        public decimal BalanceAmt { get; set; }
        public string AgingBucket { get; set; }
        public string CreditTremDescription { get; set; }
        public string Ebname { get; set; }
        public string LsrNameHist { get; set; }
        public string FsrNameHist { get; set; }
        public string LegalEntity { get; set; }
        public string Cmpinv { get; set; }
        public string SoNum { get; set; }
        public string PoNum { get; set; }
        public string PtpDate { get; set; }
        public string OverdueReason { get; set; }
        public string Comments { get; set; }
        public string Status { get; set; }
        public string CloseDate { get; set; }
    }

    public class ReportHasFeedbackItem
    {
        public string Region { get; set; }
        public string Collector { get; set; }
        public string Organization { get; set; }
        public string CustomerName { get; set; }
        public string CreditTerm { get; set; }
        public string CustomerNum { get; set; }
        public string SiteUseId { get; set; }
        public string Class { get; set; }
        public string InvoiceNum { get; set; }
        public string InvoiceDate { get; set; }
        public string DueDate { get; set; }
        public string FuncCurrCode { get; set; }
        public string Currency { get; set; }
        public int DaysLateSys { get; set; }
        public decimal BalanceAmt { get; set; }
        public string AgingBucket { get; set; }
        public string CreditTremDescription { get; set; }
        public string Ebname { get; set; }
        public string LsrNameHist { get; set; }
        public string FsrNameHist { get; set; }
        public string LegalEntity { get; set; }
        public string Cmpinv { get; set; }
        public string SoNum { get; set; }
        public string PoNum { get; set; }
        public string PtpDate { get; set; }
        public string OverdueReason { get; set; }
        public string Comments { get; set; }
        public string Status { get; set; }
        public string CloseDate { get; set; }
    }

    public class ReportFeedbackDetailItem
    {
        public string Region { get; set; }

        public string Collector { get; set; }

        public string Organization { get; set; }

        public string CustomerName { get; set; }

        public string CreditTerm { get; set; }

        public string CustomerNum { get; set; }

        public string SiteUseId { get; set; }

        public string Class { get; set; }

        public string InvoiceNum { get; set; }

        public string InvoiceDate { get; set; }

        public string DueDate { get; set; }

        public string FuncCurrCode { get; set; }

        public string Currency { get; set; }

        public int? DueDays { get; set; }

        public decimal? InvoiceAmount { get; set; }

        public string AgingBucket { get; set; }

        public string CreditTremDesc { get; set; }

        public string EbName { get; set; }

        public string CS { get; set; }

        public string Sales { get; set; }

        public string LegalEntity { get; set; }

        public string Cmpinv { get; set; }

        public string SONum { get; set; }

        public string PONum { get; set; }

        public string PtpDate { get; set; }

        public string OverdueReason { get; set; }

        public string Comments { get; set; }
        public DateTime? MemoExpirationDate { get; set; }
        public string Status { get; set; }
        public string CloseDate { get; set; }
        public string feedback { get; set; }
        public string SendDate { get; set; }
    }
}
