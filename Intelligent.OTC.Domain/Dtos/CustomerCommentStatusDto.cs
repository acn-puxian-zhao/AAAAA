using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class CustomerCommentStatusDto
    {
        public string SiteUseId { get; set; }
        public string AgingBucket { get; set; }
        public decimal? PTPAmount { get; set; }
        public decimal? PTPAmountOld { get; set; }
        public DateTime? PTPDate { get; set; }
        public DateTime? PTPDateOld { get; set; }
        public string ODReason { get; set; }
        public string ODReasonOld { get; set; }
        public string Comments { get; set; }
        public string CommentsFrom { get; set; }
        public string CommentsOld { get; set; }
        public string CommentsFromOld { get; set; }
    }
}
