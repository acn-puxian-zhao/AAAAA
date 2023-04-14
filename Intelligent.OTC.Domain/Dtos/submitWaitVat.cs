using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain
{
    public class submitWaitVat
    {
        public string Trx_Number { get; set; }
        public int? LineNumber { get; set; }
        public string SalesOrder { get; set; }
        public string CreationDate { get; set; }
        public string CustomerTrxId { get; set; }
        public string AttributeCategory { get; set; }
        public string OrgId { get; set; }
        public string VATInvoice { get; set; }
        public string VATInvoiceDate { get; set; }
        public decimal? VATInvoiceAmount { get; set; }
        public decimal? VATTaxAmount { get; set; }
        public decimal? VATInvoiceTotalAmount { get; set; }
        //public string CreatedDate { get; set; }
        //public string CreatedUser { get; set; }
        //public string IMPORT_ID { get; set; }
    }
}
