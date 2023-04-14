using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    using System;
    using System.Collections.Generic;

    public partial class T_Customer_Comments
    {
        public System.Guid ID { get; set; }
        public string CUSTOMER_NUM { get; set; }
        public string SiteUseId { get; set; }
        public string AgingBucket { get; set; }
        public Nullable<System.DateTime> PTPDATE { get; set; }
        public Nullable<decimal> PTPAmount { get; set; }
        public string OverdueReason { get; set; }
        public string Comments { get; set; }
        public string CommentsFrom { get; set; }
        public string CreateUser { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public Nullable<bool> isDeleted { get; set; }
        public Nullable<int> SortId { get; set; }
    }
}
