using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common;

namespace Intelligent.OTC.Domain.DataModel
{
    public partial class InvoiceAging: IAggregateRoot
    {
        public Nullable<decimal> StandardBalanceAmt { get; set; }

        public Nullable<decimal> ArBalanceAmtPeroid { get; set; }
        public Nullable<decimal> PaidAmt { get; set; }

        public string BillGroupName { get; set; }
    }
}
