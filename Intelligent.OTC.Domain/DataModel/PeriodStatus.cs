using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common.Attr;

namespace Intelligent.OTC.Domain.DataModel
{
    public enum PeriodStatus
    {
        [EnumCode("001")]
        Running,
        [EnumCode("002")]
        Close
    }
}
