using System;
using System.Collections.Generic;

namespace Intelligent.OTC.Domain.Dtos
{
   public class CaReconTaskDto
    {
        public string ID { get; set; }
        public string TASK_ID { get; set; }
        public string INPUT { get; set; }
        public string OUTPUT { get; set; }
        public string CREATE_USER { get; set; }
        public int STATUS { get; set; }
        public int SORT_ID { get; set; }
        public bool DEL_FLAG { get; set; }
        public DateTime? CREATE_DATE { get; set; }

    }

}
