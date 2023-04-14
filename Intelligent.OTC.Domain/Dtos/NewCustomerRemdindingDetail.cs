using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class NewCustomerRemdindingDetail
    {
        public string Collector { get; set; }
        public string Region { get; set; }
        public string Organization { get; set; }
        public string Ebname { get; set; }
        public string CreditTerm { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNum { get; set; }
        public string SiteUseId { get; set; }
        public string Sales { get; set; }
        public string CreateDate { get; set; }
    }
}
