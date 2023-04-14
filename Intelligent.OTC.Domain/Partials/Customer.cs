using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common;

namespace Intelligent.OTC.Domain.DataModel
{
    public partial class Customer : IAggregateRoot
    {
        public string CustomerClass { get; set; }
        public Customer UpdateWithLevel(CustomerLevelView level)
        {
            
            this.CustomerClass = level.ClassLevel + level.RiskLevel;
            return this;
        }

        public class ExpCustomerDto
        {
            public string Deal { get; set; }
            public string CustomerNum { get; set; }
            public string CustomerName { get; set; }
            public string Collector { get; set; }
        }
    }
}
