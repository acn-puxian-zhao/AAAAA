using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Business
{
    public interface IStatisticsCollectService
    {
        IQueryable<StatisticsCollectDto> GetStatisticsCollect(string region);
        string createCustomerstatisticReport(string region);
        string createCollectorstatisticReport();
        IQueryable<V_STATISTICS_COLLECTOR> GetStatisticsCollector();
        StatisticsCollectSumDto GetStatisticsCollectSum();
        void BackCollectorStatisticsJob();
    }
}
