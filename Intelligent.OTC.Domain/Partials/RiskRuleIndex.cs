using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common;

namespace Intelligent.OTC.Domain.DataModel
{
    public partial class RiskRuleIndex : IAggregateRoot
    {
        //public string IndexRateMehtod { get; set; }
        //public string IndexName { get; set; }
        public int IndexWeight { get; set; }

        public decimal IndexValue { get; set; }

        public decimal IndexRate { get; set; }

        //public int Id
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }
        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}
    }
}
