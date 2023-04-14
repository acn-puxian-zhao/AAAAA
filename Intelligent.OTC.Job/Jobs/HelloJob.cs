using Common.Logging;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Utils;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Job
{

    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution] //不允许此 Job 并发执行任务（禁止新开线程执行）
    public class HelloJob : BaseJob
    {
        protected override void ExecuteInternal(IJobExecutionContext context)
        {
            try
            {
                FakeProvider jobProvider = SpringFactory.GetObjectImpl<FakeProvider>("FakeProvider");
                var sendSoaMailJobKey = new JobKey("SendSoaMailJob", "QueueJobs");
                IJobDetail sendSoaMailJob;
                if (jobProvider.Scheduler.CheckExists(sendSoaMailJobKey))
                {
                    sendSoaMailJob = jobProvider.Scheduler.GetJobDetail(sendSoaMailJobKey);
                }
                else
                {
                    sendSoaMailJob = JobBuilder.Create<SendSoaMailJob>()
                        .WithIdentity(sendSoaMailJobKey)
                        .StoreDurably()
                        .Build();
                }
                //截止执行时间,需要设置截止时间,防止到人员作业开始时还没发送完毕
                DateTimeOffset endTime = DateTime.SpecifyKind(DateTime.Now.Date.AddHours(23), DateTimeKind.Utc);
                var sendSoaMailTriggerKey = new TriggerKey(Guid.NewGuid().ToString());
                var sendSoaMailTrigger = TriggerBuilder.Create()
                    .UsingJobData("Arg","Value")
                    .WithIdentity(sendSoaMailTriggerKey)
                    .StartAt(DateBuilder.FutureDate(10, IntervalUnit.Second))
                    .EndAt(endTime)
                    .ForJob(sendSoaMailJobKey)
                    .Build();
                jobProvider.Scheduler.ScheduleJob(sendSoaMailTrigger);
            }
            catch (Exception ex)
            {
                throw new JobExecutionException(string.Format("Execution error,Job:{0}", ((JobDetailImpl)context.JobDetail).FullName), ex, true);
            }
        }
    }
}
