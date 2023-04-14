using Intelligent.OTC.Domain.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class TaskDto
    {
        public string Deal { get; set; }
        public string LegalEntity { get; set; }
        public string CustomerNo { get; set; }
        public string CustomerName { get; set; }
        public string SiteUseId { get; set; }
        public string Collector { get; set; }
        public string Contactor { get; set; }
        public string CS { get; set; }
        public string Sales { get; set; }
        public string LastSoaDate { get; set; }
        public string LastSoaMailId { get; set; }
        public string Currency { get; set; }
        public Nullable<decimal> TotalAr { get; set; }
        public Nullable<decimal> NotOverdue { get; set; }
        public Nullable<decimal> Overdue { get; set; }
        public Nullable<decimal> Overdue60 { get; set; }
        public Nullable<decimal> Overdue120 { get; set; }
        public Nullable<decimal> Overdue270 { get; set; }
        public Nullable<decimal> Overdue360 { get; set; }
        public Nullable<int> Star { get; set; }
        public List<TaskDetailDto> taskDetail { get; set; }
        public int ResponseTimes { get; set; }
        public string EbName { get; set; }
        public string CreditTerm { get; set; }
        public DateTime? lastsenddate { get; set; }
        public int isExp { get; set; }
        public DateTime? CommentExpirationDate { get; set; }
        public DateTime? PtpDate { get; set; }
        public Nullable<decimal> PtpAmount { get; set; }
    }
}
