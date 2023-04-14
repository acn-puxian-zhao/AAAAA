using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class TaskReportDto
    {
        public string Deal { get; set; }
        public string LegalEntity { get; set; }
        public string CustomerNo { get; set; }
        public string CustomerName { get; set; }
        public string SiteUseId { get; set; }
        public string Collector { get; set; }
        public string Contactor { get; set; }
        public Nullable<decimal> TotalAr { get; set; }
        public Nullable<System.DateTime> TaskDate { get; set; }
        public string TaskType { get; set; }
        public string TaskContent { get; set; }
        public string TaskStatus { get; set; }
        public string IsAuto { get; set; }
        public string LastSoaDate { get; set; }
        public int? Star { get; set; }
    }
}
