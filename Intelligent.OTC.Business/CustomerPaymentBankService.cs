using Intelligent.OTC.Common;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Repositories;
using System.Collections.Generic;
using System.Linq;


namespace Intelligent.OTC.Business
{
    public class CustomerPaymentBankService
    {
        public CustomerPaymentBankService()
        { 
        }

        public OTCRepository CommonRep { get; set; }

        public IList<CustomerPaymentBank> CustomerPaymentBankGet()
        {
            return CommonRep.GetQueryable<CustomerPaymentBank>().ToList();
        }

        public IList<CustomerPaymentBank> GetCustPaymentBank(string strCustNum)
        {
            return CommonRep.GetQueryable<CustomerPaymentBank>().Where(o => o.CustomerNum == strCustNum && o.Deal == AppContext.Current.User.Deal).ToList();
        }


    }
}
