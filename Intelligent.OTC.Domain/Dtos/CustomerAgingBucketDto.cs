using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class CustomerAgingBucketDto
    {
        public string CustomerNum { get; set; }
        public string SiteUseId { get; set; }
        public Nullable<decimal> TotalAmt { get; set; }
        public Nullable<decimal> CurrentAmt { get; set; }
        public Nullable<decimal> DueoverTotalAmt { get; set; }
        public Nullable<decimal> Due15Amt { get; set; }
        public Nullable<decimal> Due30Amt { get; set; }
        public Nullable<decimal> Due45Amt { get; set; }
        public Nullable<decimal> Due60Amt { get; set; }
        public Nullable<decimal> Due90Amt { get; set; }
        public Nullable<decimal> Due120Amt { get; set; }
        public Nullable<decimal> Due150Amt { get; set; }
        public Nullable<decimal> Due180Amt { get; set; }
        public Nullable<decimal> Due210Amt { get; set; }
        public Nullable<decimal> Due240Amt { get; set; }
        public Nullable<decimal> Due270Amt { get; set; }
        public Nullable<decimal> Due300Amt { get; set; }
        public Nullable<decimal> Due330Amt { get; set; }
        public Nullable<decimal> Due360Amt { get; set; }
        public Nullable<decimal> DueOver360Amt { get; set; }
    }
}
