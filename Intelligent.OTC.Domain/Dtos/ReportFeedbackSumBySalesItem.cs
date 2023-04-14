using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class ReportFeedbackSumItemBySales
    {
        public string Region { get; set; }

        public string Sales { get; set; }

        public string Branchmanager { get; set; }

        public string Currency { get; set; }

        public decimal BalanceAmt { get; set; }
    }

    public class ReportFeedbackDetailItemBySales
    {
        public string Region { get; set; }

        public string Sales { get; set; }

        public string EbName { get; set; }

        public string CreditTerm { get; set; }

        public string CustomerName { get; set; }

        public string CustomerNum { get; set; }

        public string SiteUseId { get; set; }

        public string InvoiceNum { get; set; }

        public string InvoiceDate { get; set; }

        public string DueDate { get; set; }

        public string Currency { get; set; }

        public decimal BalanceAmount { get; set; }

        public string PtpDate { get; set; }

        public string OverDueReason { get; set; }

        public string Comments { get; set; }
    }
}
