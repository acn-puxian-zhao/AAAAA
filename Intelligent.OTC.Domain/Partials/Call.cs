using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common;

namespace Intelligent.OTC.Domain.DataModel
{
    public partial class Call : IAggregateRoot
    {
        public string customerNum { get; set; }
        public int[] invoiceIds { get; set; }
        public string contacterId { get; set; }
        public string siteuseId { get; set; }
        public string logAction { get; set; }
        public string LegalEntity { get; set; }
    }
}
