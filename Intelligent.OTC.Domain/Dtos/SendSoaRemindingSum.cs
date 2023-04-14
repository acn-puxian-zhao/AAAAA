using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class SendSoaRemindingSum
    {
        public string status { get; set; }
        public string eid { get; set; }
        public string region { get; set; }
        public int count { get; set; }
    }
}
