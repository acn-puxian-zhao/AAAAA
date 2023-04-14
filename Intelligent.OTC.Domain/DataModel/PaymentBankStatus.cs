using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common.Attr;

namespace Intelligent.OTC.Domain.DataModel
{
    public enum PaymentBankStatus
    {
        [EnumCode("0")]
        Invalid,
        [EnumCode("1")]
        Valid
    }
}
