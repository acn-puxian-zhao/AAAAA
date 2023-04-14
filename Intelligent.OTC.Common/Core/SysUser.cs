using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common;
using System.Globalization;

namespace Intelligent.OTC.Common
{
    public class SysUser : IAggregateRoot
    {
        public int Id { get; set; }

        public string EID { get; set; }

        public string Name { get; set; }

        public bool IsAdmin { get; set; }

        public string Permissions { get; set; }

        public string ActionPermissions { get; set; }

        public long[] GroupIds { get; set; }

        public long[] RoleIds { get; set; }

        public IDictionary<long, string> Subordinates { get; set; }

        public string Process { get; set; }

        public string ProcessCode { get; set; }

        public long[] PermissionIds { get; set; }

        public string Email { get; set; }

        public int TimeZone { get; set; }

        public string tokenId { get; set; }

        public DateTime Now
        {
            get {
                return DateTime.Now;
            }
        }

        #region Orgnization
        public string Region { get; set; }
        public string RegionId { get; set; }

        public string Center { get; set; }
        public string CenterId { get; set; }

        public string Group { get; set; }
        public string GroupId { get; set; }

        public string Deal { get; set; }
        public string DealId { get; set; }

        public string Team { get; set; }
        public string TeamId { get; set; }
        #endregion
    }
}
