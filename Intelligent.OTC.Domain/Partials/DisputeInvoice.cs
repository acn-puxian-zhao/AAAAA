using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common;

namespace Intelligent.OTC.Domain.DataModel
{
    public partial class DisputeInvoice : IAggregateRoot
    {
        public string customerNum { get; set; }
        public int[] invoiceIds { get; set; }
        public string contactId { get; set; }
        public string relatedEmail { get; set; }
        public string contactPerson { get; set; }
        public string comments { get; set; }
        public string issue { get; set; }
        public string LegalEntity { get; set; }
        public string siteUseId { get; set; }
        public string callContact { get; set; }
        public string actionOwnerDepartment { get; set; }
    }
}
