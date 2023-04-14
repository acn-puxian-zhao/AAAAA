using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class CashApplicationCountReportDto
    {
        public List<CashApplicationCountReportTotalDto> total;
        public List<CashApplicationCountReportDetailDto> pmtList;
        public List<CashApplicationCountReportDetailDto> arList;
        public List<CashApplicationCountReportDetailDto> ptpList;
        public List<CashApplicationCountReportDetailDto> manualList;
    }

    public class CashApplicationCountReportTotalDto
    {
        public string LegalEntity { get; set; }
        public int TotalBS { get; set; }
        public int ClosedBS { get; set; }
        public int IsReconed { get; set; }
        public int PmtDetail { get; set; }
        public string PmtDetailPersent { get; set; }
        public int AR { get; set; }
        public string ARPersent { get; set; }
        public int PTP { get; set; }
        public int Manual { get; set; }
    }

    public class CashApplicationCountReportDetailDto
    {
        public string GroupType { get; set; }
        public string LegalEntity { get; set; }
        public string BSTYPE { get; set; }
        public string TRANSACTION_Number { get; set; }
        public decimal TRANSACTION_AMOUNT { get; set; }
        public DateTime VALUE_DATE { get; set; }
        public string CURRENCY { get; set; }
        public string FORWARD_NUM { get; set; }
        public string FORWARD_NAME { get; set; }
        public string CUSTOMER_NUM { get; set; }
        public string CUSTOMER_NAME { get; set; }
        public string PMTNUMBER { get; set; }
        public string GroupNo { get; set; }
    }

}
