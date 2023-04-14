using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common;

namespace Intelligent.OTC.Domain.DataModel
{
    public partial class SoaInvoice
    {
        public int InvoiceId { get; set; }
        public string CustomerNum { get; set; }

        public string CustomerName { get; set; }
        public string InvoiceNum { get; set; }
        public string LegalEntity { get; set; }
        public Nullable<System.DateTime> InvoiceDate { get; set; }
        public string CreditTerm { get; set; }
        public Nullable<System.DateTime> DueDate { get; set; }
        public string OverdueReason { get; set; }
        public string PurchaseOrder { get; set; }
        public string SaleOrder { get; set; }
        public string RBO { get; set; }
        public string InvoiceCurrency { get; set; }
        public string OriginalInvoiceAmount { get; set; }
        public Nullable<decimal> OutstandingInvoiceAmount { get; set; }
        public string DaysLate { get; set; }
        public string InvoiceTrack { get; set; }
        public string Status { get; set; }
        public  Nullable<System.DateTime>PtpDate { get; set; }
        public string DocumentType { get; set; }
        public string Comments { get; set; }
        public string SiteUseId { get; set; }
        public Nullable<decimal> StandardInvoiceAmount { get; set; }

        //Start add by xuan.wu for Arrow adding
        public string Sales { get; set; }
        public string States { get; set; }//Credit Limit
        public Nullable<decimal> BALANCE_AMT { get; set; }//Amt Remaining
        public Nullable<decimal> WoVat_AMT { get; set; }//Amount Wo Vat
        public string AgingBucket { get; set; }//Aging Bucket
        public string Ebname { get; set; }//Eb
        public string COLLECTOR_NAME { get; set; }//Collector
        public int? DueDays { get; set; }//due days
        public string InClass { get; set; }//invoice type
        public string Assessment { get; set; }//assessment type
        public string FinishStatus { get; set; }//Finish Status
        //End add by xuan.wu for Arrow adding

        // Start add by albert 
        public string VatNum { get; set; }
        public string VatDate { get; set; }
        public DateTime? TrackDate { get; set; }
        public DateTime? PtpIdentifiedDate { get; set; }
        public string DisputeReason { get; set; }
        public DateTime? DisputeDate { get; set; }
        public string IsDispute
        {
            get { return DisputeDate == null ? "N":"Y"; }
        }

        public string OwnerDepartment { get; set; }
        public DateTime? NextActionDate { get; set; }
        public string isBanlance { get; set; }
        //End add by albert
        public string NotClear { get; set; }
        public string ConsignmentNumber { get; set; }
        public string BalanceMemo { get; set; }
        public DateTime? MemoExpirationDate { get; set; }
        public int IsExp { get; set; }
    }
}
