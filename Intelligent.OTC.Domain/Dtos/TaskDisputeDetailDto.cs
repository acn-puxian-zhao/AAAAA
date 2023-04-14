using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class TaskDisputeDetailDto
    {
        public long Id { get; set; }
        public string InvoiceId { get; set; }
        public string InvoiceNum { get; set; }
        public Nullable<decimal> BalanceAmt { get; set; }
        public string Comments { get; set; }
    }
}
