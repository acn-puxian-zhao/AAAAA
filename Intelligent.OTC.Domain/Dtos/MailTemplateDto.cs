using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class MailTemplateDto
    {
        public int Id { get; set; }
        public string Deal { get; set; }
        public string Language { get; set; }
        public string LanguageName { get; set; }
        public string Type { get; set; }
        public string TypeName { get; set; }
        public string Subject { get; set; }
        public string MainBody { get; set; }
        public string Creater { get; set; }
        public System.DateTime CreateDate { get; set; }
    }
}
