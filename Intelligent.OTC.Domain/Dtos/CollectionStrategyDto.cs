namespace Intelligent.OTC.Domain.DataModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class CollectionStrategyDto
    {
        public int Id { get; set; }
        public int DealId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public System.DateTime CreationTime { get; set; }
        public long CreatorUserId { get; set; }
        public System.DateTime LastModificationTime { get; set; }
        public long LastModifierUserId { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsDeleted { get; set; }
        public Nullable<System.DateTime> DeletionTime { get; set; }
        public Nullable<long> DeleterUserId { get; set; }
        public string Confirm1CommunicationMethod { get; set; }
        public Nullable<int> Confirm1Days { get; set; }
        public string Confirm2CommunicationMethod { get; set; }
        public Nullable<int> Confirm2Days { get; set; }
        public string RemindingCommunicationMethod { get; set; }
        public Nullable<int> RemindingDays { get; set; }
        public string Dunning1CommunicationMethod { get; set; }
        public Nullable<int> Dunning1Days { get; set; }
        public string Dunning2CommunicationMethod { get; set; }
        public Nullable<int> Dunning2Days { get; set; }
    }
}
