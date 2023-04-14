using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class StatisticsCollectSumDto
    {
        public Nullable<decimal> openAR { get; set; }

        public Nullable<decimal> overDure { get; set; }

        public Nullable<decimal> dispute { get; set; }

        public Nullable<decimal> ptpAR { get; set; }

        public DateTime now { get; set; }
    }
}
