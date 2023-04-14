using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain
{
    public class submitWaitInvDet
    {
        public string InvoiceDate { get; set; }
        public string CustomerPO { get; set; }
        public string Manufacturer { get; set; }
        public string PartNumber { get; set; }
        public string InvoiceNumber { get; set; }
        public int? InvoiceLineNumber { get; set; }
        public string TransactionCurrencyCode { get; set; }
        public decimal? InvoiceQty { get; set; }
        public decimal? UnitResales { get; set; }
        public decimal? NSB { get; set; }
    }
}
