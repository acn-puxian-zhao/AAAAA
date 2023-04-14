using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class OpenARDto
    {
        public Nullable<decimal> ACurrent { get; set; }
        public Nullable<decimal> B30 { get; set; }
        public Nullable<decimal> C60 { get; set; }
        public Nullable<decimal> D90 { get; set; }
        public Nullable<decimal> E120 { get; set; }
        public Nullable<decimal> F150 { get; set; }
        public Nullable<decimal> G180 { get; set; }
        public Nullable<decimal> H360 { get; set; }
        public Nullable<decimal> I360 { get; set; }
        
    }
}
