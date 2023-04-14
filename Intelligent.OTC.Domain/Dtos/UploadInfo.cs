
namespace Intelligent.OTC.Domain.DataModel
{
    using System;
    using System.Collections.Generic;

    public partial class UploadInfo
    {
        public string LegalEntity { get; set; }
        public int? AccTimes { get; set; }
        public int InvTimes { get; set; }
        public string ReportTime { get; set; }
        public string Operator { get; set; }
        public int OneYearTimes { get; set; }
    }
}
