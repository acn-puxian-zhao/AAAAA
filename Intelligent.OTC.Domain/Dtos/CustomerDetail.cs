
namespace Intelligent.OTC.Domain.DataModel
{
    using System;
    using System.Collections.Generic;

    public partial class CustomerDetail : CustomerAging
    {
        public Nullable<decimal> DueOver30Amt { get; set; }
        public Nullable<decimal> DueOver60Amt { get; set; }
        public Nullable<decimal> DueOver90Amt { get; set; }
        public Nullable<decimal> DueOver180Amt { get; set; }
        public Nullable<decimal> DueOver0Amt { get; set; }
        public Nullable<decimal> FCollectAmt { get; set; }
        public Nullable<decimal> FDueOver90Amt { get; set; }
    }
}
