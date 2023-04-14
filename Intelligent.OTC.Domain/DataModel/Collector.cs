using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Common;

namespace Intelligent.OTC.Domain.DataModel
{
    public class Collector : SysUser
    {
        public Collector()
        {
            AssigmentCount = -1;
        }
        public int AssigmentCount { get; set; }

        public static Collector ConvertFromUser(SysUser user)
        {
            Collector tc = null;
            if (user != null)
            {
                tc = new Collector();
                ObjectHelper.CopyObject(user, tc);
            }

            return tc;
        }

        public string ValueClass { get; set; }
    }
}
