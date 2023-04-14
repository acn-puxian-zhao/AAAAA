using Intelligent.OTC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.DataModel
{
    public partial class PeriodControl : IAggregateRoot
    {
        public string IsCurrentFlg { get; set; }

        public string Period { get; set; }
    }
}
