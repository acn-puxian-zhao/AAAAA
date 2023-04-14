namespace Intelligent.OTC.Domain.DataModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class CustomerAssessmentHistoryDto
    {
        public int Id { get; set; }
        public string Version { get; set; }
        public int DealId { get; set; }
        public string LegalEntity { get; set; }
        public string CustomerId { get; set; }
        public string SiteUseId { get; set; }
        public decimal AssessmentScore { get; set; }
        public int AssessmentType { get; set; }
        public System.DateTime? LastModificationTime { get; set; }
        public long? LastModifierUserId { get; set; }
    
        public virtual T_Deal T_Deal { get; set; }
    }
}
