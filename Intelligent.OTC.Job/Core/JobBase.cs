using Common.Logging;
using Intelligent.OTC.Common;
using Intelligent.OTC.Domain.Repositories;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Job
{

    public abstract class BaseJob : IJob
    {
        protected ILog logger = LogManager.GetLogger("JobLogger");
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                //JobDataMap jobDataMap = context.MergedJobDataMap;  // Note the difference from the previous example
                //string loggerName = jobDataMap.GetString("LoggerName");
                //if (string.IsNullOrWhiteSpace(loggerName))
                //{
                //    logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
                //}
                //else
                //{
                //    logger = LogManager.GetLogger(loggerName);
                //}
                //< start - time > 2012 - 04 - 01T08: 00:00 + 08:00 
                //    表示北京时间2012年4月1日上午8:00开始执行，注意服务启动或重启时都会检测此属性，
                //    若没有设置此属性，服务会根据cron - expression的设置执行任务调度；
                //    若start - time设置的时间比当前时间较早，则服务启动后会忽略掉cron - expression设置，立即执行一次调度，
                //        之后再根据cron - expression执行任务调度；
                //    若设置的时间比当前时间晚，则服务会在到达设置时间相同后才会应用cron - expression，
                //        根据规则执行任务调度，一般若无特殊需要请不要设置此属性
                //</ start - time >

                  logger.Info(string.Format("Start executing job:{0}", ((JobDetailImpl)context.JobDetail).FullName));
                this.ExecuteInternal(context);
                #region 代码示例
                //JobKey key = context.JobDetail.Key;
                //JobDataMap dataMap = context.MergedJobDataMap;  
                // Note the difference from the previous example
                //IList<DateTimeOffset> state = (IList<DateTimeOffset>)dataMap["myStateData"];
                //state.Add(DateTimeOffset.UtcNow);
                //ITrigger myTrigger = TriggerBuilder
                //.Create()
                //.WithIdentity("trigger1", "myGroup")
                //.WithCronSchedule("0 0 12 1/5 * ? *", x => x.WithMisfireHandlingInstructionDoNothing())
                //.Build();
                #endregion
                logger.Info(string.Format("Execution is complete, Job:{0}", ((JobDetailImpl)context.JobDetail).FullName));
            }
            catch (JobExecutionException ex)
            {
                if (logger != null)
                {
                    logger.Error(string.Format("Execution error,Job:{0}", ((JobDetailImpl)context.JobDetail).FullName), ex);
                }
                throw ex;
            }
            catch (Exception ex)
            {
                if (logger != null)
                {
                    logger.Error(string.Format("Execution error,Job:{0}", ((JobDetailImpl)context.JobDetail).FullName), ex);
                }
                //something really unexpected happened, so give up
                throw new JobExecutionException(string.Format("Execution error,Job:{0}", ((JobDetailImpl)context.JobDetail).FullName), ex, true); // or set to true if you want to refire
            }
        }
        protected abstract void ExecuteInternal(IJobExecutionContext context);        
        
    }
}
