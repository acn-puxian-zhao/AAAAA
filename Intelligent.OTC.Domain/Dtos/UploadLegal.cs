using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain
{
    /// <summary>
    /// add by llf,上传页面 左上角表格用
    /// </summary>
    public class UploadLegal
    {
        public string LegalEntity { get; set; }
        public bool StateAcc { get; set; }
        public bool StateInv { get; set; }
        public bool StateInvDet { get; set; }
        public bool StateVat { get; set; }      
    }
}
