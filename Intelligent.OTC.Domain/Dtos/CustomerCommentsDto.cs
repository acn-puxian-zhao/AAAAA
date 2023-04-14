using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class CustomerCommentsDto
    {
        DateTime? _PTPDATE = null;
        public Guid ID { get; set; }
        public string CUSTOMER_NUM { get; set; }
        public string SiteUseId { get; set; }
        public string AgingBucket { get; set; }
        public decimal? PTPAmount { get; set; }
        public DateTime? PTPDATE { get { return _PTPDATE; } set { _PTPDATE = value == new DateTime(1900,1,1) ? null : value; } }
        public string OverdueReason { get; set; }
        public string Comments { get; set; }
        public string CommentsFrom { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
        public bool? isDeleted { get; set; }
    }
}
