using Intelligent.OTC.Domain.DataModel;
using System.Collections.Generic;

namespace Intelligent.OTC.Domain.Dtos
{
    public class CopyContactDto
    {
        public List<Contactor> Contactors { get; set; }
        public string CustomerNum { get; set; }
        public string SiteUseId { get; set; }
        public string Legal { get; set; }
    }

    public class ContactBatchUpdateDto
    {
        public string OldName { get; set; }
        public string OldEmail { get; set; }
        public string NewName { get; set; }
        public string NewEmail { get; set; }
    }
}
