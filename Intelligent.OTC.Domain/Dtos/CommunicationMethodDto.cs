namespace Intelligent.OTC.Domain.DataModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class CommunicationMethodDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsDisabled { get; set; }
        public System.DateTime CreationTime { get; set; }
        public long CreatorUserId { get; set; }
    }
}
