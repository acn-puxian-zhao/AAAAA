using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public partial class CaPMTBSDto
    {
        public string ID { get; set; }

        public string ReconId { get; set; }

        public int? SortId { get; set; }

        public string BANK_STATEMENT_ID { get; set; }

        public string Currency { get; set; }

        public Decimal? Amount { get; set; }

        public Decimal? BankCharge { get; set; }

        public int? row { get; set; }

        public string TransactionNumber { get; set; }

        public DateTime? ValueDate { get; set; }
        public string Description { get; set; }
        public string REF1 { get; set; }

        public string LegalEntity { get; set; }

        public Decimal? TransactionAmount { get; set; }
        public Decimal? currentAmount { get; set; }
    }
}
