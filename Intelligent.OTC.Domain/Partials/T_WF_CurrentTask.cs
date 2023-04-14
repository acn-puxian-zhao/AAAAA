using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common;

namespace Intelligent.OTC.Domain.DataModel
{
    public partial class T_WF_CurrentTask : WorkflowAggregateRoot 
    {

    }

    public class WorkflowAggregateRoot : IAggregateRoot
    {
        public virtual int Id { get; set; }
    } 
}
