using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class TaskDetailDto
    {
        public int TASKID { get; set; }
        public string DEAL { get; set; }
        public string LEGAL_ENTITY { get; set; }
        public string CUSTOMER_NUM { get; set; }
        public string SITEUSEID { get; set; }
        public Nullable<System.DateTime> TASK_DATE { get; set; }
        public string TASK_TYPE { get; set; }
        public string TASK_CONTENT { get; set; }
        public string TASK_STATUS { get; set; }
        public string ISAUTO { get; set; }
        public int CREATE_USER { get; set; }
        public System.DateTime CREATE_DATE { get; set; }
        public int? UPDATE_USER { get; set; }
        public Nullable<System.DateTime> UPDATE_DATE { get; set; }
    }
}
