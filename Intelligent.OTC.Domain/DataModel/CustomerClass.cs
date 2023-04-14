using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common.Attr;

namespace Intelligent.OTC.Domain.DataModel
{
    public enum CustomerClass
    {
        [EnumCode("001")]
        AMS_Excellent,
        [EnumCode("002")]
        AMS_Good,
        [EnumCode("003")]
        AMS_Issue,
        [EnumCode("004")]
        NonAMS_Excellent,
        [EnumCode("005")]
        NonAMS_Good,
        [EnumCode("006")]
        NonAMS_Issue,
        [EnumCode("007")]
        Prepaid,
        [EnumCode("008")]
        New_Customer,
        [EnumCode("009")]
        White_List,
        [EnumCode("010")]
        Black_List,
        [EnumCode("101")]
        LV,
        [EnumCode("102")]
        HV,
        [EnumCode("201")]
        LR,
        [EnumCode("202")]
        HR
    }
}
