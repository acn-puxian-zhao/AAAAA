using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class CaBankStatementPostDto
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

        public DateTime? MaturityDate { get; set; }
        public string CheckNumber { get; set; }
        public string Ref1 { get; set; }
        public string CustomerName { get; set; }
        public string EBName { get; set; }
        public string BSComments { get; set; }
        public string PMTGroupNo { get; set; }
        public string PaymentTerm { get; set; }
        public string PMTFileName { get; set; }
        public DateTime? PMTReceiveDate { get; set; }
    }
}
