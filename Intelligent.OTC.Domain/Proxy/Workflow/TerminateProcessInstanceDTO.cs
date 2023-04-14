namespace Intelligent.OTC.Domain.Proxy.Workflow
{
    using System;
    using System.Runtime.CompilerServices;

    public class TerminateProcessInstanceDTO
    {
        public long ProcessId { get; set; }

        public string ProcessInstanceId { get; set; }

        public string ReferenceNo { get; set; }
    }
}

