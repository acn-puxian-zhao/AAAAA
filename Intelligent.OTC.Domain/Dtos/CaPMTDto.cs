using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public partial class CaPMTDto
    {
        public string ID { get; set; }

        public string LegalEntity { get; set; }

        public string GroupNo { get; set; }

        public string CustomerNum { get; set; }
        public string CustomerName { get; set; }

        public string Currency { get; set; }
        public string LocalCurrency { get; set; }

        public Decimal? Amount { get; set; }

        public Decimal? TransactionAmount { get; set; }

        public Decimal? BankCharge { get; set; }
        public Decimal? LocalCurrencyAmount { get; set; }

        public string TASK_ID { get; set; }

        public bool? ISAPPLYGROUP { get; set; }

        public bool? ISPOSTGROUP { get; set; }

        public string CREATE_USER { get; set; }
        public string CREATE_USERName { get; set; }

        public DateTime? CREATE_DATE { get; set; }

        public string UPDATE_USER { get; set; }

        public DateTime? UPDATE_DATE { get; set; }

        public bool? isClosed { get; set; }

        public bool? deleteRecon { get; set; }
        public int? hasbs { get; set; }

        public int? hasMatched { get; set; }

        public int? hasinv { get; set; }

        public DateTime? ValueDate { get; set; }

        public DateTime? ReceiveDate { get; set; }

        public string SiteUseId { get; set; }

        public string filename { get; set; }
        public string businessId { get; set; }

        public List<CaPMTBSDto> PmtBs;
        public List<CaPMTDetailDto> PmtDetail;
    }
}
