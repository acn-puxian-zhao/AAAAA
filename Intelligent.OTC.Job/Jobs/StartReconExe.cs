using Quartz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Job
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution] //不允许此 Job 并发执行任务（禁止新开线程执行）
    public class StartReconExe : BaseJob
    {
        protected override void ExecuteInternal(IJobExecutionContext context)
        {
            string strReconPath = @"D:\IIS\IOTC\Recon\Recon_Other\Other.exe";
            if (File.Exists(strReconPath))
            {
                logger.Error(string.Format("start recon"));
                Process p = Process.Start(strReconPath);
                p.WaitForExit();
                logger.Error(string.Format("start recon end"));
            }
            string strUnknowPath = @"D:\IIS\IOTC\Recon\Recon_Unknow\Unknow.exe";
            if (File.Exists(strUnknowPath))
            {
                logger.Error(string.Format("start unknow"));
                Process p = Process.Start(strUnknowPath);
                p.WaitForExit();
                logger.Error(string.Format("start unknow end"));
            }
        }
    }
}
