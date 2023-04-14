
namespace Intelligent.OTC.Domain.DataModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Intelligent.OTC.Common;
    public class MyinvoicesDto
    {
        public int Id { get; set; }
        public string Deal { get; set; }
        public string LegalEntity { get; set; }
        public string CustomerNum { get; set; }
        public string CustomerName { get; set; }
        public string InvoiceNum { get; set; }
        public string GroupCodeOld { get; set; }
        public string GroupNameOld { get; set; }
        public string MstCustomer { get; set; }
        public string PoNum { get; set; }
        public string SoNum { get; set; }
        public string Class { get; set; }
        public string Currency { get; set; }
        public string Country { get; set; }
        public string States { get; set; }
        public string FuncCurrCode { get; set; }
        public Nullable<int> DaysLateSys { get; set; }
        //public string dayLate { get; set; }
        public Nullable<decimal> OriginalAmt { get; set; }
        public Nullable<decimal> BalanceAmt { get; set; }
        public string CreditTrem { get; set; }
        public string TrackStates { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime? PtpDate { get; set; }
        public string OverdueReason { get; set; }
        public string Collector { get; set; }
        public string CollectorName { get; set; }
        public string TeamName { get; set; }
        public string Remark { get; set; }
        public string Comments { get; set; }
        //Start add by xuan.wu for Arrow adding
        public string SiteUseId { get; set; }
        public string COLLECTOR_CONTACT { get; set; }
        public int? DAYS_LATE_SYS { get; set; }
        public string AgingBucket { get; set; }
        public string Ebname { get; set; }
        public DateTime? TRACK_DATE { get; set; }
        public DateTime? PTP_DATE { get; set; }
        public string COLLECTOR_NAME { get; set; }
        public string VAT_NO { get; set; }
        public string VAT_DATE { get; set; }
        public Nullable<DateTime> LastUpdateDate { get; set; }
        public Nullable<DateTime> PTP_Identified_Date { get; set; }
        public string PtpComment { get; set; }
        public string DisputeFlag { get; set; }
        public DateTime? Dispute_Identified_Date { get; set; }
        public string Dispute_Reason { get; set; }
        public string Owner_Department { get; set; }
        public DateTime? Next_Action_Date { get; set; }
        public string FinishedStatus { get; set; }
        public DateTime? Payment_Date { get; set; }
        public string DisputeStatus { get; set; }
        public string DisputeComment { get; set; }
        public string Forwarder { get; set; }
        public string IsForwarder { get; set; }
        public string isBanlance { get; set; }
        public string CS { get; set; }
        public string Sales { get; set; }
        public string BranchSalesFinance { get; set; }
        public string NotClear { get; set; }
        public string ConsignmentNumber { get; set; }
        public string BalanceMemo { get; set; }
        public DateTime? MemoExpirationDate { get; set; }
        public int IsExp { get; set; }

        //End add by xuan.wu for Arrow adding

        public string GroupCode {
            get 
            {
                return (!string.IsNullOrEmpty(GroupCodeOld) ? GroupCodeOld : CustomerName);
            }
        }

        public string GroupName
        {
            get
            {
                return (!string.IsNullOrEmpty(GroupNameOld) ? GroupNameOld : CustomerName);
            }
        }
        public string dayLate {

            get 
            {
               return (AppContext.Current.User.Now.Date - Convert.ToDateTime(DueDate).Date).Days.ToString();
            }
            
            }
    }
}
