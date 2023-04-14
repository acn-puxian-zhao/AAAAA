using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class MailMessageDto
    {
        public MailAttachmentDto[] Attachs { get; set; }
        public string Bcc { get; set; }
        public string Body { get; set; }
        public string CC { get; set; }
        public string Encoding { get; set; }
        public string From { get; set; }
        public bool IsBodyHtml { get; set; }
        public string MessageId { get; set; }
        public string ReplyTo { get; set; }
        public string Sender { get; set; }
        public string Subject { get; set; }
        public string To { get; set; }
    }
}
