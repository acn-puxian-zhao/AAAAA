using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
   public class CaReconMsgDetailAllDto
    {
        public string ID { get; set; }
        public string LegalEntity { get; set; }
        public string CUSTOMER_NUM { get; set; }
        public string SiteUseId { get; set; }
        public DateTime DUE_DATE { get; set; }
        public decimal AMT { get; set; }
        public decimal Local_AMT { get; set; }

    }
}
