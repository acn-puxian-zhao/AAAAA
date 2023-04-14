using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Web.Mvc;
using System.Web.Routing;
using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.Partials;
using log4net;
using log4net.Config;

namespace Intelligent.OTC.MailWinService
{
    public partial class TimerJob : ServiceBase
    {
        public TimerJob()
        {
            InitializeComponent();
        }

        static ILog log = LogManager.GetLogger(typeof(TimerJob));

        protected override void OnStart(string[] args)
        {
            try
            {
                new SpringFactory();
                List<MailTimerWrapper> ts = SpringFactory.GetObjectImpl<List<MailTimerWrapper>>("MailTimer");
                foreach (var t in ts)
                {
                    t.Elapsed += new ElapsedEventHandler(timer_Elapsed);
                    if (t.Enabled)
                    {
                        t.Start();
                    }
                }
                log.Info("TimerJob started.");
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                throw;
            }
        }


        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Helper.Log.Info("Start to process mail list.");
            (sender as MailTimerWrapper).Stop();
            try
            {
                SpringFactory.GetObjectImpl<MailService>("MailService").ProcessDealMailBoxs((sender as MailTimerWrapper).Deal);
                (sender as MailTimerWrapper).Start();
            }
            catch (Exception ex)
            {
                log.Error("ProcessDealMailBoxs failed.", ex);
            }
        }

        protected override void OnStop()
        {

        }
    }
}
