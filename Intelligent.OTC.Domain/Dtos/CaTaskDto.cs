using System;
using System.Collections.Generic;

namespace Intelligent.OTC.Domain.Dtos
{
   public class CaTaskDto
    {
        public string  Id { get; set; }
        public string TaskType { get; set; }
        public string TaskTypeName { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
        public string TaskName { get; set; }
        public bool DelFlag { get; set; }
        public string Status { get; set; }
        public string StatusName { get; set; }
        public string FileId { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string CreateUser { get; set; }

        public int? resultFileFlag { get; set; }
    }

    public class CaTaskDtoPage
    {
        public List<CaTaskDto> dataRows;

        public int count;
    }
}
