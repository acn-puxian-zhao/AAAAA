using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class SendSoaRemindingDetail
    {
        public string status { get; set; }
        public string eid { get; set; }
        public string region { get; set; }
        public string totitle { get; set; }
        public string toname { get; set; }
        public string cctitle { get; set; }
        public string comment { get; set; }
        public string siteuseidlist { get; set; }
        public string mailfrom { get; set; }
        public string mailto { get; set; }
        public string mailcc { get; set; }
        public string mailsubject { get; set; }
    }
}
