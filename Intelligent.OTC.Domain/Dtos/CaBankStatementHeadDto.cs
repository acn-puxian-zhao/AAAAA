using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class CaBankStatementHeadDto
    {
        public int Id { get; set; }
        public string LegalEntity { get; set; }
        public string ColumnName { get; set; }
        public int SortId { get; set; }
        public string FileTitle { get; set; }
        public string ValueSum { get; set; }
    }
}
