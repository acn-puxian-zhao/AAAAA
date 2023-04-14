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
    
    public partial class CollectorAlert
    {
        public int Id { get; set; }
        public string Eid { get; set; }
        public string Deal { get; set; }
        public string CustomerNum { get; set; }
        public System.DateTime ActionDate { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string ReferenceNo { get; set; }
        public int AlertType { get; set; }
        public string Status { get; set; }
        public string TaskId { get; set; }
        public string ProcessId { get; set; }
        public Nullable<int> PeriodId { get; set; }
        public Nullable<int> BatchType { get; set; }
        public string FailedReason { get; set; }
        public string CauseObjectNumber { get; set; }
        public string LegalEntity { get; set; }
        public string SiteUseId { get; set; }
        public Nullable<int> CollectionStrategyId { get; set; }
        public string Region { get; set; }
        public string ToTitle { get; set; }
        public string ToName { get; set; }
        public Nullable<System.DateTime> ResponseDate { get; set; }
        public string CCTitle { get; set; }
        public string TempleteLanguage { get; set; }
        public string Comment { get; set; }
        public string MessageId { get; set; }
        public bool isLasted { get; set; }
        public string GroupName { get; set; }
        public string TEMPLETELANGUAGE { get; set; }
    }
}