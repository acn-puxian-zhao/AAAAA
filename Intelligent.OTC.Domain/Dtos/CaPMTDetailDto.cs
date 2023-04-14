using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public partial class CaPMTDetailDto
    {
        public string ID { get; set; }

        public string ReconId { get; set; }

        public string GroupNo { get; set; }

        public int? SortId { get; set; }

        public string CUSTOMER_NUM { get; set; }

        public string SiteUseId { get; set; }

        public string InvoiceNum { get; set; }

        public string Currency { get; set; }

        public string LocalCurrency { get; set; }

        public DateTime? InvoiceDate { get; set; }

        public DateTime? DueDate { get; set; }

        public Decimal? Amount { get; set; }

        public Decimal? LocalCurrencyAmount { get; set; }

        public string LegalEntity { get; set; }

        public string EBName { get; set; }

        public string Description { get; set; }

        public int? row { get; set; }

        public int? invIsClosed { get; set; }
    }
}
