using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class T_INVOICE_STATUS_CUSTOMER_STAGING
    {
        public string FileType { get; set; }
        public string SiteUseId { get; set; }
        public string AgingBucket { get; set; }
        public decimal? PTPAmount { get; set; }
        public DateTime? PTPDate { get; set; }
        public string ODReason { get; set; }
        public string Comments { get; set; }
        public string Create_User { get; set; }
        public DateTime? Create_Date { get; set; }
    }
}
