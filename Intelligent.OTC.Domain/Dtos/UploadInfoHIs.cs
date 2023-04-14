
namespace Intelligent.OTC.Domain.DataModel
{
    using System;
    using System.Collections.Generic;

    public partial class UploadInfoHis
    {
        public string Period { get; set; }
        public string FileType { get; set; }
        public string Operator { get; set; }
        public DateTime? OperatorDate { get; set; }
        public string UploadFileName { get; set; }
        public string UploadFileFullName { get; set; }
        public string PeriodFlg { get; set; }
        public string DownLoadFullName { get; set; }
        public string DownLoadFlg { get; set; }
        public string DownLoadShowFlg { get; set; }
        public int sortCode { get; set; }
    }
}
