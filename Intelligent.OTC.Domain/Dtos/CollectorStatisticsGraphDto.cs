using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.DataModel
{
    public class CollectorStatisticsGraphDto
    {
        public string title { get; set; }
        public IQueryable<Object> legend { get; set; }
        public IQueryable<Object> xAxis { get; set; }
        public IQueryable<Object> series { get; set; }
    }
}
