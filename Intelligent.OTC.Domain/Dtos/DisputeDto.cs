using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.DataModel
{
    public class DisputeDto
    {
        public int Id { get; set; }
        public string Deal { get; set; }
        public string CustomerNum { get; set; }
        public string SiteUseId { get; set; }
        public string InvoiceNum { get; set; }
    }
}
