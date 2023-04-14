using Common.Logging;
using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.Domain.Repositories;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Job
{

    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution] //不允许此 Job 并发执行任务（禁止新开线程执行）
    public class SendSoaMailJob : BaseJob
    {

        public OTCRepository CommonRep { get; set; }
        public XcceleratorRepository XcceleratorRep { get; set; }
        IMailService mailService = SpringFactory.GetObjectImpl<IMailService>("MailService");
        ISoaService soaService = SpringFactory.GetObjectImpl<ISoaService>("SoaService");

        bool reFire = true;

        protected void init()
        {
            CommonRep = SpringFactory.GetObjectImpl<OTCRepository>("CommonRep");
            XcceleratorRep = SpringFactory.GetObjectImpl<XcceleratorRepository>("XcceleratorRep");
        }

        protected override void ExecuteInternal(IJobExecutionContext context)
        {
            try
            {
                JobDataMap jobDataMap = context.MergedJobDataMap;  // Note the difference from the previous example
                string indexStr = jobDataMap.GetString("Index");
                logger.Info("Index:" + indexStr);
                if (string.IsNullOrEmpty(indexStr)) { indexStr = "0"; }
                int index = int.Parse(indexStr);
                logger.Info(string.Format("In：{0}", index));

                string strCollector = jobDataMap.GetString("Collector");
                string strDeal = jobDataMap.GetString("Deal");
                string strLegalEntity = jobDataMap.GetString("LegalEntity");
                //string strGroupName = jobDataMap.GetString("GroupName");
                string strCustomerNum = jobDataMap.GetString("CustomerNum");
                string strSiteUseId = jobDataMap.GetString("SiteUseId");
                string strAlertType = jobDataMap.GetString("AlertType");
                string strPeriodId = jobDataMap.GetString("PeriodId");
                string strToTitle = jobDataMap.GetString("ToTitle");
                string strToName = jobDataMap.GetString("ToName");
                string strCCTitle = jobDataMap.GetString("CCTitle");
                string strRegion = jobDataMap.GetString("Region");
                string strResponseDate = jobDataMap.GetString("ResponseDate");
                string strTempleteLanguage = jobDataMap.GetString("TempleteLanguage");
                string strIndexFile = jobDataMap.GetString("IndexFile");

                if (!string.IsNullOrEmpty(strCollector))
                    logger.Info(string.Format("Excute SendSoaMailJob Collector is:{0}", strCollector));
                if (!string.IsNullOrEmpty(strDeal))
                    logger.Info(string.Format("Excute SendSoaMailJob Deal is:{0}", strDeal));
                //if (!string.IsNullOrEmpty(strGroupName))
                    ////logger.Info(string.Format("Excute SendSoaMailJob GroupName is:{0}", strGroupName));
                if (!string.IsNullOrEmpty(strCustomerNum))
                    logger.Info(string.Format("Excute SendSoaMailJob CustomerNum is:{0}", strCustomerNum));
                if (!string.IsNullOrEmpty(strSiteUseId))
                    logger.Info(string.Format("Excute SendSoaMailJob SiteUseId is:{0}", strSiteUseId));
                if (!string.IsNullOrEmpty(strAlertType))
                    logger.Info(string.Format("Excute SendSoaMailJob Wave is:{0}", strAlertType));
                if (!string.IsNullOrEmpty(strPeriodId))
                    logger.Info(string.Format("Excute SendSoaMailJob PeriodId is:{0}", strPeriodId));
                if (!string.IsNullOrEmpty(strToTitle))
                    logger.Info(string.Format("Excute SendSoaMailJob ToTitle is:{0}", strToTitle));
                if (!string.IsNullOrEmpty(strToName))
                    logger.Info(string.Format("Excute SendSoaMailJob ToName is:{0}", strToName));
                if (!string.IsNullOrEmpty(strCCTitle))
                    logger.Info(string.Format("Excute SendSoaMailJob CCTitle is:{0}", strCCTitle));
                if (!string.IsNullOrEmpty(strResponseDate))
                    logger.Info(string.Format("Excute SendSoaMailJob ResponseDate is:{0}", strResponseDate));
                if (!string.IsNullOrEmpty(strRegion))
                    logger.Info(string.Format("Excute SendSoaMailJob Region is:{0}", strRegion));
                if (!string.IsNullOrEmpty(strTempleteLanguage))
                    logger.Info(string.Format("Excute SendSoaMailJob strTempleteLanguage is:{0}", strTempleteLanguage));

                AlertKey alertKey = new AlertKey() {
                    Collector = strCollector,
                    Deal = strDeal,
                    LegalEntity = strLegalEntity,
                    //GroupName = strGroupName,
                    CustomerNum = strCustomerNum,
                    SiteUseId = strSiteUseId,
                    AlertType = string.IsNullOrEmpty(strAlertType) ? 1 : Convert.ToInt32(strAlertType) ,
                    PeriodId = string.IsNullOrEmpty(strPeriodId) ? 0 : Convert.ToInt32(strPeriodId),
                    ToTitle = strToTitle,
                    ToName = strToName,
                    CCTitle = strCCTitle,
                    Region = strRegion,
                    ResponseDate = Convert.ToDateTime(strResponseDate),
                    TempleteLanguage = strTempleteLanguage,
                    IndexFile = strIndexFile
                };
                                
                init();

                sendMailTask(strDeal, alertKey, index);

            }
            catch (Exception ex)
            {
                logger.Error(string.Format("SendSoaError:{0},{1}", 0, true), ex);
                // 设置错误重试次数
                if (context.RefireCount > 10)
                {
                    reFire = false;
                }
                throw new JobExecutionException(string.Format("Execution error,Job:{0}", ((JobDetailImpl)context.JobDetail).FullName), ex, reFire);
            }
        }


        /// <summary>
        /// 发送邮件任务
        /// </summary>
        /// <param name="customerKey"></param>
        private void sendMailTask(string deal,AlertKey alertKey, int index)
        {
            try
            {
                
                //logger.Info("-------------------------sendMailTask(1): Get InvoiceList To Send --------------------------");
                //1、获取待发送 Invoice 列表
                List<int> invoiceIdList = GetSendInvIdListNew(alertKey.Collector, deal, alertKey.LegalEntity, alertKey.CustomerNum, alertKey.SiteUseId, alertKey.AlertType.ToString() , alertKey.ToTitle, alertKey.ToName, alertKey.TempleteLanguage);
                if (invoiceIdList == null || invoiceIdList.Count == 0)
                {
                    logger.Info(string.Format("There is no invoice could sent in, Collector:{0}, Region:{5}, AlertType:{3}, ToName:{4}, ToTitle:{1}, CCTtitle:{2}", alertKey.Collector, alertKey.ToTitle, alertKey.CCTitle, alertKey.AlertType.ToString(), alertKey.ToName, alertKey.Region));
                    return;
                }
                Helper.Log.Info("**************************** invCount:" + invoiceIdList.Count());
                //2、根据客户，及发票信息，调用 Service ，生成邮件及附件,保存邮件
                MailTmp nMail = generateMailByAlert(alertKey, invoiceIdList);
                //3、发送邮件
                if (nMail != null && !string.IsNullOrEmpty(nMail.To))
                {
                    sendMail(nMail, alertKey, invoiceIdList);
                }
                else {
                    Helper.Log.Warn("---------------Mail Create Faild-----------Collector:" + alertKey.Collector + ",Region:" +alertKey.Region + ",AlertType:" + alertKey.AlertType + ",ToTitle:" + alertKey.ToTitle + ",ToName:" + alertKey.ToName + ",CCTitle:" + alertKey.CCTitle);
                }
                
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Fail to send mail, Collector:{0}, SiteUseId:{4}, ToTitle:{1}, CCTtitle:{2}, Wave:{3}", alertKey.Collector, alertKey.ToTitle, alertKey.CCTitle, alertKey.AlertType.ToString(), alertKey.SiteUseId), ex);
            }

        }

        public MailTmp RenderInstance(MailTmp instance, string language, AlertKey alertKey, List<int> invoiceIdList)
        {
            int factAlertType = alertKey.AlertType == 0 ? 1 : alertKey.AlertType;
            //invoiceIds
            instance.invoiceIds = invoiceIdList.ToArray();
            //soaFlg
            instance.soaFlg = "1";
            instance.MailType = "00" + factAlertType.ToString() + "," + language;

            return instance;
        }

        /// <summary>
        /// 根据客户生成邮件
        /// </summary>
        /// <param name="customerKey"></param>
        /// <returns></returns>
        private MailTmp generateMailByAlert(AlertKey alertKey, List<int> invoiceIdList)
        {
            //每个ToTitle对应的客户只能是一种语言模板
            //根据ToTitle和ToName取客户Language唯一值
            var nDefaultLanguage = alertKey.TempleteLanguage;

            if (string.IsNullOrEmpty(nDefaultLanguage)) {
                Helper.Log.Info("Customer Contact Language haven't set.");
            }

            // 获取联系人
            ContactService service = SpringFactory.GetObjectImpl<ContactService>("ContactService");
            List<ContactorDto> collectorList = service.GetContactsByAlert(alertKey.Collector, alertKey.CustomerNum, alertKey.SiteUseId, alertKey.ToTitle, alertKey.ToName, alertKey.CCTitle, invoiceIdList).ToList();
            List<string> nTo = new List<string>();
            List<string> nCC = new List<string>();
            if (collectorList != null && collectorList.Count > 0)
            {
                foreach (var x in collectorList)
                {
                    if (string.IsNullOrEmpty(x.EmailAddress))
                        continue;

                    if (x.ToCc == "1")
                    {
                        if (!nTo.Contains(x.EmailAddress))
                        {
                            nTo.Add(x.EmailAddress);
                        }
                    }
                    else
                    {
                        if (!nCC.Contains(x.EmailAddress))
                        {
                            nCC.Add(x.EmailAddress);
                        }
                    }
                }

            }
            if (nTo.Count == 0) {
                return null;
            }

            //int factAlertType = alertKey.AlertType == 0 ? 1 : alertKey.AlertType;
            MailTmp nMail = soaService.GetNewMailInstance(alertKey.CustomerNum, alertKey.SiteUseId, "00" + alertKey.AlertType.ToString(), nDefaultLanguage, invoiceIdList, alertKey.Collector, alertKey.ToTitle, alertKey.ToName, alertKey.CCTitle, (alertKey.ResponseDate == null ? "" : Convert.ToDateTime(alertKey.ResponseDate).ToString("yyyy-MM-dd")), alertKey.Region, alertKey.IndexFile);

            nMail = RenderInstance(nMail, nDefaultLanguage, alertKey, invoiceIdList);

            nMail.To = string.Join(";", nTo);
            nMail.Cc = string.Join(";", nCC);
            if (!string.IsNullOrEmpty(nMail.Cc))
            {
                nMail.Cc += ";" + nMail.From;
            }
            else
            {
                nMail.Cc = nMail.From;
            }
            //根据Collector获得发送的组邮箱
            if (!string.IsNullOrEmpty(alertKey.Collector)) {
                var groupMailBox = (from ca in CommonRep.GetDbSet<SysTypeDetail>()
                           where ca.TypeCode == "045" && ca.DetailName == alertKey.Collector
                           select ca.DetailValue2).FirstOrDefault().ToString();
                nMail.From = groupMailBox;
            }

            return nMail;
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="mail"></param>
        private void sendMail(MailTmp mail, AlertKey alertKey, List<int> invoiceIdList)
        {
            int periodId = (int)alertKey.PeriodId;
            soaService.sendSoaSaveInfoToDB(mail, invoiceIdList, alertKey.AlertType, alertKey.Collector, alertKey.LegalEntity, alertKey.CustomerNum, alertKey.SiteUseId, alertKey.ToTitle, alertKey.ToName, alertKey.CCTitle, periodId, alertKey.TempleteLanguage);
        }


        /// <summary>
        /// 根据客户获取自动发送邮件的 Invoice Id List 
        /// </summary>
        /// <param name="custNum"></param>
        /// <param name="siteUseId"></param>
        /// <returns></returns>
        private List<int> GetSendInvIdListNew(string strCollector, string strdeal, string strLegalEntity, string alertCustomerNum, string alertSiteUseId, string alertType, string strToTitle, string strToName, string strTempleteLanguage)
        {
            return soaService.GetAlertAutoSendInvoice(strCollector, strdeal, strLegalEntity, alertCustomerNum, alertSiteUseId, alertType, strToTitle, strToName, strTempleteLanguage);
        }

        /// <summary>
        /// 根据客户获取自动发送邮件的 Invoice Id List 
        /// </summary>
        /// <param name="custNum"></param>
        /// <param name="siteUseId"></param>
        /// <returns></returns>
        private List<int> GetSendInvIdList(string deal, string custNum, string siteUseId)
        {
            string invIdStr = ExecGetAutoSendInvoice(deal,custNum, siteUseId);
            if (string.IsNullOrEmpty(invIdStr))
                return null;

            return invIdStr.Split(',').Where(x => !string.IsNullOrEmpty(x)).Select(x=>int.Parse(x)).ToList();
        }


        /// <summary>
        /// 执行 p_GetAutoSendInvoice 存储过程，获取可执行自动发送的InvoiceID集合(多个 , 号分割)
        /// </summary>
        /// <param name="custNum"></param>
        /// <param name="siteUseId"></param>
        /// <returns></returns>
        private string ExecGetAutoSendInvoice(string deal, string custNum,string siteUseId)
        {
            //DEAL Parameter
            var paramBuildDEAL = new SqlParameter
            {
                ParameterName = "@Deal",
                Value = deal,
                Direction = ParameterDirection.Input
            };
            //CustomerNo Parameter
            var paramBuildCustomerNo = new SqlParameter
            {
                ParameterName = "@CustomerNum",
                Value = custNum,
                Direction = ParameterDirection.Input
            };
            //SiteUseId Parameter
            var paramBuildSiteUseId = new SqlParameter
            {
                ParameterName = "@SiteUseId",
                Value = siteUseId,
                Direction = ParameterDirection.Input
            };
            //Date Parameter
            var paramBuildSysDate = new SqlParameter
            {
                ParameterName = "@Date",
                Value = DateTime.Now,
                Direction = ParameterDirection.Input
            };
            //Reuslt Parameter(0:NG; 1:OK)
            var paramBuildResultInvoiceIds = new SqlParameter
            {
                ParameterName = "@InvoiceIds",
                Value = String.Empty,
                Direction = ParameterDirection.Output
            };

            object[] paramBuildList = new object[5];
            paramBuildList[0] = paramBuildDEAL;
            paramBuildList[1] = paramBuildCustomerNo;
            paramBuildList[2] = paramBuildSiteUseId;
            paramBuildList[3] = paramBuildSysDate;
            paramBuildList[4] = paramBuildResultInvoiceIds;

            Helper.Log.Info("Start: call p_GetAutoSendInvoice(procedure):@DEAL" + deal);

            string idStr = "";
            DataTable nIdTable = CommonRep.ExecuteDataTable(CommandType.StoredProcedure, "p_GetAutoSendInvoice", paramBuildList.ToArray());
            if (nIdTable != null && nIdTable.Rows.Count > 0)
            {
                idStr = nIdTable.Rows[0][0].ToString();
            }
            
            Helper.Log.Info("End: call p_GetAutoSendInvoice(procedure):@DEAL" + deal + ",RETURN:" + idStr);
            
            if (!string.IsNullOrEmpty(idStr))
                idStr = idStr.TrimEnd(',');

            return idStr;
        }
    }
}
