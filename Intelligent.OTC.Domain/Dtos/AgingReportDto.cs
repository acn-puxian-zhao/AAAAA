using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class AgingReportDto
    {
        public string Ebname { get; set; }
        public string Customertype { get; set; }
        public string AccntNumber { get; set; }
        public string SiteUseId { get; set; }
        public string CustomerName { get; set; }
        public string SellingLocationCode { get; set; }
        public string Class { get; set; }
        public string TrxNum { get; set; }
        public DateTime TrxDate { get; set; }
        public DateTime DueDate { get; set; }
        public int DueDays { get; set; }
        public decimal? AmtRemaining { get; set; }
        public decimal? AmountWoVat { get; set; }
        public string PaymentTermName { get; set; }
        public decimal? OverCreditLmt { get; set; }
        public decimal? OverCreditLmtAcct { get; set; }
        public string FuncCurrCode { get; set; }
        public string InvCurrCode { get; set; }
        public string SalesName { get; set; }
        public string AgingBucket { get; set; }
        public string PaymentTermDesc { get; set; }
        public string SellingLocationCode2 { get; set; }
        public string Isr { get; set; }
        public string Fsr { get; set; }
        public string OrgId { get; set; }
        public string Cmpinv { get; set; }
        public string SalesOrder { get; set; }
        public string Cpo { get; set; }
        public string FsrNameHist { get; set; }
        public string IsrNameHist { get; set; }
        public string Eb { get; set; }
        public decimal? AmtRemainingTran { get; set; }

    }

    public class AgingReportSumDto {
        public string Ebname { get; set; }
        public string AccntNumber { get; set; }
        public string SiteUseId { get; set; }
        public string CustomerName { get; set; }
        public string PaymentTermDesc { get; set; }
        public decimal? OverCreditLmt { get; set; }
        public string FuncCurrCode { get; set; }
        public string Fsr { get; set; }
        public decimal? AmtRemaining01To15 { get; set; }
        public decimal? AmtRemaining16To30 { get; set; }
        public decimal? AmtRemaining31To45 { get; set; }
        public decimal? AmtRemaining46To60 { get; set; }
        public decimal? AmtRemaining61To90 { get; set; }
        public decimal? AmtRemaining91To120 { get; set; }
        public decimal? AmtRemaining121To180 { get; set; }
        public decimal? AmtRemaining181To270 { get; set; }
        public decimal? AmtRemaining271To360 { get; set; }
        public decimal? AmtRemaining360Plus { get; set; }
        public decimal? AmtRemainingTotalFutureDue { get; set; }
    }

    public class AgingReportDtoPage
    {
        public List<AgingReportDto> detail;
        public List<AgingReportSumDto> summary;

        public int detailcount;
        public int summarycount;
    }

}
