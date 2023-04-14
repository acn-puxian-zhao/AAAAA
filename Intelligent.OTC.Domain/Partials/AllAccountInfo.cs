using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common;
namespace Intelligent.OTC.Domain.DataModel
{
    public partial class AllAccountInfo
    {
        public int Id
        {
            get;
            set;
        } 
        public string LegalEntity { get; set; }
        public string CustomerNum { get; set; }
        public string CustomerName { get; set; }
        public string BillGroupCode { get; set; }
        public string BillGroupName { get; set; }
        public string Class { get; set; }
        public Nullable<decimal> ArBalanceAmtPeroid { get; set; }
        public Nullable<decimal> BalanceAmt { get; set; }
        public Nullable<decimal> PaidAmt { get; set; }
        public Nullable<decimal> OverDue90Amt { get; set; }
        public Nullable<decimal> PtpAmt { get; set; }
        public Nullable<decimal> BrokenPTPAmt { get; set; }
        public Nullable<decimal> DisputeAmt { get; set; }
        public Nullable<decimal> UnapplidPayment { get; set; }
        public Nullable<DateTime> DueDate { get; set; }
        public string States { get; set; }
        public Nullable<DateTime> CreateDate { get; set; }
        public string PaymentTerm { get; set; }
        public Nullable<DateTime> SoaDate { get; set; }
        public Nullable<DateTime> SecondDate { get; set; }
        public Nullable<DateTime> FinalDate { get; set; }
        public string SpecialNotes { get; set; }
        public decimal? AdjustedOver90 { get; set; }
        public string Collector { get; set; }
        public string Team { get; set; }
        public string Country { get; set; }
        public IEnumerable<string> ContactList { get; set; }
        public string Contact
        {
            get
            {
                StringBuilder res = new StringBuilder();
                ContactList.ToList().ForEach(c => res.Append(c).Append(","));
                return res.ToString().TrimEnd(',');
            }
        }
        //Start add by xuan.wu for Arrow adding
        public string SiteUseId { get; set; }
        public string COLLECTOR_CONTACT { get; set; }
        public string CS { get; set; }
        public string Sales { get; set; }
        public Nullable<decimal> CreditLimit { get; set; }
        public string CreditTremDescription { get; set; }
        public Nullable<decimal> TotalFutureDue { get; set; }
        public Nullable<decimal> PastDueAmount { get; set; }
        public string FinishedStatus { get; set; }
        public string Comment { get; set; }
        //End add by xuan.wu for Arrow adding

        public Nullable<decimal> AccountPtpAmount { get; set; }

    }
}
