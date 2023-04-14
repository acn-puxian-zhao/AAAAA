using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Job
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution] //不允许此 Job 并发执行任务（禁止新开线程执行）
    public class AutoBackCollectorStatisticsJob : BaseJob
    {
        IStatisticsCollectService staticsCollectService = SpringFactory.GetObjectImpl<IStatisticsCollectService>("StatisticsCollectService");

        protected override void ExecuteInternal(IJobExecutionContext context)
        {
            try
            {
                staticsCollectService.BackCollectorStatisticsJob();
            }
            catch (Exception ex)
            {
                logger.Error("AutoBackCollectorStatisticsJob Error", ex);
            }
            finally
            {

            }
        }

    }
}
