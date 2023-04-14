using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.WebApi.Core;

namespace Intelligent.OTC.Domain.Dtos
{
    public class AlertExtention<T> : ODataMetadata<T> where T : class
    {
        public AlertExtention(IEnumerable<T> result, long? count)
            : base(result, count)
        {
        }
        public decimal? TotalAmount { get; set; }
        public decimal? TotalPastDue { get; set; }
        public decimal? TotalOver90Days { get; set; }
        public decimal? TotalCreditLimit { get; set; }
    }
}
