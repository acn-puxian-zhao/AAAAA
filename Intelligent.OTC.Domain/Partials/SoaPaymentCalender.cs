using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common;

namespace Intelligent.OTC.Domain.DataModel
{
    public partial class SoaPaymentCalender
    {
        public int sortId { get; set; }
        public System.DateTime PaymentDay { get; set; }
        public string WeekDay { get; set; }
    }
}
