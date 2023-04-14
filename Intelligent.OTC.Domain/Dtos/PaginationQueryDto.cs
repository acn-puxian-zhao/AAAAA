using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class PaginationQueryDto
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Skip { get { return (Page - 1) * PageSize; } }
    }
}
