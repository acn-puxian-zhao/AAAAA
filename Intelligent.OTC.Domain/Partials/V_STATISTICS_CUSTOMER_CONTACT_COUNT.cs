using Intelligent.OTC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.DataModel
{
    public partial class V_STATISTICS_CUSTOMER_CONTACT_COUNT : IAggregateRoot
    {
        public int Id { get; set; }
    }
}
