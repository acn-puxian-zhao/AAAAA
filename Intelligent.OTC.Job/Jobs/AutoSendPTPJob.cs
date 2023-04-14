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
using System.Threading;
using System.Threading.Tasks;

namespace Intelligent.OTC.Job
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution] //不允许此 Job 并发执行任务（禁止新开线程执行）
    public class AutoSendPMTJob : BaseJob
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
                init();

                JobDataMap jobDataMap = context.MergedJobDataMap;
                string deal = jobDataMap.GetString("Deal");
                string endTime = jobDataMap.GetString("SubJobEndTimeUTC8");

                ICustomerService ServiceCustomer = SpringFactory.GetObjectImpl<ICustomerService>("CustomerService");
                //Build Alert
                var sites = CommonRep.GetQueryable<Sites>().Where(o => o.Deal == deal).ToList();
                foreach (var item in sites)
                {
                    int buildResult = ServiceCustomer.BuildInvoiceAgingStatus(deal, item.LegalEntity, "", "", "", "AutoJob");
                    if (buildResult == 0)
                    {
                        logger.Info(string.Format("BuildInvoiceAgingStatus failed,DEAL:{0},LegalEntity:{1}", deal, item.LegalEntity));
                    }
                    logger.Debug("End:SubmitAndBuildAgingDataJob-Build, DEAL-" + deal);
                }

                logger.Info("AutoReceiveMailJob,Executing !");

                //同步新联系人
                logger.Info(string.Format("--------------------------------------- Start autoBuildContactor ---------------------------------"));
                autoBuildContactor();
                logger.Info(string.Format("--------------------------------------- End autoBuildContactor ---------------------------------"));

                //Send
                autoSendPTP(deal, endTime);
                Thread.Sleep(180000);    //3分钟后发送Mail

                string warnMailReceiver = jobDataMap.GetString("PMTMailReceiver").TrimEnd(';');
                autoRemindPMT(deal, warnMailReceiver);
            }
            catch (Exception ex)
            {
                logger.Error("AutoSendPTPJob Error", ex);
            }
        }


        private bool checkIsWorkDay()
        {
            // 检查工厂日历，如果为工作日，正常发邮件
            int cnt = 0;
            System.Data.DataTable dt = this.XcceleratorRep.ExecuteDataTable(System.Data.CommandType.Text
                , string.Format("SELECT COUNT(1) AS Cnt FROM dbo.T_WORKING_CALENDAR WITH (NOLOCK) WHERE CONVERT(VARCHAR(10),'{0}',120) BETWEEN CONVERT(VARCHAR(10),START_TIME,120) AND CONVERT(VARCHAR(10),END_TIME,120)", DateTime.Now.ToString("yyyy-MM-dd"))
                , null);
            if (dt != null && dt.Rows.Count > 0)
                cnt = (int)dt.Rows[0][0];

            return cnt > 0;
        }

        private void autoSendPTP(string deal,string endTime)
        {

            int index = 0;
            //3、获取需要发送的客户名单,并根据客户派发发邮件任务(PMT)
            #region 3-1: Not 006 & 007
            List<AlertKey1> nAlertKeyTocssalesListPMT = CommonRep.GetQueryable<CollectorAlert>().Where(x => x.Status == "Initialized" && x.ToName != "" && x.Deal == deal && x.AlertType == 5 && x.TempleteLanguage != "006" && x.TempleteLanguage != "009" && x.TempleteLanguage != "007" && x.TempleteLanguage != "0071").Select(x => new AlertKey1() { Region = x.Region, Deal = x.Deal, Collector = x.Eid, AlertType = x.AlertType, PeriodId = x.PeriodId, ToTitle = x.ToTitle, ToName = x.ToName, CCTitle = x.CCTitle, ResponseDate = x.ResponseDate, TempleteLanguage = x.TempleteLanguage }).Distinct()
                .OrderBy(o => o.Region).ThenBy(o => o.ToTitle).ThenBy(o => o.ToName).ToList();
            index = index + 1;
            for (int i = 0; i < nAlertKeyTocssalesListPMT.Count; i++)
            {
                index = i + 1;
                AlertKey1 nAlertKey1 = nAlertKeyTocssalesListPMT[i];
                if (string.IsNullOrEmpty(nAlertKey1.Region))
                {
                    Exception ex = new Exception("Not set Region, Please set first!");
                    logger.Error(ex.Message, ex);
                    continue;
                }
                
                CreateNewTask(deal, nAlertKey1, endTime, index);

                Thread.Sleep(10000);    //10秒1个Mail
            }
            #endregion
            #region 3-2: 006 & 007
            List<AlertKey1> nAlertKeyToCustomerListPMT = CommonRep.GetQueryable<CollectorAlert>().Where(x => x.Status == "Initialized" && x.ToName != "" && x.Deal == deal && x.AlertType == 5 && (x.TempleteLanguage == "006" || x.TempleteLanguage == "009" || x.TempleteLanguage == "007" || x.TempleteLanguage == "0071") && x.ToName != "" ).Select(x => new AlertKey1() { Region = x.Region, Deal = x.Deal, Collector = x.Eid, AlertType = x.AlertType, PeriodId = x.PeriodId, ToTitle = x.ToTitle, ToName = x.ToName, CCTitle = x.CCTitle, ResponseDate = x.ResponseDate, CustomerNum = x.CustomerNum, TempleteLanguage = x.TempleteLanguage }).Distinct()
                .OrderBy(o => o.Region).ThenBy(o => o.ToTitle).ThenBy(o => o.ToName).ToList();
            index = index + 1;
            for (int i = 0; i < nAlertKeyToCustomerListPMT.Count; i++)
            {
                index = i + 1;
                AlertKey1 nAlertKey1 = nAlertKeyToCustomerListPMT[i];
                if (string.IsNullOrEmpty(nAlertKey1.Region))
                {
                    Exception ex = new Exception("Not set Region, Please set first!");
                    logger.Error(ex.Message, ex);
                    continue;
                }
                logger.Info(string.Format("Create PMT SOA Task Deal:{0},PeriodId:{1},Wave:{2},ToTile:{3},CustomerNum:{4}", deal, nAlertKey1.PeriodId, nAlertKey1.AlertType, nAlertKey1.ToTitle, nAlertKey1.CustomerNum));

                CreateNewTask(deal, nAlertKey1, endTime, index);

                Thread.Sleep(10000);    //10秒1个Mail
            }
            #endregion

            //4、记录日志
            logger.Info("Task assignment completed!");
        }

        private void CreateNewTask(string deal,AlertKey1 AlertKey1,string endTime,int index)
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
                .UsingJobData("Region", AlertKey1.Region)
                .UsingJobData("Collector", AlertKey1.Collector)
                .UsingJobData("Deal", AlertKey1.Deal)
                .UsingJobData("LegalEntity", AlertKey1.LegalEntity == null ? "" : AlertKey1.LegalEntity)
                .UsingJobData("CustomerNum", AlertKey1.CustomerNum == null ? "" : AlertKey1.CustomerNum)
                .UsingJobData("SiteUseId", AlertKey1.SiteUseId == null ? "" : AlertKey1.SiteUseId)
                .UsingJobData("AlertType", AlertKey1.AlertType.ToString())
                .UsingJobData("PeriodId", AlertKey1.PeriodId == null ? "" : AlertKey1.PeriodId.ToString())
                .UsingJobData("ToTitle", AlertKey1.ToTitle == null ? "" : AlertKey1.ToTitle)
                .UsingJobData("ToName", AlertKey1.ToName == null ? "" : AlertKey1.ToName)
                .UsingJobData("CCTitle", AlertKey1.CCTitle == null ? "" : AlertKey1.CCTitle)
                .UsingJobData("TempleteLanguage", AlertKey1.TempleteLanguage)
                .UsingJobData("ResponseDate", (AlertKey1.ResponseDate == null ? "" : Convert.ToDateTime(AlertKey1.ResponseDate).ToString("yyyy-MM-dd")))
                .WithIdentity(sendSoaMailTriggerKey)
                .StartAt(DateBuilder.FutureDate(index * 500, IntervalUnit.Millisecond))
                .ForJob(sendSoaMailJobKey)
                .Build();
            jobProvider.Scheduler.ScheduleJob(sendSoaMailTrigger);
        }
        
        private void autoRemindPMT(string deal, string warnMailReceiver)
        {
            //校验PMT未成功发送Mail
            List<SendPMTSummary> sendSummary = soaService.GetPmtSendSummaryList();
            List<TaskPmtDto> sendDetail = soaService.GetPmtSendDetailList();
            if (sendSummary.Count > 0)
            {
                SendPMTMail(warnMailReceiver, sendSummary, sendDetail, deal);
            }

        }

        private void SendPMTMail(string to, List<SendPMTSummary> sendSummary, List<TaskPmtDto> sendDetail, string deal)
        {
            string senderMailbox = mailService.GetWarningSenderMailAddress();
            StringBuilder sb = new StringBuilder();
            sb.Append("<p class=\"MsoNormal\">Dear,\n<br/>");
            sb.Append("\n");
            sb.Append(DateTime.Now.ToString("yyyy-MM-dd") + "日,PMT发送情况如下：<br/>");
            sb.Append("\n");
            sb.Append("<Table cellspacing=\"0\" cellpadding=\"0\" style=\"font-family:'Microsoft YaHei';text-align:center\">" + Environment.NewLine);
            sb.Append("<tr style=\"background:#F0FFFF\"><th style=\"font-size:12px;border-left:#000000 solid 1px;border-top:#000000 solid 1px;border-bottom:#000000 solid 1px;min-width:20px\">#</th><th style=\"font-size:12px;border-left:#000000 solid 1px;border-top:#000000 solid 1px;border-bottom:#000000 solid 1px;min-width:200px\">Collector</th><th style=\"font-size:12px;border-left:#000000 solid 1px;border-top:#000000 solid 1px;border-bottom:#000000 solid 1px;min-width:200px\">Success</th><th style=\"font-size:12px;border-left:#000000 solid 1px;border-top:#000000 solid 1px;border-bottom:#000000 solid 1px;border-right:#000000 solid 1px;min-width:200px\">Failed</th></tr>\n");
            int i = 1;
            foreach (SendPMTSummary item in sendSummary)
            {
                sb.Append("<tr><td style=\"font-size:12px;border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + i + "</td><td style=\"text-align:left;font-size:12px;border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + item.collector + "</td><td style=\"text-align:left;font-size:12px;border-left:#000000 solid 1px;border-right:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + item.success + "</td><td style=\"max-width:300px;text-align:left;font-size:12px;border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + item.failed + "</td></tr>\n");
                i++;
            }
            sb.Append("</table >\n");
            sb.Append("</p>");
            MailTmp mail = new MailTmp();
            mail.Subject = "***** IOTC - PMT Send Remiding *****";
            mail.Body = sb.ToString();
            mail.From = senderMailbox;
            mail.To = to;
            mail.Attachment = soaService.CreateSendPMTAttachment(sendDetail);
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

    }

    public class AlertKey1
    {
        public string Collector { get; set; }
        public string Deal { get; set; }            //deal
        public string LegalEntity { get; set; }
        public string CustomerNum { get; set; }     //CustomerNum
        public string SiteUseId { get; set; }       //SiteUseId
        public int AlertType { get; set; }   //WaveX
        public int? PeriodId { get; set; }    //账期
        public string ToTitle { get; set; }  //联系人Title
        public string ToName { get; set; }
        public string CCTitle { get; set; }
        public DateTime? ResponseDate { get; set; }  //响应日期
        public string Region { get; set; }
        public string TempleteLanguage { get; set; }    //模板语言
    }
}
