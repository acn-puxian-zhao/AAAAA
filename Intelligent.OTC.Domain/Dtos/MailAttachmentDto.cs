using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class MailAttachmentDto
    {
        public byte[] Content { get; set; }
        public string Encoding { get; set; }
        public string FileName { get; set; }
        public string Name { get; set; }
    }
}
