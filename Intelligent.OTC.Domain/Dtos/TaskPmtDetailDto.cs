using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class TaskPmtDetailDto
    {
        public long Id { get; set; }
        public string InvoiceNum { get; set; }
        public Nullable<decimal> BalanceAmt { get; set; }
        public string Comments { get; set; }
        public long IsFullMatch { get; set; }
        public string Color { get; set; }
    }
}
