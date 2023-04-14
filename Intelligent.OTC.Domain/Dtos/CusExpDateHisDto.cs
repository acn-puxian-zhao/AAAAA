using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class CusExpDateHisDto
    {
        public int ID { get; set; }
        public string UserId { get; set; }
        public DateTime?  ChangeDate { get; set; }
        public string CustomerNum { get; set; }
        public string SiteUseId { get; set; }
        public DateTime? OldCommentExpirationDate { get; set; }
        public DateTime? NewCommentExpirationDate{ get; set; }
        
    }
}
