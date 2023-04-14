using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;

namespace Intelligent.OTC.Job
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution] //不允许此 Job 并发执行任务（禁止新开线程执行）
    public class AutoExportFeedbackReportJob : BaseJob
    {
        List<FileUploadHistory> listAutoFile = new List<FileUploadHistory>();

        protected override void ExecuteInternal(IJobExecutionContext context)
        {
            try
            {
                ReportFeedbackService service = SpringFactory.GetObjectImpl<ReportFeedbackService>("ReportFeedbackService");
                service.ExportDetailJob();
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
