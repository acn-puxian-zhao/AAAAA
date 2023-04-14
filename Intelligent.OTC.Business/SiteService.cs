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
    public class SiteService
    {
        public OTCRepository CommonRep { get; set; }
        public string[] DataBaseInfo;
        public ICacheService CacheSvr { get; set; }
        public List<Sites> GetAllSites()
        {
            string userDeal = AppContext.Current.User.Deal;
            return CommonRep.GetDbSet<Sites>().Where(o => o.Deal == AppContext.Current.User.Deal).ToList();
        }

        public IQueryable<Sites> GetSites(string siteCode)
        {
            var Result = from sites in GetAllSites()
                         where sites.LegalEntity==siteCode
                         select sites;

            return Result.AsQueryable();
        }

        public IQueryable<Sites> GetSites()
        {
            var Result = CommonRep.GetDbSet<Sites>().Where(o => o.Deal == AppContext.Current.User.Deal);
            return Result.AsQueryable();
        }
    }
}
