using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class InvoiceVatDto 
    {
        public string TrxNumber { get; set; }
        public int LineNumber { get; set; }
        public string SalesOrder { get; set; }
        public DateTime? CreationDate { get; set; }
        public string CustomerTrxId { get; set; }
        public string AttributeCategory { get; set; }
        public string OrgId { get; set; }
        public string VATInvoice { get; set; }
        public string VATInvoiceDate { get; set; }
        public decimal? VATInvoiceAmount { get; set; }
        public decimal? VATTaxAmount { get; set; }
        public decimal? VATInvoiceTotalAmount { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedUser { get; set; }

    }
}
