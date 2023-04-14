namespace Intelligent.OTC.Domain.DataModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class AssessmentFactorDto
    {
        public int Id { get; set; }
        public int DealId { get; set; }
        public string FactorName { get; set; }
        public int Algorithm { get; set; }
        public int Weight { get; set; }
        public bool IsDisabled { get; set; }
        public string Description { get; set; }
        public System.DateTime CreationTime { get; set; }
        public long CreatorUserId { get; set; }
        public System.DateTime LastModificationTime { get; set; }
        public long LastMOdifierUserId { get; set; }
    }
}
