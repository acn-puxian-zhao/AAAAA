using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class CaBankStatementPostClearDto
    {

        public string ID { get; set; }
        public string IOTCLegalEntity { get; set; }
        public string LegalEntity { get; set; }
        public string Region { get; set; }
        public int No { get; set; }
        public string RVNumber { get; set; }
        public DateTime? ReceiptsDate { get; set; }
        public string Currency { get; set; }
        public decimal? NetReceiptsAmount { get; set; }
        public string ReceiptsMethod { get; set; }
        public string BankAccountNumber { get; set; }
        public decimal BankCharge { get; set; }
        public string CustomerNumber { get; set; }
        public string ForwardNumber { get; set; }
        public string SiteUseId { get; set; }
        public string Comments { get; set; }

        public int InvoiceRowNo { get; set; }
        public string InvoiceNumber { get; set; }
        public string InvoiceSiteUseId { get; set; }
        public decimal? AmountApplied { get; set; }
        public string InvoiceComments { get; set; }
        public int HasVAT { get; set; }
        public string Ebname { get; set; }
    }
}
