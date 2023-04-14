using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Domain.Dtos
{
    public class TaskGridDto
    {
        public List<TaskDto> taskRow { get; set; }
        public int count { get; set; }
    }
}