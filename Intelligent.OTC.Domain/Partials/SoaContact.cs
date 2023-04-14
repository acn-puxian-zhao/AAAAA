using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common;

namespace Intelligent.OTC.Domain.DataModel
{
    public partial class SoaContact
    {
        public string ContactName { get; set; }
        public string Department { get; set; }
        public string Title { get; set; }
        public string ContactNumber { get; set; }
        public string Email { get; set; }
        public string Comment { get; set; }
    }
}
