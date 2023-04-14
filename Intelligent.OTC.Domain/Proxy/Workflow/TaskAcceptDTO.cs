namespace Intelligent.OTC.Domain.Proxy.Workflow
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class TaskAcceptDTO
    {
        public long OperatorId { get; set; }

        public string ReferenceNo { get; set; }

        public long TaskId { get; set; }

        public Dictionary<string, object> Vars { get; set; }
    }
}

