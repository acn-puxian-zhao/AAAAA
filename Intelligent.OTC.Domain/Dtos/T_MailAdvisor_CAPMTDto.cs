using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class T_MailAdvisor_CAPMTDto
    {
        public string Id { get; set; }
        public string BusinessId { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
        public bool CAProcessFlag { get; set; }
        public bool MAProcessFlag { get; set; }
        public bool isLocked { get; set; }
    }
}
