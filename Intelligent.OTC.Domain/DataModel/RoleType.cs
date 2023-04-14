using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common.Attr;

namespace Intelligent.OTC.Domain.DataModel
{
    public enum RoleType
    {
        [EnumCode("002")]
        Collector,
        [EnumCode("001")]
        DataProcessor,
        [EnumCode("003")]
        TeamLead,
        [EnumCode("004")]
        Administrator,
    }
}
