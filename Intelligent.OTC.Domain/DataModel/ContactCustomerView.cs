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
    
    public partial class ContactCustomerView
    {
        public int Id { get; set; }
        public string Deal { get; set; }
        public string CustomerNum { get; set; }
        public string CustomerName { get; set; }
        public string BillGroupCode { get; set; }
        public string BillGroupName { get; set; }
        public string ExistCont { get; set; }
        public string CusStatus { get; set; }
        public string IsHoldFlg { get; set; }
        public string Operator { get; set; }
        public string Contact { get; set; }
        public string LegalEntity { get; set; }
        public Nullable<decimal> CreditLimit { get; set; }
        public Nullable<decimal> TotalAmt { get; set; }
        public Nullable<decimal> CurrentAmt { get; set; }
        public Nullable<decimal> FDueOver90Amt { get; set; }
        public Nullable<decimal> PastDueAmount { get; set; }
        public Nullable<decimal> Risk { get; set; }
        public Nullable<decimal> Value { get; set; }
        public string Class { get; set; }
        public string MailFlag { get; set; }
        public string States { get; set; }
        public string TrackStates { get; set; }
    }
}