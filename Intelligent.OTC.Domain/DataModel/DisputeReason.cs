using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common.Attr;

namespace Intelligent.OTC.Domain.DataModel
{
    public enum DisputeReason
    {
        [EnumCode("025001")]
        VAT_ISSUE,
        [EnumCode("025002")]
        LOGISTIC_ISSUE,
        [EnumCode("025003")]
        QUALITY_ISSUE,
        [EnumCode("025004")]
        INCORRECT_PRICING,
        [EnumCode("025005")]
        INCORRECT_QTY,
        [EnumCode("025006")]
        RMA,
        [EnumCode("025007")]
        WRONG_CID_BID,
        [EnumCode("025008")]
        DISCOUNT,
        [EnumCode("025009")]
        WRONG_SERVICE_BILLING_PERIOD,
        [EnumCode("025010")]
        CURRENCY_ISSUE,
        [EnumCode("025011")]
        WRONG_SHIP_TO,
        [EnumCode("025012")]
        WRONG_CREDIT,
        [EnumCode("025013")]
        PURE_DELINQUENCY,
        [EnumCode("025014")]
        WRONG_TABLE,
        [EnumCode("025015")]
        PAST_DUE,
        [EnumCode("025016")]
        OTHER,
    }
}
