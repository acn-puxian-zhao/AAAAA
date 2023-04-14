using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Intelligent.OTC.Domain.DataModel
{
    public partial class DisputeTracking
    {
        public int InvoiceId { get; set; }
        public string CustomerNum { get; set; }
        public string CustomerName { get; set; }
        public string InvoiceNum { get; set; }
        public string LegalEntity { get; set; }
        public Nullable<System.DateTime> InvoiceDate { get; set; }
        public string CreditTerm { get; set; }
        public Nullable<System.DateTime> DueDate { get; set; }
        public string PurchaseOrder { get; set; }
        public string SaleOrder { get; set; }
        public string RBO { get; set; }
        public string InvoiceCurrency { get; set; }
        public string OriginalInvoiceAmount { get; set; }
        public Nullable<decimal> OutstandingInvoiceAmount { get; set; }
        public string DaysLate { get; set; }
        public Nullable<System.DateTime> PtpDate { get; set; }
        public string InvoiceTrack { get; set; }
        public string Status { get; set; }
        public string OrderBy { get; set; }
        public string DocumentType { get; set; }
        public string Comments { get; set; }
        public Nullable<decimal> WoVat_AMT { get; set; }
        public Nullable<System.DateTime> TRACK_DATE { get; set; }
        public string AgingBucket { get; set; }
        public string Eb { get; set; }
        public string TrackStates { get; set; }
        public string CollectorName { get; set; }
        public Nullable<int> DueDays { get; set; }


        // Start add by albert 
        public string VatNum { get; set; }
        public string VatDate { get; set; }
        public DateTime? TrackDate { get; set; }
        public DateTime? PtpIdentifiedDate { get; set; }
        //End add by albert
    }
}
