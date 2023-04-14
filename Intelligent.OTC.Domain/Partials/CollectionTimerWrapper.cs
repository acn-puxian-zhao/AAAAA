using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace Intelligent.OTC.Domain.Partials
{
    public class CollectionTimerWrapper : Timer
    {
        public string Deal { get; set; }
    }
}
