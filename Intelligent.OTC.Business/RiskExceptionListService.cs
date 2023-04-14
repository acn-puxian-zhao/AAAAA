using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Domain.Repositories;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain;
using Intelligent.OTC.Common;
using System.Data.Entity.Validation;
using Intelligent.OTC.Common.UnitOfWork;

namespace Intelligent.OTC.Business
{
    public class RiskExceptionListService
    {
        public OTCRepository CommonRep { get; set; }

        public List<CustomerPrioritizationExceptionList> getAllRiskExceptionByValue(string strType)
        {
            DateTime dt = AppContext.Current.User.Now;
            string strDeal = AppContext.Current.User.Deal.ToString();
            return CommonRep.GetDbSet<CustomerPrioritizationExceptionList>().Where(o => o.Deal == strDeal
                                                        && o.ExpiryDate >= dt
                                                        && o.EffectDate <= dt
                                                        && o.ExType == strType
                                                        ).Select(o=>o).ToList();
        }

    }
}
