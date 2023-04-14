using Common.Logging;
using Intelligent.OTC.Common;
using Intelligent.OTC.Job;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Intelligent.OTC.WebApi
{
    public class QuartzConfig
    {
        public static void RegisterQuartz()
        {
            ILog logger = LogManager.GetLogger("JobLogger");
            try
            {
                if (string.Equals(ConfigurationManager.AppSettings["StartQuartz"], "true", StringComparison.CurrentCultureIgnoreCase))
                {
                    FakeProvider jobProvider = SpringFactory.GetObjectImpl<FakeProvider>("FakeProvider");
                    jobProvider.Scheduler.Start();
                }
            }
            catch(Exception ex)
            {
                logger.Error("Register Quartz Error", ex);
            }
        }
    }
}