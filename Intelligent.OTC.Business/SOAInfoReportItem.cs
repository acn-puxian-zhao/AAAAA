using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Intelligent.OTC.Business
{
    public class SOAInfoReportItem
    {
        public List<Tuple<string, decimal>> overdueCharge=new List<Tuple<string, decimal>> ();
       public List<Tuple<string, decimal>> currentCharge=new List<Tuple<string, decimal>> ();

        public string LegalEntity  { get; set; }
        public string BillGroupCode { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNum { get; set; }

    }
}
