using System;

namespace Intelligent.OTC.Domain.DomainModel
{
    public class ReportItem
    {
        public int ID { get; set; }
        public string CustomerName { get; set; }
        public string AccntNumber { get; set; }
        public string SiteUseId { get; set; }
        public string SellingLocationCode { get; set; }
        public string Class { get; set; }
        public string TrxNum { get; set; }
        public DateTime? TrxDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string OverdueReason { get; set; }
        public string PaymentTermName { get; set; }
        public string OverCreditLmt { get; set; }
        public string OverCreditLmtAcct { get; set; }
        public string FuncCurrCode { get; set; }
        public string InvCurrCode { get; set; }
        public string SalesName { get; set; }
        public string DueDays { get; set; }
        public string AmtRemaining { get; set; }
        public string AmountWoVat { get; set; }
        public string AgingBucket { get; set; }
        public string PaymentTermDesc { get; set; }
        public string SellingLocationCode2 { get; set; }
        public string Ebname { get; set; }
        public string Customertype { get; set; }
        public string Isr { get; set; }
        public string Fsr { get; set; }
        public string OrgId { get; set; }
        public string Cmpinv { get; set; }
        public string SalesOrder { get; set; }
        public string Cpo { get; set; }
        public string FsrNameHist { get; set; }
        public string IsrNameHist { get; set; }
        public string Eb { get; set; }
        public string LocalName { get; set; }
        public string VATNo { get; set; }
        public DateTime? VATDate { get; set; }
        public string Collector { get; set; }
        public string CurrentStatus { get; set; }
        public DateTime? Lastupdatedate { get; set; }
        public string ClearingDocument { get; set; }
        public DateTime? ClearingDate { get; set; }
        public DateTime? PTPIdentifiedDate { get; set; }
        public DateTime? PTPDate { get; set; }
        public string PTPBroken { get; set; }
        public string PTPComment { get; set; }
        public string PTPDatehis { get; set; }
        public string Dispute { get; set; }
        public DateTime? DisputeIdentifiedDate { get; set; }
        public string DisputeStatus { get; set; }
        public string DisputeReason { get; set; }
        public string DisputeComment { get; set; }
        public string ActionOwnerDepartment { get; set; }
        public string ActionOwnerName { get; set; }
        public DateTime? NextActionDate { get; set; }
        public string CommentsHelpNeeded { get; set; }
        public DateTime? Payment_Date { get; set; } 
        public string PONum { get; set; }
        public string SONum { get; set; }
        public string InvMemo { get; set; }
        public string IsPartial { get; set; }
        public string PtpAmount { get; set; }
        public string PartialAmount { get; set; }
        public string PaymentID { get; set; }
        public string IsForwarder { get; set; }
        public string Forwarder { get; set; }
    }
}
