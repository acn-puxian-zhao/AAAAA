using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class ContactorDto
    {
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public string ToCc { get; set; }
    }
}
