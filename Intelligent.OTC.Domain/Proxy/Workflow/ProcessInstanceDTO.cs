namespace Intelligent.OTC.Domain.Proxy.Workflow
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class ProcessInstanceDTO
    {
        public ProcessInstanceDTO()
        {
            this.Vars = new Dictionary<string, object>();
        }

        public long OperatorId { get; set; }

        public long ProcessId { get; set; }

        public string ReferenceNo { get; set; }

        public Dictionary<string, object> Vars { get; set; }
    }
}

