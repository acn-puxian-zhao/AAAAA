namespace Intelligent.OTC.Domain.Proxy.Workflow
{
    using System;
    using System.Runtime.CompilerServices;

    public class TaskResult
    {
        private bool _Success = true;

        public long ActivityId { get; set; }

        public string ActivityName { get; set; }

        public string BusinessStatusId { get; set; }

        public string Employees { get; set; }

        public string Groups { get; set; }

        public bool IsProcessCompleted { get; set; }

        public string Message { get; set; }

        public int Priority { get; set; }

        public string ReferenceNo { get; set; }

        public string Roles { get; set; }

        public string Status { get; set; }

        public bool Success
        {
            get
            {
                return this._Success;
            }
            set
            {
                this._Success = value;
            }
        }

        public long TaskId { get; set; }
    }
}

