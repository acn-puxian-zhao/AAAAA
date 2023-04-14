using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class LegalEntityAssessmentDateDto
    {
        public string id { get; set; }

        public string LegalEntity { get; set; }

        public DateTime? AD { get; set; }

        public string AssessmentDate { get; set; }
    }
}
