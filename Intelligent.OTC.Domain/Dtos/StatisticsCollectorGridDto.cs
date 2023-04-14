using Intelligent.OTC.Domain.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class StatisticsCollectorGridDto
    {
        public List<V_STATISTICS_COLLECTOR> result { get; set; }
        public int count { get; set; }
    }
}
