using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.DataModel
{
    public partial class MailInvoiceDto
    {
        public int Id { get; set; }
        public string Deal { get; set; }
        public string ImprortId { get; set; }
        public string LegalEntity { get; set; }
        public string CustomerNum { get; set; }
        public string CustomerName { get; set; }
        public string InvoiceNum { get; set; }
        public string BillGroupCode { get; set; }
        public string InvoiceType { get; set; }
        public string MstCustomer { get; set; }
        public string PoNum { get; set; }
        public string SoNum { get; set; }
        public string Class { get; set; }
        public string Currency { get; set; }
        public string States { get; set; }
        public string OrderBy { get; set; }
        public Nullable<decimal> OriginalAmt { get; set; }
        public Nullable<decimal> BalanceAmt { get; set; }
        public string CreditTrem { get; set; }
        public string TrackStates { get; set; }
        public Nullable<System.DateTime> InvoiceDate { get; set; }
        public Nullable<System.DateTime> DueDate { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public Nullable<System.DateTime> UpdateDate { get; set; }
        public string Remark { get; set; }
        public string Comments { get; set; }
        public Nullable<System.DateTime> PtpDate { get; set; }
        public string MissAccountFlg { get; set; }
        public Nullable<System.DateTime> StatementDate { get; set; }
        public string CustomerAddress1 { get; set; }
        public string CustomerAddress2 { get; set; }
        public string CustomerAddress3 { get; set; }
        public string CustomerAddress4 { get; set; }
        public string CustomerCountry { get; set; }
        public string CustomerCountryDetail { get; set; }
        public string AttentionTo { get; set; }
        public string CollectorName { get; set; }
        public string CollectorContact { get; set; }
        public Nullable<int> DaysLateSys { get; set; }
        public string RboCode { get; set; }
        public Nullable<decimal> OutstandingAccumulatedInvoiceAmt { get; set; }
        public string CustomerBillToSite { get; set; }
        public string SiteUseId { get; set; }
        public string SellingLocationCode { get; set; }
        public string Sales { get; set; }
        public string FuncCurrCode { get; set; }
        public Nullable<decimal> WoVat_AMT { get; set; }
        public string AgingBucket { get; set; }
        public string CreditTremDescription { get; set; }
        public string SellingLocationCode2 { get; set; }
        public string Ebname { get; set; }
        public string Customertype { get; set; }
        public string Cmpinv { get; set; }
        public Nullable<System.DateTime> CloseDate { get; set; }
        public Nullable<decimal> CreditLmt { get; set; }
        public Nullable<decimal> CreditLmtAcct { get; set; }
        public string CustomerService { get; set; }
        public string LsrNameHist { get; set; }
        public string FsrNameHist { get; set; }
        public string Eb { get; set; }
        public string MailId { get; set; }
        public string CallId { get; set; }
        public string Fsr { get; set; }
        public Nullable<System.DateTime> TRACK_DATE { get; set; }
        public Nullable<bool> NeedTask { get; set; }
        public string FinishedStatus { get; set; }
        public Nullable<int> CollectionStrategyId { get; set; }

        public string TrackStatesName { get; set; }
        public string vatNo { get; set; }
        public string vatDate { get; set; }
        public string pTPIdentified { get; set; }
        public string dispute { get; set; }
        public DateTime? disputeIdentifiedDate { get; set; }
        public string disputeReason { get; set; }
        public string actionOwnerDepartment { get; set; }
        public DateTime? nextActionDate { get; set; }
        public DateTime? Payment_Date { get; set; }
    }
}
