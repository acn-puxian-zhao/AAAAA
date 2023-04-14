using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public partial class CaReconBSDto
    {
        public string ID { get; set; }

        public string ReconId { get; set; }

        public int? SortId { get; set; }

        public string BANK_STATEMENT_ID { get; set; }

        public string Currency { get; set; }

        public Decimal? Amount { get; set; }

        public int? row { get; set; }
    }

    public class CaReconBSDtoPage
    {
        public List<CaReconBSDto> dataRows;

        public int count;
    }
}
