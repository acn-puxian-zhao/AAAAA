using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class GetSOAMailInstanceDto
    {
        public string customerNums { get; set; }
        public int templateId { get; set; }
        public string siteUseId { get; set; }
        public string templateType { get; set; }
        public string templatelang { get; set; }
        public List<int> intIds { get; set; }
    }
}
