using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public partial class CaReconDto
    {
        public string ID { get; set; }

        public string MENUREGION { get; set; }

        public string GroupNo { get; set; }

        public string GroupType { get; set; }

        public string TASK_ID { get; set; }

        public string CREATE_USER { get; set; }

        public DateTime? CREATE_DATE { get; set; }

        public string UPDATE_USER { get; set; }

        public DateTime? UPDATE_DATE { get; set; }

        public int? DEL_FLAG { get; set; }
        public string PMT_ID { get; set; }

        public bool isClosed { get; set; }
    }
}
