using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common;

namespace Intelligent.OTC.Domain.DataModel
{
    public partial class CustomerPaymentBank : IAggregateRoot
	{
        public string InUse { get; set; }
	}
}
