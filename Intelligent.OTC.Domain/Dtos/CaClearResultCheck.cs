using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class CaClearResultCheck
    {
        public DateTime ChangeDate { get; set; }
        public string LegalEntity { get; set; }
        public string BSTransactionInc { get; set; }
        public string BSCurrency { get; set; }
        public string CustomerNum { get; set; }
        public string SiteUseId { get; set; }
        public string InvoiceNum { get; set; }
        public decimal ClearAmount { get; set; }
        public decimal OracleChange { get; set; }
        public decimal Charge { get; set; }
        public string status { get; set; }
    }
}
