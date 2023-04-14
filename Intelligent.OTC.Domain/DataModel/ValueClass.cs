using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common.Attr;

namespace Intelligent.OTC.Domain.DataModel
{
    public enum ValueClass
    {
        [EnumCode("002")]
        lowValue,
        [EnumCode("001")]
        HighValue
    }
}
