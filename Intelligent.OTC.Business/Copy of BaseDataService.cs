using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OTC.POC.Repository.Repositories;
using OTC.POC.Repository.DataModel;
using OTC.POC.Repository;
using OTC.POC.Common;
using System.Data.Entity.Validation;
using OTC.POC.Common.UnitOfWork;

namespace OTC.POC.Business
{
    public class BaseDataService : IBaseDataService
    {
        public CommonRepository CommonRep { get; set; }
        public string[] DataBaseInfo;
        public IUnitOfWork UOW { get; set; }
        public ICacheService CacheSvr { get; set; }

        /// <summary>
        /// Get all system type details from cache
        /// </summary>
        /// <returns></returns>
        public List<SysTypeDetail> GetAllSysTypeDetail()
        {
            return CacheSvr.GetOrSet<List<SysTypeDetail>>("Cache_SysTypeDetail", () =>
            {
                HashSet<string> processed = new HashSet<string>();
                List<SysTypeDetail> res = CommonRep.GetDbSet<SysTypeDetail>().OrderBy(td => td.Seq).ToList();
                return res;
            });
        }

        public List<SysTypeDetail> GetSysTypeDetail(string strTypecode)
        {
            var Result = GetAllSysTypeDetail().FindAll(d=>d.TypeCode == strTypecode);

            return Result;
        }

        public List<SysConfig> GetAllSysConfigs()
        {
            return CacheSvr.GetOrSet<List<SysConfig>>("Cache_SysConfig", () =>
            {
                return CommonRep.GetDbSet<SysConfig>().ToList();
            });
        }

        public SysConfig GetSysConfigByCode(string code)
        {
            return GetAllSysConfigs().Find(cfg => cfg.CfgCode == code);
        }
    }
}