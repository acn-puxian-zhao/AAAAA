using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class AgingExpDateHisDto
    {
        public int ID { get; set; }
        public string UserId { get; set; }
        public DateTime?  ChangeDate { get; set; }
        public int InvID { get; set; }
        public DateTime? NewMemoExpirationDate { get; set; }
        public DateTime? OldMemoExpirationDate { get; set; }
        
    }
}
