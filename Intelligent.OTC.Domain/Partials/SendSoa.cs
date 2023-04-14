using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common;

namespace Intelligent.OTC.Domain.DataModel
{
    public partial class SendSoaHead
    {
        public string Deal { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public Nullable<decimal> TotalBalance { get; set; }
        public string CustomerClass { get; set; }
    //    public string GroupCode { get; set; }
        public List<SoaLegal> SubLegal { get; set; }
        public List<SubContactHistory> SubContactHistory { get; set; }
        public List<Dispute> SubDisputeList { get; set; }
        //public string[] LegalGroup { get; set; }
        //public Nullable<decimal> CreditLimit { get; set; }
        //public Nullable<decimal> PastDueAmount { get; set; }
        //public string CustomerScore { get; set; }
        //public string[] CountryGroup { get; set; }
        //public Nullable<decimal> CreditBalance { get; set; }
        //public Nullable<decimal> FCollectableAmount { get; set; }
        //public Nullable<decimal> FOverdue90Amount { get; set; }
        //public string SpecialNotes { get; set; }
        //public List<SoaInvoice> SubInvoice { get; set; }
        //public List<SoaContact> SubContact { get; set; }
        //public List<SoaPaymentBank> SubPaymentBank { get; set; }
        //public List<SoaPaymentCalender> SubPaymentCalender { get; set; }
        //Start add by xuan.wu for Arrow adding
        public string SiteUseId { get; set; }
        public string LegalEntity { get; set; }
        public string Sales { get; set; }
        public Nullable<decimal> CreditLimit { get; set; }//Credit Limit
        public string CreditTremDescription { get; set; }//Payment Term
        public Nullable<decimal> Amount { get; set; }//F-Collectable Amount
        public Nullable<decimal> Total_Balance { get; set; }//Total Balance
        public Nullable<decimal> Current_Balance { get; set; }//Current Balance
        public int Count1PTP { get; set; }                                 //Wait_for_1st_Time_Confirm_PTP
        public int Count2PTP { get; set; }                                 //Wait_for_2nd_Time_Confirm_PTP
        public int CountPaymentReminding { get; set; }          //Wait_for_Payment_Reminding
        public int Count1Dunning { get; set; }                          //Wait_for_1st_Time_Dunning
        public int Count2Dunning { get; set; }                          //Wait_for_1st_Time_Dunning
        public int DunFlag { get; set; }
        public string Assessment { get; set; }//assessment type
        public string Eb { get; set; }
        //End add by xuan.wu for Arrow adding

        public string IsCostomerContact { get; set; }
        public DateTime ReconciliationDay { get; set; }
        public string comment { get; set; }

        public DateTime? CommentLastDate { get; set; }
        public DateTime? CommentExpirationDate { get; set; }

    }
}
