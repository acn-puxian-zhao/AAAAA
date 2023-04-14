namespace Intelligent.OTC.Domain.DataModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class CustomerScoreDto
    {
        public int Id { get; set; }
        public int DealId { get; set; }
        public string CustomerId { get; set; }
        public string SiteUseId { get; set; }
        public int FactorId { get; set; }
        public decimal FactorValue { get; set; }
        public decimal FactorScore { get; set; }
        public Nullable<System.DateTime> LastModificationTime { get; set; }
        public Nullable<long> LastModifierUserId { get; set; }
        public int Version { get; set; }
        public string LegalEntity { get; set; }
        public decimal SourceValue1 { get; set; }
        public Nullable<decimal> SourceValue2 { get; set; }
        public Nullable<decimal> SourceValue3 { get; set; }
    }
}
