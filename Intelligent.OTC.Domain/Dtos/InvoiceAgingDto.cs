using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class InvoiceAgingDto
    {
        public string CustomerName { get; set; }
        public string CustomerNum { get; set; }
        public string SiteUseId { get; set; }
        public string SellingLocationCode { get; set; }
        public string Class { get; set; }
        public string InvoiceNum { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string CreditTrem { get; set; }
        public decimal? CreditLmt { get; set; }
        public decimal? CreditLmtAcct { get; set; }
        public string FuncCurrCode { get; set; }
        public string Currency { get; set; }
        public string Sales { get; set; }
        public int? DaysLateSys { get; set; }
        public decimal? BalanceAmt { get; set; }
        public decimal? WoVat_AMT { get; set; }
        public string AgingBucket { get; set; }
        public string CreditTremDescription { get; set; }
        public string SellingLocationCode2 { get; set; }
        public string Ebname { get; set; }
        public string Customertype { get; set; }
        public string LsrNameHist { get; set; }
        public string Fsr { get; set; }
        public string LegalEntity { get; set; }
        public string Cmpinv { get; set; }
        public string SoNum { get; set; }
        public string PoNum { get; set; }
        public string FsrNameHist { get; set; }
        public string Eb { get; set; }
        public decimal? RemainingAmtTran { get; set; }
        public string ConsignmentNumber { get; set; }
        public DateTime? MemoExpirationDate { get; set; }
    }
}
