using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common.Attr;

namespace Intelligent.OTC.Domain.DataModel
{
    public enum FileType
    {
        [EnumCode("001")]
        Account,
        [EnumCode("002")]
        Invoice,
        [EnumCode("003")]
        OneYearSales,
        [EnumCode("004")]
        ConsolidateReport,
        [EnumCode("005")]
        PaymentDateCircle,
        [EnumCode("006")]
        Customer,
        [EnumCode("007")]
        MailAttachment,
        [EnumCode("008")]
        SOA,
        [EnumCode("009")]
        ReceivedMail,
        [EnumCode("010")]
        SentMail,
        [EnumCode("011")]
        MailBodyPart,
        [EnumCode("012")]
        DailyReport,
        [EnumCode("013")]
        InvoiceDetail,
        [EnumCode("014")]
        VAT,
        [EnumCode("015")]
        AccountPeriod,
        [EnumCode("016")]
        CustLocalize,
        [EnumCode("017")]
        VarData,
        [EnumCode("018")]
        CustPayment,
        [EnumCode("019")]
        SAPInvoice,
        [EnumCode("020")]
        MissingContactor,
        [EnumCode("021")]
        PMTExport,
        [EnumCode("022")]
        ContactorReplace,
        [EnumCode("023")]
        CreditHold,
        [EnumCode("024")]
        CurrencyAmount,
        [EnumCode("025")]
        CustComment,
        [EnumCode("026")]
        CustEBBranch,
        [EnumCode("027")]
        CustLitigation,
        [EnumCode("028")]
        CustBadDebt,
        [EnumCode("029")]
        ConsigmentNumber,
        [EnumCode("030")]
        CustCommentsFromCsSales,
        [EnumCode("031")]
        EBSetting

    }
}
