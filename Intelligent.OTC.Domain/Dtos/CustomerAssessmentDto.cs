namespace Intelligent.OTC.Domain.DataModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class CustomerAssessmentDto
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
    }
}
