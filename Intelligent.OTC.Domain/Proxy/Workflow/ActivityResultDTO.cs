namespace Intelligent.OTC.Domain.Proxy.Workflow
{
    using System;
    using System.Runtime.CompilerServices;

    public class ActivityResultDTO
    {
        public long ActivityId { get; set; }

        public string ActivityName { get; set; }

        public long ProcessId { get; set; }

        public long StatusId { get; set; }
    }
}

