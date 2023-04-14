using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Job
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution] //不允许此 Job 并发执行任务（禁止新开线程执行）
    public class AutoImportCusJob : BaseJob
    {              
        List<FileUploadHistory> listAutoFile = new List<FileUploadHistory>();

        ICustomerService cusService = SpringFactory.GetObjectImpl<ICustomerService>("CustomerService");

        protected override void ExecuteInternal(IJobExecutionContext context)
        {
            try
            {
                var datetimeNow = AppContext.Current.User.Now.Hour;
                IJobService Service = SpringFactory.GetObjectImpl<IJobService>("JobService");
              
                listAutoFile = Service.GetAutoDataCus();

                if (listAutoFile != null)
                {
                    foreach (var cus in listAutoFile)
                    {
                        cusService.ImportCustomerLocalize(cus);
                    }
                }
                else
                {
                    //今天没有上传的这个文件
                    return;
                }

            }
            catch (Exception ex)
            {
                logger.Error("AutoImportCusJob Error", ex);
            }
            finally
            {

            }
        }
    }
}
