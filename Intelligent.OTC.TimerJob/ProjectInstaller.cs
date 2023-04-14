using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace Intelligent.OTC.MailWinService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();

            this.serviceInstaller1.ServiceName = "Intelligent.OTC.TimerJob";
            this.serviceInstaller1.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
        }
    }
}
