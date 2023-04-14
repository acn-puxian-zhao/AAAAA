using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class PageResultDto<T>
    {
        public List<T> dataRows { get; set; }
        public int count { get; set; }
    }
}
