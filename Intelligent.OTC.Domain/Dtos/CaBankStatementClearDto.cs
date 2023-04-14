using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class CaBankStatementClearDto
    {
        public string ID { get; set; }
        public string IOTCLegalEntity { get; set; }
        public string LegalEntity { get; set; }
        public string Currency { get; set; }
        public string CustomerNum { get; set; }
        public string CustomerName { get; set; }
        public string ReconId { get; set; }
        public string ReconDetailId { get; set; }
        public int No { get; set; }
        public string RVNumber { get; set; }

        public DateTime? RVDate { get; set; }

        public decimal? RVAmount { get; set; }
        public string RVSiteUseId { get; set; }
        public string InvoiceSiteUseId { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public decimal? AmountApplied { get; set; }
        public string Comments { get; set; }
        public int HasVAT { get; set; }
        public string Ebname { get; set; }

        public string Country { get; set; }
        public string BSComments { get; set; }
        public string PMTGroupNo { get; set; }
        public string PaymentTerm { get; set; }
        public string PMTFileName { get; set; }
        public DateTime PMTReceiveDate { get; set; }
    }
}
