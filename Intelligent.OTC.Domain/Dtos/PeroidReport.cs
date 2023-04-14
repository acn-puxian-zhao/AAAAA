
namespace Intelligent.OTC.Domain.DataModel
{
    using System;
    using System.Collections.Generic;

    public partial class PeroidReport : PeriodControl
    {
        public string statusFlg { get; set; }
        public string agingRepotFlg { get; set; }
        public string oneYearsFlg { get; set; }
        public string soaDoneFlg { get; set; }
        public string soaDone { get; set; }
        public int sortId { get; set; }
        public string StartDate { get; set; }
        //public string EndDate { get; set; }
    }
}
