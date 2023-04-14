using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common.Attr;

namespace Intelligent.OTC.Domain.DataModel
{
    public enum MailTemplateType
    {
        [EnumCode("001")]
        Confirm_PTP,
        [EnumCode("002")]
        First_Time_Dispute,
        [EnumCode("003")]
        Second_Time_Dispute,
        [EnumCode("010")]
        New,
        [EnumCode("005")]
        Reply,
        [EnumCode("006")]
        Foward,
        [EnumCode("007")]
        BreakPTP,
        [EnumCode("008")]
        HoldCustomer
        //[EnumCode("009")]
        //Others
    }
}
