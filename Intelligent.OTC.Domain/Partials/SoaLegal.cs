using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common;

namespace Intelligent.OTC.Domain.DataModel
{
    public partial class SoaLegal
    {
        public string LegalEntity { get; set; }
        public string Country { get; set; }
        public Nullable<decimal> CreditLimit { get; set; }
        public Nullable<decimal> TotalARBalance { get; set; }
        public Nullable<decimal> PastDueAmount { get; set; }
        public Nullable<decimal> CreditBalance { get; set; }
        public Nullable<decimal> CurrentBalance { get; set; }
        public Nullable<decimal> FCollectableAmount { get; set; }
        public Nullable<decimal> FOverdue90Amount { get; set; }
        //change to legal
        //public List<SoaContact> SubContact { get; set; }
        //public List<SoaPaymentBank> SubPaymentBank { get; set; }
        //public List<SoaPaymentCalender> SubPaymentCalender { get; set; }
        //change to legal
        public List<SoaInvoice> SubInvoice { get; set; }
        public CurrentTracking SubTracking { get; set; }
        public string SiteUseId { get; set; }
        public string SpecialNotes { get; set; }
    }
}
