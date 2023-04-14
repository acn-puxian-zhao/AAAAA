using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class CallBackReconResultDto
    {
        public string KEY { get; set; }

        public List<CallBackReconGroup> GROUP;
    }

    public class CallBackReconGroup
    { 
        public string INV { get; set; }

        public decimal? AMOUNT { get; set; }

        public decimal? FUNCTAMOUNT { get; set; }
    }
}
