using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class DsoAnalysisDto
    { 
        public string Legal { get; set; }
        public string InvClass { get; set; }
        public string InvoiceNo { get; set; }
        public string CusNumberNum { get; set; }
        public string CusName { get; set; }
        public string InvoiceDate { get; set; }
        public string PaymentTerm { get; set; }
        public decimal EnterAmount { get; set; }
        public decimal FunctionalAmount { get; set; }
        public decimal ARAvg { get; set; }
        public decimal DSO { get; set; }
        public decimal REV { get; set; }
        public decimal GAP { get; set; }
    }
}
