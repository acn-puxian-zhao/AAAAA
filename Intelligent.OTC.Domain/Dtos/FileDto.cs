using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class FileDto
    {
        public int Id { get; set; }

        public string FileId { get; set; }

        public string FileName { get; set; }

        public string Type { get; set; }

        public string PhysicalPath { get; set; }

        public string Operator { get; set; }

        public DateTime? CreateTime { get; set; }

        public DateTime? UpdateTime { get; set; }

        public string ContentType { get; set; }

        public string ContentId { get; set; }
    }
}
