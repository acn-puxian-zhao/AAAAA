using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.Dtos;
using Quartz;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Intelligent.OTC.Job
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution] //不允许此 Job 并发执行任务（禁止新开线程执行）
    public class AutoSendCaMailJob : BaseJob
    {
        protected override void ExecuteInternal(IJobExecutionContext context)
        {
            try
            {
                autoSendCaMailResult();
                autoSendCaMail();
            }
            catch (Exception ex)
            {
                logger.Error("AutoSendCaDetailMailJob Error", ex);
            }
        }
        private void autoSendCaMailResult() {
            string strSql = string.Format(@"SELECT Id,IOCBusinessId as BusinessId,MessageId,Status,ErrorMessage FROM T_MailAdvisor_CASendMail WHERE MAProcessFlag = 1 AND CAProcessFlag = 0");
            List<T_MailAdvisor_CASendMailDto> sendList = SqlHelperMailAdvisor.GetList<T_MailAdvisor_CASendMailDto>(SqlHelperMailAdvisor.ExcuteTable(strSql, CommandType.Text));
            List<string> sqlIOTCList = new List<string>();
            List<string> sqlInterfaceList = new List<string>();
            foreach (T_MailAdvisor_CASendMailDto send in sendList) {
                string strBusinessIds = "'" + send.BusinessId.Replace(",", "','") + "'";
                if (send.Status.ToLower() == "success")
                {
                    string strUpdateIOTCStatus = string.Format(@"Update T_CA_MailAlert SET ISLOCKED = 0, Status = 'Finish',FactMessageId='{1}' WHERE ID in ({0})", strBusinessIds, send.MessageId);
                    string strUpdateInterfaceStatus = string.Format(@"Update T_MailAdvisor_CASendMail SET CAProcessFlag = 1 WHERE ID = '{0}'", send.Id);
                    Helper.Log.Info("IOTC:" + strUpdateIOTCStatus);
                    Helper.Log.Info("Interface:" + strUpdateInterfaceStatus);
                    sqlIOTCList.Add(strUpdateIOTCStatus);
                    sqlInterfaceList.Add(strUpdateInterfaceStatus);
                }
            }
            if (sqlIOTCList.Count > 0)
            {
                SqlHelper.ExcuteListSql(sqlIOTCList);
            }
            if (sqlInterfaceList.Count > 0)
            {
                SqlHelperMailAdvisor.ExcuteListSql(sqlInterfaceList);
            }
        }
        private void autoSendCaMail() {

            int index = 0;
            //PostConfirm Mail
            string strSql = @"SELECT distinct STUFF((
												   SELECT ',' + [t].[ID]
												   FROM dbo.V_CA_MailAlert t WITH (NOLOCK)
												   WHERE t.EID = V_CA_MailAlert.EID
														 AND t.AlertType = V_CA_MailAlert.AlertType
														 AND T.LegalEntity = V_CA_MailAlert.LegalEntity
														 AND isnull(T.CustomerNum,'') = isnull(V_CA_MailAlert.CustomerNum,'')
														 AND isnull(T.ToTitle,'') = isnull(V_CA_MailAlert.ToTitle,'')
														 AND isnull(T.CCTitle,'') = isnull(V_CA_MailAlert.CCTitle,'')
														 AND isnull(T.businessId,'') = isnull(V_CA_MailAlert.businessId,'')
                                                         and t.AlertType = '006' and t.STATUS = 'Initialized' and isnull(Comment,'') = '' and (ISLOCKED = 0 or (ISLOCKED = 1 and DATEADD(HOUR, 6, isnull(lockeddate,getdate())) >= getdate()))
												   FOR XML PATH('')
											   ),
											   1,
											   1,
											   '') AS ID, STUFF((
												   SELECT ',' + [t].[TransNumber]
												   FROM dbo.V_CA_MailAlert t WITH (NOLOCK)
												   WHERE t.EID = V_CA_MailAlert.EID
														 AND t.AlertType = V_CA_MailAlert.AlertType
														 AND T.LegalEntity = V_CA_MailAlert.LegalEntity
														 AND isnull(T.CustomerNum,'') = isnull(V_CA_MailAlert.CustomerNum,'')
														 AND isnull(T.ToTitle,'') = isnull(V_CA_MailAlert.ToTitle,'')
														 AND isnull(T.CCTitle,'') = isnull(V_CA_MailAlert.CCTitle,'')
														 AND isnull(T.businessId,'') = isnull(V_CA_MailAlert.businessId,'')
                                                         and t.AlertType = '006' and t.STATUS = 'Initialized' and isnull(Comment,'') = '' and (ISLOCKED = 0 or (ISLOCKED = 1 and DATEADD(HOUR, 6, isnull(lockeddate,getdate())) >= getdate()))
												   FOR XML PATH('')
											   ),
											   1,
											   1,
											   '') AS TransNumber,
                                            STUFF((
												   SELECT ',' + [t].[BSID]
												   FROM dbo.V_CA_MailAlert t WITH (NOLOCK)
												   WHERE t.EID = V_CA_MailAlert.EID
														 AND t.AlertType = V_CA_MailAlert.AlertType
														 AND T.LegalEntity = V_CA_MailAlert.LegalEntity
														 AND isnull(T.CustomerNum,'') = isnull(V_CA_MailAlert.CustomerNum,'')
														 AND isnull(T.ToTitle,'') = isnull(V_CA_MailAlert.ToTitle,'')
														 AND isnull(T.CCTitle,'') = isnull(V_CA_MailAlert.CCTitle,'')
														 AND isnull(T.businessId,'') = isnull(V_CA_MailAlert.businessId,'')
                                                         and t.AlertType = '006' and t.STATUS = 'Initialized' and isnull(Comment,'') = '' and (ISLOCKED = 0 or (ISLOCKED = 1 and DATEADD(HOUR, 6, isnull(lockeddate,getdate())) >= getdate()))
												   FOR XML PATH('')
											   ),
											   1,
											   1,
											   '') AS BSID,
                                        businessId,
                                            EID, AlertType, LegalEntity,CustomerNum, isnull(TOTITLE, '') as TOTITLE, isnull(CCTITLE,'') as CCTITLE, Convert(CHAR(10),CreateTime,120) as strCreateTime
                               FROM V_CA_MailAlert  with (nolock) 
                              WHERE AlertType = '006' and STATUS = 'Initialized' and isnull(Comment,'') = '' and (ISLOCKED = 0 or (ISLOCKED = 1 and DATEADD(HOUR, 6, isnull(lockeddate,getdate())) >= getdate()))
                             order by Convert(CHAR(10),CreateTime,120) desc";
            List<CaMailAlertDto> list = SqlHelper.GetList<CaMailAlertDto>(SqlHelper.ExcuteTable(strSql, System.Data.CommandType.Text, null));
            string strUpdateSQL = string.Format("Update T_CA_MailAlert set islocked = 1, lockeddate = getdate() WHERE AlertType = '006' and STATUS = 'Initialized' and isnull(Comment,'') = '' and ISLOCKED = 0");
            SqlHelper.ExecuteNonQuery(System.Data.CommandType.Text, strUpdateSQL, null);
            foreach (CaMailAlertDto cm in list)
            {
                index = index + 1;
                logger.Info("index:" + index);
                CreateNewTask(cm, index);
                Thread.Sleep(5000);    //5秒1个Mail
            }

            //ClearConfirm Mail
            string strSqlClear = @"SELECT distinct STUFF((
												   SELECT ',' + [t].[ID]
												   FROM dbo.V_CA_MailAlert t WITH (NOLOCK)
												   WHERE t.EID = V_CA_MailAlert.EID
														 AND t.AlertType = V_CA_MailAlert.AlertType
														 AND T.LegalEntity = V_CA_MailAlert.LegalEntity
														 AND isnull(T.CustomerNum,'') = isnull(V_CA_MailAlert.CustomerNum,'')
														 AND isnull(T.ToTitle,'') = isnull(V_CA_MailAlert.ToTitle,'')
														 AND isnull(T.CCTitle,'') = isnull(V_CA_MailAlert.CCTitle,'')
														 AND isnull(T.businessId,'') = isnull(V_CA_MailAlert.businessId,'')
                                                         and t.AlertType = '008' and t.STATUS = 'Initialized' and isnull(Comment,'') = '' and (ISLOCKED = 0 or (ISLOCKED = 1 and DATEADD(HOUR, 6, isnull(lockeddate,getdate())) >= getdate()))
												   FOR XML PATH('')
											   ),
											   1,
											   1,
											   '') AS ID,
											   STUFF((
												   SELECT ',' + [t].[TransNumber]
												   FROM dbo.V_CA_MailAlert t WITH (NOLOCK)
												   WHERE t.EID = V_CA_MailAlert.EID
														 AND t.AlertType = V_CA_MailAlert.AlertType
														 AND T.LegalEntity = V_CA_MailAlert.LegalEntity
														 AND isnull(T.CustomerNum,'') = isnull(V_CA_MailAlert.CustomerNum,'')
														 AND isnull(T.ToTitle,'') = isnull(V_CA_MailAlert.ToTitle,'')
														 AND isnull(T.CCTitle,'') = isnull(V_CA_MailAlert.CCTitle,'')
														 AND isnull(T.businessId,'') = isnull(V_CA_MailAlert.businessId,'')
                                                         and t.AlertType = '008' and t.STATUS = 'Initialized' and isnull(Comment,'') = '' and (ISLOCKED = 0 or (ISLOCKED = 1 and DATEADD(HOUR, 6, isnull(lockeddate,getdate())) >= getdate()))
												   FOR XML PATH('')
											   ),
											   1,
											   1,
											   '') AS TransNumber, 
                                                STUFF((
												   SELECT ',' + [t].[BSID]
												   FROM dbo.V_CA_MailAlert t WITH (NOLOCK)
												   WHERE t.EID = V_CA_MailAlert.EID
														 AND t.AlertType = V_CA_MailAlert.AlertType
														 AND T.LegalEntity = V_CA_MailAlert.LegalEntity
														 AND isnull(T.CustomerNum,'') = isnull(V_CA_MailAlert.CustomerNum,'')
														 AND isnull(T.ToTitle,'') = isnull(V_CA_MailAlert.ToTitle,'')
														 AND isnull(T.CCTitle,'') = isnull(V_CA_MailAlert.CCTitle,'')
														 AND isnull(T.businessId,'') = isnull(V_CA_MailAlert.businessId,'')
                                                         and t.AlertType = '008' and t.STATUS = 'Initialized' and isnull(Comment,'') = '' and (ISLOCKED = 0 or (ISLOCKED = 1 and DATEADD(HOUR, 6, isnull(lockeddate,getdate())) >= getdate()))
												   FOR XML PATH('')
											   ),
											   1,
											   1,
											   '') AS BSID, 
                                         businessId,
                                                EID, AlertType, LegalEntity,CustomerNum,
                                                isnull(TOTITLE, '') as TOTITLE, isnull(CCTITLE,'') as CCTITLE, Convert(CHAR(10),CreateTime,120) as strCreateTime
                               FROM V_CA_MailAlert with (nolock) 
                              WHERE AlertType = '008' and STATUS = 'Initialized' and isnull(Comment,'') = '' and (ISLOCKED = 0 or (ISLOCKED = 1 and DATEADD(HOUR, 6, isnull(lockeddate,getdate())) >= getdate()))
                              order by Convert(CHAR(10),CreateTime,120) desc";
            List<CaMailAlertDto> listClear = SqlHelper.GetList<CaMailAlertDto>(SqlHelper.ExcuteTable(strSqlClear, System.Data.CommandType.Text, null));
            string strUpdateSQLClear = string.Format("Update T_CA_MailAlert set islocked = 1, lockeddate = getdate() WHERE AlertType = '008' and STATUS = 'Initialized' and isnull(Comment,'') = '' and ISLOCKED = 0");
            SqlHelper.ExecuteNonQuery(System.Data.CommandType.Text, strUpdateSQLClear, null);
            foreach (CaMailAlertDto cm in listClear)
            {
                index = index + 1;
                logger.Info("index:" + index);
                CreateNewTask(cm, index);
                Thread.Sleep(5000);    //5秒1个Mail
            }

            //4、记录日志
            logger.Info("Task assignment completed!");
        }
        private void CreateNewTask(CaMailAlertDto AlertKey, int index)
        {
            int modJob = index % 5;
            FakeProvider jobProvider = SpringFactory.GetObjectImpl<FakeProvider>("FakeProvider");
            var sendCaMailJobKey = new JobKey("SendCaMailJob" + modJob, "QueueJobs");
            IJobDetail sendCaMailJob;
            if (jobProvider.Scheduler.CheckExists(sendCaMailJobKey))
            {
                sendCaMailJob = jobProvider.Scheduler.GetJobDetail(sendCaMailJobKey);
            }
            else
            {
                sendCaMailJob = JobBuilder.Create<SendCaMailJob>()
                    .WithIdentity(sendCaMailJobKey)
                    .StoreDurably()
                    .Build();
            }
            //截止执行时间,需要设置截止时间,防止到人员作业开始时还没发送完毕
            var sendCaMailTriggerKey = new TriggerKey(Guid.NewGuid().ToString());
            logger.Info("Index：" + index.ToString());

            logger.Info("AlertKey.ID.ToString()：" + AlertKey.ID.ToString());
            logger.Info("AlertKey.EID：" + AlertKey.EID);
            logger.Info("AlertKey.BSID：" + AlertKey.BSID);
            logger.Info("AlertKey.TransNumber：" + AlertKey.TransNumber);
            logger.Info("AlertKey.AlertType" + AlertKey.AlertType == null ? "" : AlertKey.AlertType);
            logger.Info("AlertKey.LegalEntity" + AlertKey.LegalEntity == null ? "" : AlertKey.LegalEntity);
            logger.Info("AlertKey.CustomerNum" + AlertKey.CustomerNum == null ? "" : AlertKey.CustomerNum);
            logger.Info("AlertKey.SiteUseId" + AlertKey.SiteUseId == null ? "" : AlertKey.SiteUseId);
            logger.Info("AlertKey.TOTITLE" + AlertKey.TOTITLE == null ? "" : AlertKey.TOTITLE);
            logger.Info("AlertKey.CCTITLE" + AlertKey.CCTITLE == null ? "" : AlertKey.CCTITLE);

            var sendCaMailTrigger = TriggerBuilder.Create()
                .UsingJobData("Index", index.ToString())
                .UsingJobData("ID", AlertKey.ID.ToString())
                .UsingJobData("EID", AlertKey.EID)
                .UsingJobData("BSID", AlertKey.BSID)
                .UsingJobData("TransNumber", AlertKey.TransNumber)
                .UsingJobData("AlertType", AlertKey.AlertType == null ? "" : AlertKey.AlertType)
                .UsingJobData("LegalEntity", AlertKey.LegalEntity == null ? "" : AlertKey.LegalEntity)
                .UsingJobData("CustomerNum", AlertKey.CustomerNum == null ? "" : AlertKey.CustomerNum)
                .UsingJobData("SiteUseId", AlertKey.SiteUseId == null ? "" : AlertKey.SiteUseId)
                .UsingJobData("TOTITLE", AlertKey.TOTITLE == null ? "" : AlertKey.TOTITLE)
                .UsingJobData("CCTITLE", AlertKey.CCTITLE == null ? "" : AlertKey.CCTITLE)
                .UsingJobData("BusinessId", AlertKey.businessId == null ? "" : AlertKey.businessId)
                .UsingJobData("IndexFile", modJob.ToString())
                .WithIdentity(sendCaMailTriggerKey)
                .StartAt(DateBuilder.FutureDate(index * 500, IntervalUnit.Millisecond))
                .ForJob(sendCaMailJobKey)
                .Build();
            jobProvider.Scheduler.ScheduleJob(sendCaMailTrigger);
        }

    }

}
