using System;

namespace Intelligent.OTC.Domain.Dtos
{
    public class ReportPTPSumItem
    {
        public string Region { get; set; }

        //confirm / broken
        public string Category { get; set; } 

        public int CustomerPTPCount { get; set; }

        public int CustomerODCount { get; set; }

        public int CustomerBrokenCount { get; set; }

        public string Currency { get; set; }

        public decimal PTPAmount { get; set; }

        public decimal ODAmount { get; set; }

        public decimal BrokenAmount { get; set; }

        public decimal CountRate
        {
            get
            {
                if (Category == "Confirm")
                {
                    if (CustomerODCount == 0) return 0;
                    return decimal.Parse((CustomerPTPCount * 100 / CustomerODCount).ToString("#0.00"));
                }
                else
                {
                    if (CustomerPTPCount == 0) return 0;
                    return decimal.Parse((CustomerBrokenCount * 100 / CustomerPTPCount).ToString("#0.00"));
                }
               
            }
        }

        public decimal AmountRate
        {
            get
            {
                if (Category == "Confirm")
                {
                    if (ODAmount == 0) return 0;
                    return decimal.Parse((PTPAmount * 100 / ODAmount).ToString("#0.00"));
                }
                else
                {
                    if (PTPAmount == 0) return 0;
                    return decimal.Parse((BrokenAmount * 100 / PTPAmount).ToString("#0.00"));
                }

            }
        }
    }

    public class ReportPTPDetailItem
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
