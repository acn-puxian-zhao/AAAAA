using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class SendPMTSummary
    {
        public string collector { get; set; }
        public int success { get; set; }
        public int failed { get; set; }
    }
}
