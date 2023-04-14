using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OTC.POC.Common.Attr;

namespace OTC.POC.Repository.DataModel
{
    public enum SOAstatus
    {
        [EnumCode("0")]
        Collector,
        [EnumCode("1")]
        DataProcessor,
        [EnumCode("2")]
        Start SOA Task
    }
}
