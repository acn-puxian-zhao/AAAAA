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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Job
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution] //不允许此 Job 并发执行任务（禁止新开线程执行）
    public class AutoImportARJob : BaseJob
    {
        string strCodeAcc = string.Empty;
        string strCodeInv = string.Empty;
        string strCodeInvDetail = string.Empty;
        string strCodeVat = string.Empty;
        List<string> legalEntitys = new List<string>();

        FileUploadHistory accFileName = new FileUploadHistory();
        FileUploadHistory invFileName = new FileUploadHistory();
        FileUploadHistory invDetailFileName = new FileUploadHistory();
        List<FileUploadHistory> listAutoFile = new List<FileUploadHistory>();
        ICustomerService cusService = SpringFactory.GetObjectImpl<ICustomerService>("CustomerService");

        protected override void ExecuteInternal(IJobExecutionContext context)
        {
            strCodeAcc = Helper.EnumToCode<FileType>(FileType.Account);
            strCodeInv = Helper.EnumToCode<FileType>(FileType.Invoice);
            strCodeInvDetail = Helper.EnumToCode<FileType>(FileType.InvoiceDetail);
            try
            {
                var datetimeNow = AppContext.Current.User.Now.Hour;
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
                            accFileName = new FileUploadHistory();
                            invFileName = new FileUploadHistory();
                            if (Service.GetLegalEntityIsFinish(legalEntity))
                            {
                                continue;
                            }
                            else
                            {
                                //对每一个legal进行处理
                                listAutoFile = Service.GetAutoData(legalEntity);
                                accFileName = listAutoFile.FirstOrDefault(o => o.FileType == strCodeAcc);
                                invFileName = listAutoFile.FirstOrDefault(o => o.FileType == strCodeInv);
                                logger.Debug(string.Format("AutoImportARJob Account File:{0},Invoice File:{1}", accFileName, invFileName));
                                if (accFileName != null && invFileName != null)
                                {
                                    cusService.allFileImportArrow(accFileName, invFileName, null, null);
                                }
                                else
                                {
                                    logger.Error(string.Format("upload files less than two,legal entity:{0}", legalEntity));
                                    continue;
                                }
                            }
                        }
                        catch(Exception exLegal)
                        {
                            logger.Error(string.Format("you must upload two files under the same legal entity,legal entity:{0}", legalEntity), exLegal);
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




