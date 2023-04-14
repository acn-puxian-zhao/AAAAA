using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class TaskPtpDto
    {
        public long Id { get; set; }
        public string LegalEntity { get; set; }
        public string CustomerNum { get; set; }
        public string CustomerName { get; set; }
        public string SiteUseId { get; set; }
        public Nullable<System.DateTime> PromiseDate { get; set; }
        public Nullable<bool> IsPartialPay { get; set; }
        public string Payer { get; set; }
        public Nullable<decimal> PromissAmount { get; set; }
        public string Comments { get; set; }
        public Nullable<System.DateTime> CreateTime { get; set; }
        public string PTPStatus { get; set; }
        public string PTPStatusName { get; set; }
        public Nullable<System.DateTime> Status_Date { get; set; }
    }
}
