using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.DomainModel
{
  public  class ReportModel
    {
        public ReportModel()
        {
            List = new List<ReportItem>();
        }
        public int TotalItems { get; set; }
        public List<ReportItem> List { get; set; }
    }
}
