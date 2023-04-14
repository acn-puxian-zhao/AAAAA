using System.Collections.Generic;

namespace Intelligent.OTC.Domain.DomainModel
{
    public class DashBoardModel
    {
        //总金额
        public decimal TotalAMT { get; set; }
        //答应付款金额
        public decimal ConfirmTotal { get; set; }
        //逾期金额
        public decimal OverdueTotal { get; set; }
        //有争议金额
        public decimal DisputeTotal { get; set; }

        public int NoCollector { get; set; }
        public string NoUpload { get; set; }
        //逾期原因统计
        public List<AMTItem> OverdueReasonStatistics { get; set; }
        //逾期老化统计
        public List<AMTItem> OverdueAgingStatistics { get; set; }

        public int ShowYear { get; set; }
        public int ShowMonth { get; set; }
        public double Target { get; set; }
        public double Actual { get; set; }
        public double ConfirmDone { get; set; }
        public string TotalAMTFmt { get; set; }
        public string RecivedAMTFmt { get; set; }
        public double RemindingTotal { get; set; }
        public double RemindingDone { get; set; }
        public double DunningTotal { get; set; }
        public double DunningDone { get; set; }
        public double DisputeDone { get; set; }
        public double MailPending { get; set; }
        public double MailDone { get; set; }

        public double RecivedAMT { get; set; }
     
    }

    public class AMTItem
    {
        public string ItemName { get; set; }
        public decimal Amt { get; set; }
    }
}
