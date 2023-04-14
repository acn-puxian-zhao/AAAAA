using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Intelligent.OTC.Domain.Dtos
{
    public class UserLoginResultDto
    {
        public bool Success { get; set; }

        public long Id { get; set; }

        public long EmployeeId { get; set; }

        public string UserName { get; set; }

        public bool IsAdmin { get; set; }

        public string Permissions { get; set; }

        public long[] GroupIds { get; set; }

        public long[] RoleIds { get; set; }

        public long[] PermissionIds { get; set; }

        public string UserCode { get; set; }

        public IDictionary<long, string> Subordinates { get; set; }

        //public string Country { get; set; }

        //public string Location { get; set; }

        //public string Deal { get; set; }

        //public string Vertical { get; set; }

        //public string Tower { get; set; }

        public string Process { get; set; }

        public string ProcessCode { get; set; }

        //public string SubProcess { get; set; }

        //public DefaultPortal DefaultPortal { get; set; }

        public string tokenId { get; set; }

    }
}
