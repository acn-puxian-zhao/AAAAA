using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class TaskSendSoaPmtList
    {
        public string ActionDate { get; set; }
        public string TempleteLanguage { get; set; }
        public string TempleteLanguageName { get; set; }
        public string Region { get; set; }
        public string Deal { get; set; }
        public string Eid { get; set; }
        public int PeriodId { get; set; }
        public int AlertType { get; set; }
        public string ToTitle { get; set; }
        public string ToName { get; set; }
        public string CcTitle { get; set; }
        public DateTime ResponseDate { get; set; }
        public string CustomerNum { get; set; }
        public string CustomerName { get; set; }
        public string Comment { get; set; }

    }
}
