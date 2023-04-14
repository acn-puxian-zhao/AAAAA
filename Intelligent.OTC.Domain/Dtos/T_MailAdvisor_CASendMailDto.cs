using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class T_MailAdvisor_CASendMailDto
    {
        public string Id { get; set; }
        public string BusinessId { get; set; }
        public string MailId { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string BodyFormat { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Cc { get; set; }
        public string Attachment { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
        public string MessageId { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
        public bool CAProcessFlag { get; set; }
        public bool MAProcessFlag { get; set; }
    }
}
