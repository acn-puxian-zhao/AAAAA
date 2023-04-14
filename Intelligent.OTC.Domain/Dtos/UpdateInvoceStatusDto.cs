using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class UpdateInvoceStatusDto
    {
        public int disputeId { get; set; }
        public string status { get; set; }
        public List<int> invIds { get; set; }
    }
}
