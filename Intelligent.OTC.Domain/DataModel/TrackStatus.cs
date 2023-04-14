using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common.Attr;

namespace Intelligent.OTC.Domain.DataModel
{
    public enum TrackStatus
    {
        [EnumCode("000")]
        Open,
        [EnumCode("001")]
        Responsed_OverDue_Reason,
        [EnumCode("002")]
        Wait_for_2nd_Time_Confirm_PTP,
        [EnumCode("003")]
        PTP_Confirmed,
        [EnumCode("004")]
        Wait_for_Payment_Reminding,
        [EnumCode("005")]
        Wait_for_1st_Time_Dunning,
        [EnumCode("006")]
        Wait_for_2nd_Time_Dunning,
        [EnumCode("007")]
        Dispute_Identified,
        [EnumCode("008")]
        Wait_for_2nd_Time_Dispute_contact,
        [EnumCode("009")]
        Wait_for_1st_Time_Dispute_respond,
        [EnumCode("010")]
        Dispute_Resolved,
        [EnumCode("011")]
        Wait_for_2nd_Time_Dispute_respond,
        [EnumCode("012")]
        Escalation,
        [EnumCode("013")]
        Write_off_uncollectible_accounts,
        [EnumCode("014")]
        Closed,
        [EnumCode("015")]
        Payment_Notice_Received,
        [EnumCode("016")]
        Cancel,
        //[EnumCode("017")]
        //Close,
        //[EnumCode("018")]
        //Contra,
        //[EnumCode("019")]
        //Breakdown
    }
}
