using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public partial class VCustomerAssessmentDto
    {
        public string Index { get; set; }
        public string ItemCount { get; set; }
        public string LegalEntity { get; set; }
        public string AssessmentDate { get; set; }
        public string CustomerNum { get; set; }
        public string AssessmentType { get; set; }
        public string SiteUseId { get; set; }
        public string CustomerName { get; set; }
    }
}
