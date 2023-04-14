using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class TaskPmtDto
    {
        public string status { get; set; }
        public long Id { get; set; }
        public string Collector { get; set; }
        public string LegalEntity { get; set; }
        public string CustomerNum { get; set; }
        public string CustomerName { get; set; }
        public string SiteUseId { get; set; }
        public string Currency { get; set; }
        public string Class { get; set; }
        public string InvoiceNum { get; set; }
        public DateTime InvoiceDate { get; set; }
        public Nullable<decimal> BalanceAmt { get; set; }
        public string TrackStatus { get; set; }
        public string TrackStatusName { get; set; }
        public DateTime TrackDate { get; set; }
        public string Comments { get; set; }
        public string LsrNameHist { get; set; }
        public string FsrNameHist { get; set; }
        public string haspmt { get; set; }
    }
}
