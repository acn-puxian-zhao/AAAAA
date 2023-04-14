using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class ExportCadaliyReportDto
    {
        public string LegalEntity { get; set; }
        public string BSTYPE { get; set; }
        public string TRANSACTION_NUMBER { get; set; }
        public decimal? TRANSACTION_AMOUNT { get; set; }
        public DateTime? VALUE_DATE { get; set; }
        public string CURRENCY { get; set; }
        public decimal? CURRENT_AMOUNT { get; set; }
        public decimal? UNCLEAR_AMOUNT { get; set; }
        public DateTime? CREATE_DATE { get; set; }
        public string APPLY_STATUS { get; set; }
        public DateTime? APPLY_TIME { get; set; }
        public string CLEARING_STATUS { get; set; }
        public DateTime? CLEARING_TIME { get; set; }
        public string PostMailStatus { get; set; }
        public DateTime? PostMailSendTime { get; set; }
        public string PostMailSubject { get; set; }
        public string PostMailTo { get; set; }
        public string PostMailCc { get; set; }
        public string ClearMailStatus { get; set; }
        public DateTime? ClearSendTime { get; set; }
        public string ClearMailSubject { get; set; }
        public string ClearMailTo { get; set; }
        public string ClearMailCc { get; set; }
        public int count { get; set; }
    }
}
