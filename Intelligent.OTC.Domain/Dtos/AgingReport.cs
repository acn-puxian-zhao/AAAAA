
namespace Intelligent.OTC.Domain.DataModel
{
    using System;
    using System.Collections.Generic;

    public partial class AgingReport
    {
        public string Deal { get; set; }
        public string LegalEntity { get; set; }
        public string Team { get; set; }
        public string CustomerNum { get; set; }
        public string CustomerName { get; set; }
        public string BillGroupCode { get; set; }
        public string BillGroupName { get; set; }
        public string Country { get; set; }
        public Nullable<decimal> CreditLimit { get; set; }
        public string CreditTrem { get; set; }
        public string Collector { get; set; }
        public string Collectorsys { get; set; }
        public string Sales { get; set; }
        public Nullable<decimal> TotalAmt { get; set; }
        public Nullable<decimal> CurrentAmt { get; set; }
        public Nullable<decimal> Due30Amt { get; set; }
        public Nullable<decimal> Due60Amt { get; set; }
        public Nullable<decimal> Due90Amt { get; set; }
        public Nullable<decimal> Due180Amt { get; set; }
        public Nullable<decimal> Due360Amt { get; set; }
        public Nullable<decimal> DueOver360Amt { get; set; }
        public Nullable<decimal> DueOver60Amt { get; set; }
        public Nullable<decimal> AdjustedDueOver60Amt { get; set; }
        public Nullable<decimal> DueOver90Amt { get; set; }
        public Nullable<decimal> AdjustedDueOver90Amt { get; set; }
        public Nullable<decimal> DueOver180Amt { get; set; }
        public Nullable<decimal> OneYearSales { get; set; }
    }
}
