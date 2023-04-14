
namespace Intelligent.OTC.Domain.DataModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class CustomerLevelDto
    {
        public int Id { get; set; }
        public string Deal { get; set; }
        public string CustomerNum { get; set; }
        public string CustomerName { get; set; }
        public string BillGroupCode { get; set; }
        public string BillGroupName { get; set; }
        public string Collector { get; set; }
        public Nullable<decimal> Risk { get; set; }
        public Nullable<decimal> Value { get; set; }
        public string ValueLevel { get; set; }
        public string RiskLevel { get; set; }
        public string Class { get; set; }
        //Start add by xuan.wu for Arrow adding
        public string SiteUseId { get; set; }
        public string CS { get; set; }
        public string Sales { get; set; }
        //End add by xuan.wu for Arrow adding
    }
}
