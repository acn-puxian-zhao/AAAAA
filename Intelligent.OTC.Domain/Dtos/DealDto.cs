namespace Intelligent.OTC.Domain.DataModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class DealDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDIsabled { get; set; }
        public System.DateTime CreationTime { get; set; }
        public long CreatorUserId { get; set; }
        public System.DateTime LastModificationTIme { get; set; }
        public long LastModifierUserId { get; set; }
        public bool IsDeleted { get; set; }
        public Nullable<System.DateTime> DeletionTime { get; set; }
        public Nullable<long> DeleterUserId { get; set; }
    }
}
