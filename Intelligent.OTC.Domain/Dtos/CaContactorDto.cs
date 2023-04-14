using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public partial class CaContactorDto
    {
        public string CustomerNum { get; set; }

        public string SiteUseId { get; set; }

        public string Title { get; set; }

        public string Name { get; set; }

        public string EmailAddress { get; set; }
    }
}
