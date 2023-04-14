using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.DomainModel
{
    public class DailyAgingDto
    {
        public int ID { get; set; }

        public string legalEntity { get; set; }

        public string CustomerName { get; set; }

        public string AccntNumber { get; set; }

        public string SiteUseId { get; set; }

        public string PaymentTermDesc { get; set; }

        public string Ebname { get; set; }

        public decimal? OverCreditLmt { get; set; }

        public string Collector {get;set;}

        public string LocalizeCustomerName { get; set; }

        public string FuncCurrCode { get; set; }

        public decimal? TotalOverDue { get; set; }

        public decimal? TotalFutureDue { get; set; }

        #region 2018-02-28 追加

        public decimal? CustomerODPercent { get; set; }

        public decimal? DisputeODPercent { get; set; }

        public decimal? PtpODPercent { get; set; }

        public decimal? OthersODPercent { get; set; }

        public string DisputeAnalysis { get; set; }

        public string AutomaticSendMailDate { get; set; }

        public int? AutomaticSendMailCount { get; set; }

        public string FollowUpCallDate { get; set; }

        public int? FollowUpCallCount { get; set; }

        public string CurrentMonthCustomerContact { get; set; }

        #endregion 2018-02-28 追加

        public decimal? Due15Amt { get; set; }

        public decimal? Due30Amt { get; set; }

        public decimal? Due45Amt { get; set; }

        public decimal? Due60Amt { get; set; }

        public decimal? Due90Amt { get; set; }

        public decimal? Due120Amt { get; set; }

        public decimal? Due180Amt { get; set; }

        public decimal? Due270Amt { get; set; }

        public decimal? Due360Amt { get; set; }

        public decimal? LargerDue120Amt { get; set; }

        public decimal? DueOver360Amt { get; set; }

        public decimal? TotalAR { get; set; }

        public string InvoiceMemo { get; set; }

        public string PTPComment { get; set; }

        public decimal? TotalPTPAmount { get; set; }

        public string DisputeComment { get; set; }

        public decimal? DisputeAmount { get; set; }

        public string SpecialNote { get; set; }

        public string Lsr { get; set; }

        public string Fsr { get; set; }

        public int count { get; set; }

        public string comments { get; set; }

        public DateTime? CommentLastDate { get; set; }
        public DateTime? CommentExpirationDate { get; set; }

    }
}
