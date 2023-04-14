using Intelligent.OTC.Common;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Repositories;
using System.Linq;

namespace Intelligent.OTC.Business
{
    public class CustomerGroupCfgService
    {
        public OTCRepository CommonRep { get; set; }

        public CustomerGroupCfgService()
        { 
        }

        public IQueryable<CustomerGroupCfg> GetAllGroups()
        {
            return CommonRep.GetQueryable<CustomerGroupCfg>().Where(o => o.Deal==AppContext.Current.User.Deal).AsQueryable();
        }
        public CustomerGroupCfg GetGroupByCode(string code) 
        {
            return CommonRep.GetQueryable<CustomerGroupCfg>().Where(o => o.BillGroupCode == code && o.Deal==AppContext.Current.User.Deal).FirstOrDefault();
        }
    }
}
