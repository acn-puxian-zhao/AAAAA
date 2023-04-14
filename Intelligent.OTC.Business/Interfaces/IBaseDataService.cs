using System;
using System.Collections.Generic;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Common;
using Intelligent.OTC.Domain.Repositories;
namespace Intelligent.OTC.Business
{
    public interface IBaseDataService
    {
        ICacheService CacheSvr { set; }
        OTCRepository CommonRep { set; }
        List<SysTypeDetail> GetAllSysTypeDetail();
        List<SysTypeDetail> GetSysTypeDetail(string strTypecode);
        Dictionary<string, List<SysTypeDetail>> GetSysTypeDetails(string strTypeCodes);
        List<SysConfig> GetAllSysConfigs();
        SysConfig GetSysConfigByCode(string code);
        CurrentTracking AppendTrackingConfig(CurrentTracking tracking, string deal, string customerNum, string legalEntity);

        string CreateDailyReport();
        IEnumerable<CollectorReport> GetCollectorReport();
    }
}
