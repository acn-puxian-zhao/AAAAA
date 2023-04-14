
namespace Intelligent.OTC.Domain.DataModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class CustomerMasterDto
    {
        public int Id { get; set; }
        public string Deal { get; set; }
        public string CustomerNum { get; set; }
        public string CustomerName { get; set; }
        public string BillGroupCode { get; set; }
        public string BillGroupName { get; set; }
        public string Collector { get; set; }
        public string siteUseId { get; set; }
        public string SoaTemplete { get; set; }
    }
}
