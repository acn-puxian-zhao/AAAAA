//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Intelligent.OTC.Domain.DataModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class V_CustomerAssessment
    {
        public int Id { get; set; }
        public int DealId { get; set; }
        public string CustomerId { get; set; }
        public string SiteUseId { get; set; }
        public decimal AssessmentScore { get; set; }
        public int AssessmentType { get; set; }
        public Nullable<System.DateTime> LastModificationTime { get; set; }
        public Nullable<long> LastModifierUserId { get; set; }
        public string LegalEntity { get; set; }
        public int Expr1 { get; set; }
        public string CUSTOMER_NUM { get; set; }
        public string CUSTOMER_NAME { get; set; }
        public string DEAL { get; set; }
        public string BILL_GROUP_CODE { get; set; }
        public string COUNTRY { get; set; }
        public string COLLECTOR { get; set; }
        public string CREDIT_TREM { get; set; }
        public string SALES { get; set; }
        public string SPECIAL_NOTES { get; set; }
        public string PAY_CYCLE { get; set; }
        public string AUTO_REMINDER_FLG { get; set; }
        public string IS_HOLD_FLG { get; set; }
        public string EXCLUDE_FLG { get; set; }
        public string DESCRIPTION { get; set; }
        public string OPERATOR { get; set; }
        public string CONTACT_LANGUAGE { get; set; }
        public Nullable<decimal> CREDIT_LIMIT { get; set; }
        public Nullable<int> PAYMENT_TAT { get; set; }
        public string SOA_FLG { get; set; }
        public string REMOVE_FLG { get; set; }
        public Nullable<System.DateTime> CREATE_TIME { get; set; }
        public Nullable<System.DateTime> UPDATE_TIME { get; set; }
        public string COLLECTOR_NAME { get; set; }
        public string PARTY_TYPE { get; set; }
        public string PARTY_NUMBER { get; set; }
        public string TERRITORY_NAME { get; set; }
        public string SITE_NO { get; set; }
        public string COUNTRY_CODE { get; set; }
        public string STATUS { get; set; }
        public string SITE_USE_CODE { get; set; }
        public string LOCALIZE_CUSTOMER_NAME { get; set; }
        public string PARTY_SITE_NAME { get; set; }
        public string ALTERNATE_NAME { get; set; }
        public string TRANSLATED_CUSTOMER_NAME { get; set; }
        public string CUSTOMER_SERVICE { get; set; }
        public Nullable<decimal> AMTLimit { get; set; }
        public string Organization { get; set; }
        public string FSR { get; set; }
        public string LSR { get; set; }
        public Nullable<System.DateTime> LastSendDate { get; set; }
        public Nullable<bool> IsAMS { get; set; }
        public Nullable<bool> IsVAT { get; set; }
        public string atName { get; set; }
        public Nullable<int> Rank { get; set; }
    }
}