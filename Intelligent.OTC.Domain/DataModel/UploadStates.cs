using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common.Attr;

namespace Intelligent.OTC.Domain.DataModel
{
    public enum UploadStates
    {
        [EnumCode("0")]
        Untreated,
        [EnumCode("1")]
        Success,
        [EnumCode("2")]
        Failed,
        [EnumCode("3")]
        Cancel,
        [EnumCode("4")]
        Submitted
    }
}
