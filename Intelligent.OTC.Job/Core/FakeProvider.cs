using Common.Logging;
using CrystalQuartz.Core.SchedulerProviders;
using Intelligent.OTC.Job;
using Quartz;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Intelligent.OTC.Job
{
    public class FakeProvider : StdSchedulerProvider
    {
        private ILog logger = LogManager.GetLogger<BaseJob>();
        protected override NameValueCollection GetSchedulerProperties()
        {
            var properties = base.GetSchedulerProperties();
            NameValueCollection props = (NameValueCollection)ConfigurationManager.GetSection("quartz");
            properties.Add(props);
            return properties;
        }

        protected override void InitScheduler(IScheduler scheduler)
        {
            #region 示例
            //construct job info
            try
            {
               
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
       
            #endregion
        }
    }
}