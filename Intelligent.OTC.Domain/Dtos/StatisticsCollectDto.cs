using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class StatisticsCollectDto
    {
        public string CustomerNum { get; set; }

        public string CustomerName { get; set; }

        public string SiteUseId { get; set; }

        public Nullable<decimal> openAR { get; set; }

        public Nullable<decimal> overDure { get; set; }

        public Nullable<decimal> dispute { get; set; }

        public string Region { get; set; }

        public string Collector { get; set; }

    }
}
