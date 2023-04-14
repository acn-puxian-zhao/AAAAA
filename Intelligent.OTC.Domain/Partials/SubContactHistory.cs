using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common;

namespace Intelligent.OTC.Domain.DataModel
{
    public partial class SubContactHistory
    {
        public int SortId { get; set; }
        public string LegalEntity { get; set; }
        public string CustomerNum { get; set; }
        public string ContactType { get; set; }
        public System.DateTime ContactDate { get; set; }
        public string Deal { get; set; }
        public string ContactId { get; set; }
        public string ContacterId { get; set; }
        public string Comments { get; set; }
        public string SiteUseId { get; set; }
        //    public string GroupCode { get; set; }
    }
}
