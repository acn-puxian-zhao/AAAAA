using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class SendContactorNameDto
    {
        public string sender { get; set; }
        public List<ContactorNameDto> names { get; set; }
    }
}
