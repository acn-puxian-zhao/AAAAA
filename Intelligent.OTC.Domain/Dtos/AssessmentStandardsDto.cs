namespace Intelligent.OTC.Domain.DataModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class AssessmentStandardsDto
    {
        public int Id { get; set; }
        public int DealId { get; set; }
        public string Name { get; set; }
        public decimal Excellent { get; set; }
        public decimal Good { get; set; }
        public decimal Issue { get; set; }
        public System.DateTime CreationTime { get; set; }
        public long CreatorUserId { get; set; }
        public System.DateTime LastModificationTime { get; set; }
        public long LastModifierUserId { get; set; }
    }
}
