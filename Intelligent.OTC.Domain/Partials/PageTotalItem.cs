using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common;

namespace Intelligent.OTC.Domain.DataModel
{
    public partial class PageTotalItem
    {
        public int TotalNum { get; set; }
        public int TotalARBalance { get; set; }
        public int TotalPassDueAmt { get; set; }
        public int TotalOver90Days { get; set; }
        public int TotalCreditLimit{ get; set; }
    }
}
