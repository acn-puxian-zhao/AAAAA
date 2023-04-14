using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain
{
    /// <summary>
    /// add by llf,上传页面 最下方表格用
    /// </summary>
    public class UploadLegalHis
    {
        public string LegalEntity { get; set; }
        public string FileType { get; set; }
        public string ReportName { get; set; }
        public string Operator { get; set; }
        public string OperatorDate { get; set; }
        public string DownLoadFlg { get; set; }
    }
}
