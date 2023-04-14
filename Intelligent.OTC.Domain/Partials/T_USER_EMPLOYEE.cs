using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common;

namespace Intelligent.OTC.Domain.DataModel
{
    public partial class T_USER_EMPLOYEE : XcceleratorAggregateRoot
    {
    }

    public class XcceleratorAggregateRoot : IAggregateRoot
    {
        public virtual int Id { get; set; }
    } 

}
