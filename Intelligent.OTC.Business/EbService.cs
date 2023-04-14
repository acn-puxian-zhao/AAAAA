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
using Intelligent.OTC.Common.Repository;

namespace Intelligent.OTC.Business
{
    public class EbService
    {
        public OTCRepository CommonRep { get; set; }
        public string[] DataBaseInfo;
        public ICacheService CacheSvr { get; set; }
        public List<T_LeglalEB> GetAllEbs()
        {
            return (from t in CommonRep.GetDbSet<T_LeglalEB>()
                        select t).ToList().DistinctBy(q => q.EB).OrderBy(o=>o.EB).ToList();
        }

        public IQueryable<T_LeglalEB> GetEbs(string ebCode)
        {
            var Result = from ebs in GetAllEbs()
                         select ebs;

            return Result.AsQueryable();
        }

        public IQueryable<T_LeglalEB> GetEbs()
        {
            var Result = CommonRep.GetDbSet<T_LeglalEB>();
            return Result.AsQueryable();
        }
    }
}
