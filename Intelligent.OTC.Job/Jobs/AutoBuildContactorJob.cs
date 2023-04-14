using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Repository;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Job
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution] //不允许此 Job 并发执行任务（禁止新开线程执行）
    public class AutoBuildContactorJob : BaseJob
    {
        List<string> legalEntitys = new List<string>();
        ICustomerService cusService = SpringFactory.GetObjectImpl<ICustomerService>("CustomerService");

        protected override void ExecuteInternal(IJobExecutionContext context)
        {
            try
            {
                var datetimeNow = AppContext.Current.User.Now;
                string batchDeal = ConfigurationManager.AppSettings["BatchDeal"];

                IJobService Service = SpringFactory.GetObjectImpl<IJobService>("JobService");

                legalEntitys = Service.GetSysTypeDetail("015").Select(o => o.DetailName).ToList();
                if (legalEntitys == null)
                {
                    throw new JobExecutionException("legal Entitys not found in system");
                }
                else
                {
                    foreach (var legalEntity in legalEntitys)
                    {
                        #region 上传legalEntity数据
                        try
                        {
                             cusService.autoBuildContactor(batchDeal, legalEntity);
                                
                        }
                        catch (Exception exLegal)
                        {
                            logger.Error(string.Format("auto build contactor faild,legal entity:{0}", legalEntity), exLegal);
                        }
                        #endregion 上传legalEntity数据
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("AutoImportARJob Error", ex);
                // or set to true if you want to refire
            }
            finally
            {

            }
        }
    }
}
