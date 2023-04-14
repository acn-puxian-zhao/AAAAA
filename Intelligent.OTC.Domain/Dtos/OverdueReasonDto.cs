using System.Collections.Generic;

namespace Intelligent.OTC.Domain.Dtos
{
    public class OverdueReasonDto
    {
        public List<string> InvoiceNums { get; set; }

        public string Reason { get; set; }

        public string Comments { get; set; }
    }
}
