using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
   public class CaReconMsgDetailDto
    {
        public string ID { get; set; }
        public DateTime DUE_DATE { get; set; }
        public decimal AMT { get; set; }

    }
}
