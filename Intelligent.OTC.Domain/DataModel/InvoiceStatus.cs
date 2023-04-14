using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common.Attr;

namespace Intelligent.OTC.Domain.DataModel
{
    public enum InvoiceStatus
    {
        [EnumCode("004001")]
        Open,
        [EnumCode("004002")]
        PTP,
        [EnumCode("004003")]
        Paid,
        [EnumCode("004004")]
        Dispute,
        [EnumCode("004005")]
        Cancelled,
        [EnumCode("004006")]
        Uncollectable,
        [EnumCode("004007")]
        WriteOff,
        [EnumCode("004008")]
        PartialPay,
        [EnumCode("004009")]
        Closed,
        [EnumCode("004010")]
        Broken_PTP,
        [EnumCode("004011")]
        Hold,
        [EnumCode("004012")]
        Payment
    }
}
