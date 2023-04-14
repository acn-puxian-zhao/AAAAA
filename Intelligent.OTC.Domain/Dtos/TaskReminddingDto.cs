using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class TaskReminddingDto
    {
        public string deal { get; set; }
        public string legalEntity { get; set; }
        public string customerNum { get; set; }
        public string CustomerName { get; set; }
        public string siteUseId { get; set; }
        public Nullable<System.DateTime> task_date { get; set; }
        public string task_content { get; set; }
    }
}
