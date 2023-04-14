using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common.Attr;

namespace Intelligent.OTC.Domain.DataModel
{
    public enum DisputeStatus
    {
        [EnumCode("026001")]
        Dispute_Identified,
        [EnumCode("026002")]
        Dispute_Confirmed,
        [EnumCode("026003")]
        Wait_for_1st_Time_Dispute_Contact,
        [EnumCode("026004")]
        Wait_for_1st_Time_Dispute_Response,
        [EnumCode("026005")]
        Wait_for_2nd_Time_Dispute_Contact,
        [EnumCode("026006")]
        Wait_for_2nd_Time_Dispute_Response,
        [EnumCode("026009")]
        Wait_for_Escalation,
        [EnumCode("026010")]
        Escalation,
        [EnumCode("026011")]
        Cancelled,
        [EnumCode("026012")]
        Closed,
    }
}
