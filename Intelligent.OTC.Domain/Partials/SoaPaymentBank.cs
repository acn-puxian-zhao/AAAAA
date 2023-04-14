using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common;

namespace Intelligent.OTC.Domain.DataModel
{
    public partial class SoaPaymentBank
    {
        public string AccountName { get; set; }
        public string BankName { get; set; }
        public string BankAccount { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string CreatePerson { get; set; }
        public string InUse { get; set; }
        public string Description { get; set; }
    }
}
