using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Domain.DataModel;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;

namespace Intelligent.OTC.Job
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution] //不允许此 Job 并发执行任务（禁止新开线程执行）
    public class AutoImportInvDetailJob  //: BaseJob   remove job，remove implement
    {
        List<FileUploadHistory> listAutoFile = new List<FileUploadHistory>();

        ICustomerService cusService = SpringFactory.GetObjectImpl<ICustomerService>("CustomerService");

        // remove job,  remove override
        protected void ExecuteInternal(IJobExecutionContext context)
        {
            try
            {
                var datetimeNow = AppContext.Current.User.Now.Hour;
                IJobService Service = SpringFactory.GetObjectImpl<IJobService>("JobService");
                listAutoFile = Service.GetAutoDataInvoiceDetail();
                if (listAutoFile == null || listAutoFile.Count <= 0)
                {
                    //今天没有上传的这个文件
                    return;
                }
                foreach (var detail in listAutoFile)
                {
                    cusService.ImportInvoiceDetailOnly(detail);
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {

            }
        }
    }
}
