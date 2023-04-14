using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common;

namespace Intelligent.OTC.Domain.DataModel
{
    public partial class InvoiceLog : IAggregateRoot
    {
        public string RelatedEmail { get; set; }
        public Nullable<System.DateTime> PtpDate { get; set; }
        public int[] invoiceIds { get; set; }
        public string legalEntity { get; set; }
    }
}
