using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common;

namespace Intelligent.OTC.Domain.DataModel
{
    public partial class ContactCustomer : CustomerDetail
    {
        public string SpecialNotes { get; set; }
        public string[] LegalGroup { get; set; }
        public string[] CountryGroup { get; set; }
        public string TaskNo { get; set; }
    }
}
