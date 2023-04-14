using Common.Logging;
using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.Domain.Repositories;
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
using System.Threading;
using System.Threading.Tasks;

namespace Intelligent.OTC.Job
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution] //不允许此 Job 并发执行任务（禁止新开线程执行）
    public class AutoSendSOAJobNoBuild : BaseJob
    {
        public OTCRepository CommonRep { get; set; }
        public XcceleratorRepository XcceleratorRep { get; set; }
        ISoaService soaService = SpringFactory.GetObjectImpl<ISoaService>("SoaService");
        IMailService mailService = SpringFactory.GetObjectImpl<IMailService>("MailService");

        protected void init()
        {
            CommonRep = SpringFactory.GetObjectImpl<OTCRepository>("CommonRep");
            XcceleratorRep = SpringFactory.GetObjectImpl<XcceleratorRepository>("XcceleratorRep");
        }

        //public static bool MSwitch = false;
        protected override void ExecuteInternal(IJobExecutionContext context)
        {
            try
            {
                Thread.Sleep(1000);

                init();

                JobDataMap jobDataMap = context.MergedJobDataMap;
                string deal = jobDataMap.GetString("Deal");
                string endTime = jobDataMap.GetString("SubJobEndTimeUTC8");

                ICustomerService ServiceCustomer = SpringFactory.GetObjectImpl<ICustomerService>("CustomerService");

                ////Build Alert
                //var sites = CommonRep.GetQueryable<Sites>().Where(o => o.Deal == deal).ToList();
                //foreach (var item in sites)
                //{
                //    logger.Info("start:SubmitAndBuildAgingDataJob-Build");
                //    int buildResult = ServiceCustomer.BuildInvoiceAgingStatus(deal, item.LegalEntity, "", "", "", "AutoJob");
                //    if (buildResult == 0)
                //    {
                //        logger.Info(string.Format("BuildInvoiceAgingStatus failed,DEAL:{0},LegalEntity:{1}", deal, item.LegalEntity));
                //    }
                //    logger.Debug("End:SubmitAndBuildAgingDataJob-Build, DEAL-" + deal);
                //}

                //判断当前日期是否需要发送SOA
                if (checkNeedSoa(deal))
                {
                    //同步CS/SALES
                    autoBuildContactor();

                    //Send
                    if (autoSendPTP(deal, endTime) > 0) {

                        Thread.Sleep(600000);    //3分钟后发送Mail

                        //SOA发送完后，发出Remind邮件
                        string warnMailReceiver = jobDataMap.GetString("WarnMailReceiver").TrimEnd(';');
                        autoRemindSOA(deal, warnMailReceiver);
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Error("AutoSendPTPJob Error", ex);
            }
        }

        private bool checkNeedSoa(string deal) {
            bool lb_Flag = true;
            
            List<CollectorAlert> nAlertKeyList = CommonRep.GetQueryable<CollectorAlert>().Where(x => x.Status == "Initialized" && x.ToName != "" && x.Deal == deal && x.AlertType != 5).ToList();
            if (nAlertKeyList == null || nAlertKeyList.Count == 0) {
                lb_Flag = false;
            }

            return lb_Flag;
        }

        /// <summary>
        /// 自动同步联系人
        /// </summary>
        private void autoBuildContactor()
        {

            //自动添加CS&Sales
            string strSQL = @"INSERT INTO dbo.T_CONTACTOR
                                ( CUSTOMER_NUM, DEAL, NAME, TITLE, EMAIL_ADDRESS, COMMENT, IS_DEFAULT_FLG, COMMUNICATION_LANGUAGE, LEGAL_ENTITY, TO_CC, SiteUseId, IsCostomerContact)
                                SELECT a.CUSTOMER_NUM, a.DEAL, a.NAME, a.title, a.EMAIL_ADDRESS, a.COMMENT, a.IS_DEFAULT_FLG, a.COMMUNICATION_LANGUAGE, a.LEGAL_ENTITY, a.TO_CC, a.SiteUseId, a.IsCostomerContact FROM (
                                SELECT DISTINCT c.CUSTOMER_NUM, c.DEAL, inv.LsrNameHist AS NAME, 'CS' AS title, clist.email_address AS EMAIL_ADDRESS, 'Auto Find' AS COMMENT, '1' AS IS_DEFAULT_FLG, c.CONTACT_LANGUAGE AS COMMUNICATION_LANGUAGE, c.Organization AS LEGAL_ENTITY, '1' AS TO_CC, c.SiteUseId AS SiteUseId, '1' AS IsCostomerContact FROM dbo.T_INVOICE_AGING AS inv WITH(NOLOCK)
                                JOIN dbo.T_CUSTOMER AS c WITH(NOLOCK) ON c.SiteUseId = inv.SiteUseId
                                JOIN dbo.T_CONTACTOR_LIST AS clist WITH(NOLOCK) ON clist.name = inv.LsrNameHist
                                WHERE ISNULL(c.COLLECTOR,'') <> '' 
                                AND inv.TRACK_STATES NOT IN ('014','016')
                                AND ISNULL(inv.LsrNameHist,'') <> ''
                                AND NOT EXISTS(SELECT 1 FROM dbo.T_CONTACTOR AS con WITH(NOLOCK) WHERE con.SiteUseId = c.SiteUseId AND con.TITLE = 'CS' AND con.NAME = inv.LsrNameHist)
                                UNION
                                SELECT DISTINCT c.CUSTOMER_NUM, c.DEAL, inv.FsrNameHist, 'Sales', clist.email_address, 'Auto Find', '1', c.CONTACT_LANGUAGE, c.Organization, '1', c.SiteUseId, '1' FROM dbo.T_INVOICE_AGING AS inv WITH(NOLOCK)
                                JOIN dbo.T_CUSTOMER AS c WITH(NOLOCK) ON c.SiteUseId = inv.SiteUseId
                                JOIN dbo.T_CONTACTOR_LIST AS clist WITH(NOLOCK) ON clist.name = inv.FsrNameHist
                                WHERE ISNULL(c.COLLECTOR,'') <> '' 
                                AND inv.TRACK_STATES NOT IN ('014','016')
                                AND ISNULL(inv.FsrNameHist,'') <> ''
                                AND NOT EXISTS(SELECT 1 FROM dbo.T_CONTACTOR AS con WITH(NOLOCK) WHERE con.SiteUseId = c.SiteUseId AND con.TITLE = 'Sales' AND con.NAME = inv.FsrNameHist)
                                ) AS a";
            SqlHelper.ExcuteSql(strSQL, null);
        }

        private bool checkIsWorkDay()
        {
            // 检查工厂日历，如果为工作日，正常发邮件
            int cnt = 0;
            System.Data.DataTable dt = this.XcceleratorRep.ExecuteDataTable(System.Data.CommandType.Text
                , string.Format("SELECT COUNT(1) AS Cnt FROM dbo.T_WORKING_CALENDAR WHERE CONVERT(VARCHAR(10),'{0}',120) BETWEEN CONVERT(VARCHAR(10),START_TIME,120) AND CONVERT(VARCHAR(10),END_TIME,120)", DateTime.Now.ToString("yyyy-MM-dd"))
                , null);
            if (dt != null && dt.Rows.Count > 0)
                cnt = (int)dt.Rows[0][0];

            return cnt > 0;
        }

        private int autoSendPTP(string deal,string endTime)
        {
            int intCount = 0;
            int index = 0;
            //2、获取需要发送的客户名单,并根据客户派发发邮件任务（非PMT）
            #region 2-1: Not 001 & 006 & 007
            List<AlertKey> nAlertKeyToCsSalesList = CommonRep.GetQueryable<CollectorAlert>().Where(x => x.Status == "Initialized" && x.ToName != "" && x.Deal == deal && x.AlertType != 5 && x.TempleteLanguage != "001" && x.TempleteLanguage != "006" && x.TempleteLanguage != "009" && x.TempleteLanguage != "007" && x.TempleteLanguage != "0071").Select(x => new AlertKey() { Region = x.Region, Deal = x.Deal, Collector = x.Eid, AlertType = x.AlertType, PeriodId = x.PeriodId, ToTitle = x.ToTitle, ToName = x.ToName, CCTitle = x.CCTitle, ResponseDate = x.ResponseDate, TempleteLanguage = x.TempleteLanguage }).Distinct()
                .OrderBy(o => o.Region).ThenBy(o => o.ToTitle).ThenBy(o => o.ToName).ToList();
            logger.Info("---------------------------------autoSendPTP:" + nAlertKeyToCsSalesList.Count());
            logger.Info("------------------------------------ SOA - To CsSales mailCount：" + nAlertKeyToCsSalesList.Count);
            for (int i = 0; i < nAlertKeyToCsSalesList.Count; i++)
            {
                index = i + 1;
                AlertKey nAlertKey = nAlertKeyToCsSalesList[i];
                if (string.IsNullOrEmpty(nAlertKey.Region))
                {
                    Exception ex = new Exception("Not set Region, Please set first!");
                    logger.Error(ex.Message, ex);
                    continue;
                }
                logger.Info(string.Format("Not To Customer: Create Not PMT SOA Task Deal:{0},PeriodId:{1},Wave:{2},ToTile:{3}", deal, nAlertKey.PeriodId, nAlertKey.AlertType, nAlertKey.ToTitle));
                //根据toname获得Customer
                List<AlertKey> ncustomer = CommonRep.GetQueryable<CollectorAlert>().Where(x => x.Status == "Initialized" && x.Deal == deal && x.Region == nAlertKey.Region && x.PeriodId == nAlertKey.PeriodId && x.AlertType == nAlertKey.AlertType && x.Eid == nAlertKey.Collector && x.ToName == nAlertKey.ToName && x.TempleteLanguage == nAlertKey.TempleteLanguage).Select(x => new AlertKey() { CustomerNum = x.CustomerNum }).Distinct().ToList();
                if (ncustomer != null)
                {
                    foreach (AlertKey ncus in ncustomer)
                    {
                        if (string.IsNullOrEmpty(nAlertKey.CustomerNum))
                        {
                            nAlertKey.CustomerNum = ncus.CustomerNum;
                        }
                        else
                        {
                            nAlertKey.CustomerNum += "," + ncus.CustomerNum;
                        }
                    }
                }
                CreateNewTask(deal, nAlertKey, endTime, index);

                Thread.Sleep(5000);    //20秒1个Mail
            }
            intCount += nAlertKeyToCsSalesList.Count();
            #endregion

            #region 2-2: 001 单Customer
            List<AlertKey> nAlertKeyToCsSalesListCN = CommonRep.GetQueryable<CollectorAlert>().Where(x => x.Status == "Initialized" && x.ToName != "" && x.Deal == deal && x.AlertType != 5 && x.TempleteLanguage == "001").Select(x => new AlertKey() { Region = x.Region, Deal = x.Deal, Collector = x.Eid, AlertType = x.AlertType, LegalEntity = x.LegalEntity, CustomerNum = x.CustomerNum, PeriodId = x.PeriodId, ToTitle = x.ToTitle, CCTitle = x.CCTitle, ResponseDate = x.ResponseDate, TempleteLanguage = x.TempleteLanguage }).Distinct()
                .OrderBy(o => o.Region).ThenBy(o => o.ToTitle).ToList();
            logger.Info("---------------------------------autoSendPTP:" + nAlertKeyToCsSalesListCN.Count());
            logger.Info("------------------------------------ CN SOA - To CsSales mailCount：" + nAlertKeyToCsSalesListCN.Count);
            for (int i = 0; i < nAlertKeyToCsSalesListCN.Count; i++)
            {
                index = i + 1;
                AlertKey nAlertKey = nAlertKeyToCsSalesListCN[i];
                if (string.IsNullOrEmpty(nAlertKey.Region))
                {
                    Exception ex = new Exception("Not set Region, Please set first!");
                    logger.Error(ex.Message, ex);
                    continue;
                }
                logger.Info(string.Format("Not To Customer: Create Not PMT SOA Task Deal:{0},PeriodId:{1},Wave:{2},ToTile:{3}", deal, nAlertKey.PeriodId, nAlertKey.AlertType, nAlertKey.ToTitle));
                //合并toName
                List<string> toNamesList = CommonRep.GetQueryable<CollectorAlert>().Where(x => x.Status == "Initialized"
                                                                                            && x.ToName != ""
                                                                                            && x.Deal == deal
                                                                                            && x.TempleteLanguage == "001"
                                                                                            && x.Region == nAlertKey.Region
                                                                                            && x.Eid == nAlertKey.Collector
                                                                                            && x.AlertType == nAlertKey.AlertType
                                                                                            && x.LegalEntity == nAlertKey.LegalEntity
                                                                                            && x.PeriodId == nAlertKey.PeriodId
                                                                                            && x.ToTitle == nAlertKey.ToTitle
                                                                                            && x.CCTitle == nAlertKey.CCTitle
                                                                                            && x.TempleteLanguage == nAlertKey.TempleteLanguage
                                                                                            && x.CustomerNum == nAlertKey.CustomerNum).Select(o => o.ToName).Distinct().ToList();
                if (toNamesList != null && toNamesList.Count > 0)
                {
                    nAlertKey.ToName = string.Join(";", toNamesList.ToArray());
                }
                else
                {
                    Exception ex = new Exception("No toName!");
                    logger.Error(ex.Message, ex);
                    continue;
                }

                Helper.Log.Info("*********************************************** CustomerNum:" + nAlertKey.CustomerNum);
                Helper.Log.Info("*********************************************** SiteUseId:" + nAlertKey.SiteUseId);
                Helper.Log.Info("*********************************************** ToTitle:" + nAlertKey.ToTitle);
                Helper.Log.Info("*********************************************** CCTitle:" + nAlertKey.CCTitle);
                Helper.Log.Info("*********************************************** ToName:" + nAlertKey.ToName);
                CreateNewTask(deal, nAlertKey, endTime, index);

                Thread.Sleep(5000);    //5秒1个Mail
            }
            intCount += nAlertKeyToCsSalesListCN.Count();
            #endregion
            //3、记录日志
            logger.Info("Task assignment completed!");

            return intCount;
        }

        private void CreateNewTask(string deal,AlertKey alertKey,string endTime,int index)
        {
            int modJob = index % 10;
            FakeProvider jobProvider = SpringFactory.GetObjectImpl<FakeProvider>("FakeProvider");
            var sendSoaMailJobKey = new JobKey("SendSoaMailJob" + modJob, "QueueJobs");
            IJobDetail sendSoaMailJob;
            if (jobProvider.Scheduler.CheckExists(sendSoaMailJobKey))
            {
                sendSoaMailJob = jobProvider.Scheduler.GetJobDetail(sendSoaMailJobKey);
            }
            else
            {
                sendSoaMailJob = JobBuilder.Create<SendSoaMailJob>()
                    .WithIdentity(sendSoaMailJobKey)
                    .StoreDurably()
                    .Build();
            }
            var sendSoaMailTriggerKey = new TriggerKey(Guid.NewGuid().ToString());
            logger.Info("Index：" + index.ToString());
            var sendSoaMailTrigger = TriggerBuilder.Create()
                .UsingJobData("Index", index.ToString())
                .UsingJobData("Collector", alertKey.Collector)
                .UsingJobData("Deal", alertKey.Deal)
                .UsingJobData("LegalEntity", alertKey.LegalEntity == null ? "" : alertKey.LegalEntity)
                .UsingJobData("CustomerNum", alertKey.CustomerNum == null ? "" : alertKey.CustomerNum)
                .UsingJobData("SiteUseId", alertKey.SiteUseId == null ? "" : alertKey.SiteUseId)
                .UsingJobData("AlertType", alertKey.AlertType.ToString())
                .UsingJobData("PeriodId", alertKey.PeriodId == null ? "" : alertKey.PeriodId.ToString())
                .UsingJobData("ToTitle", alertKey.ToTitle == null ? "" : alertKey.ToTitle)
                .UsingJobData("ToName", alertKey.ToName == null ? "" : alertKey.ToName)
                .UsingJobData("CCTitle", alertKey.CCTitle == null ? "" : alertKey.CCTitle)
                .UsingJobData("Region", alertKey.Region)
                .UsingJobData("TempleteLanguage", alertKey.TempleteLanguage)
                .UsingJobData("ResponseDate", (alertKey.ResponseDate == null ? "" : Convert.ToDateTime(alertKey.ResponseDate).ToString("yyyy-MM-dd")))
                .UsingJobData("IndexFile", modJob.ToString())
                .WithIdentity(sendSoaMailTriggerKey)
                .StartAt(DateTime.Now.AddMilliseconds(index * 500))
                .ForJob(sendSoaMailJobKey)
                .Build();
            jobProvider.Scheduler.ScheduleJob(sendSoaMailTrigger);
        }
        
        private void autoRemindSOA(string deal, string warnMailReceiver)
        {
            //SOA发送结果Mail
            List<SendSoaRemindingSum> soaAlertSum = soaService.GetSoaSendWarningSum();
            List<SendSoaRemindingDetail> soaAlertDetail = soaService.GetSoaSendWarningDetail();
            if (soaAlertSum.Count > 0)
            {
                SendSOARemindMail(warnMailReceiver, soaAlertSum, soaAlertDetail, deal);
            }

        }
        private void SendSOARemindMail(string to, List<SendSoaRemindingSum> soaAlertSum, List<SendSoaRemindingDetail> soaAlertDetail, string deal)
        {
            string senderMailbox = mailService.GetWarningSenderMailAddress();
            StringBuilder sb = new StringBuilder();
            sb.Append("<p class=\"MsoNormal\">Dear,\n</br>");
            sb.Append("\n");
            sb.Append(DateTime.Now.ToString("yyyy-MM-dd") + ", SOA发送情况如下：");
            sb.Append("\n");
            sb.Append("<Table cellspacing=\"0\" cellpadding=\"0\" style=\"font-family:'Microsoft YaHei';text-align:center\">" + Environment.NewLine);
            sb.Append("<tr style=\"background:#F0FFFF\"><th style=\"font-size:12px;border-left:#000000 solid 1px;border-top:#000000 solid 1px;border-bottom:#000000 solid 1px;min-width:20px\">#</th><th style=\"font-size:12px;border-left:#000000 solid 1px;border-top:#000000 solid 1px;border-bottom:#000000 solid 1px;min-width:100px\">Status</th><th style=\"font-size:12px;border-left:#000000 solid 1px;border-top:#000000 solid 1px;border-bottom:#000000 solid 1px;min-width:200px\">Collector</th><th style=\"font-size:12px;border-left:#000000 solid 1px;border-top:#000000 solid 1px;border-bottom:#000000 solid 1px;min-width:100px\">Region</th><th style=\"font-size:12px;border-left:#000000 solid 1px;border-top:#000000 solid 1px;border-right:#000000 solid 1px;border-bottom:#000000 solid 1px;min-width:100px\">Count</th></tr>\n");
            int i = 1;
            foreach (SendSoaRemindingSum item in soaAlertSum)
            {
                sb.Append("<tr><td style=\"font-size:12px;border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + i + "</td><td style=\"text-align:left;font-size:12px;border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + item.status + "</td><td style=\"text-align:left;font-size:12px;border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + item.eid + "</td><td style=\"text-align:left;font-size:12px;border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + item.region + "</td><td style=\"text-align:left;font-size:12px;border-left:#000000 solid 1px;border-right:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + item.count + "</td></tr>\n");
                i++;
            }
            sb.Append("</table >\n");
            sb.Append("</p>");
            MailTmp mail = new MailTmp();
            mail.Subject = "***** IOTC - SOA Send Remiding *****";
            mail.Body = sb.ToString();
            mail.From = senderMailbox;
            mail.To = to;
            mail.Attachment = soaService.CreateSendSoaRemindingAttachment(soaAlertDetail);
            mail.Deal = deal;
            mail.Type = "OUT";
            mail.Category = "Sent";
            mail.MailBox = senderMailbox;
            mail.CreateTime = DateTime.Now;
            mail.Operator = "System";

            mailService.SendMail(mail);
            //删除数据
            mailService.deleteFsrLsrChange();

        }


    }

}
