using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public partial class CaPMTHisDto
    {
        public int GroupNo { get; set; }
        public string CustomerNum { get; set; }
        public string CustomerName { get; set; }
        public string LegalEntity { get; set; }
        public int BSNo { get; set; }
        public string TransactionINC { get; set; }
        public DateTime? ValueDate { get; set; }
        public DateTime? ReceiveDate { get; set; }
        public Decimal? BSAmount { get; set; }
        public Decimal? BSClearAmount { get; set; }
        public Decimal? ARAmount { get; set; }
        public Decimal? ClearAmount { get; set; }
        public Decimal? FxRateClearAmount { get; set; }
        public string BSCurrency { get; set; }
        public string BSDescription { get; set; }
        public int No { get; set; }
        public string InvNo { get; set; }
        public string FuncCurrency { get; set; }
        public string FxRateCurrency { get; set; }
        public string InvDescription { get; set; }
        public int row { get; set; }
        public string SiteUseId { get; set; }
       

    }
}
