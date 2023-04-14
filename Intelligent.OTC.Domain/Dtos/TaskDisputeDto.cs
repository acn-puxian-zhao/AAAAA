using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class TaskDisputeDto
    {
        public long Id { get; set; }
        public string LegalEntity { get; set; }
        public string CustomerNum { get; set; }
        public string SiteUseId { get; set; }
        public string IssueReason { get; set; }
        public string IssueReasonName { get; set; }
        public string DisputeStatus { get; set; }
        public string DisputeStatusName { get; set; }
        public Nullable<System.DateTime> Status_Date { get; set; }
        public string Comments { get; set; }
    }
}
