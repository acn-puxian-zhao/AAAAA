using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intelligent.OTC.Business;
using Intelligent.OTC.Common;
using Quartz;
using Quartz.Impl;
using Intelligent.OTC.Domain.DataModel;
using System.Transactions;
using System.Configuration;
using Intelligent.OTC.Domain.Repositories;
using System.Data.SqlClient;
using System.Data;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.Dtos;
using System.Threading;

namespace Intelligent.OTC.Job
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution]//不允许此 Job 并发执行任务（禁止新开线程执行）
    public class SubmitAndBuildAgingDataJob : BaseJob
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

        protected override void ExecuteInternal(IJobExecutionContext context)
        {
            try
            {
                init();

                logger.Info(string.Format("--------------------------------------- Start SubmitAndBuildAgingDataJob ---------------------------------"));

                IJobService ServiceJob = SpringFactory.GetObjectImpl<IJobService>("JobService");
                
                string batchDeal = ConfigurationManager.AppSettings["BatchDeal"];

                try
                {
                    ICustomerService ServiceCustomer = SpringFactory.GetObjectImpl<ICustomerService>("CustomerService");
                    int submitResult = ServiceCustomer.SubmitInitialAgingNew(batchDeal);
                    if (submitResult == 0)
                    {
                        logger.Info(string.Format("SubmitAndBuildAgingDataJob-Aging failed,DEAL:{0}", batchDeal));
                    }
                }
                catch (Exception ex) {
                    Helper.Log.Info(ex.Message);
                }

                logger.Info(string.Format("--------------------------------------- Start getAllInvoiceByUserForArrow ---------------------------------"));

                ServiceJob.getAllInvoiceByUserForArrow("false");
                logger.Info(string.Format("--------------------------------------- End getAllInvoiceByUserForArrow ---------------------------------"));

                //同步新联系人
                logger.Info(string.Format("--------------------------------------- Start autoBuildContactor ---------------------------------"));
                autoBuildContactor();
                logger.Info(string.Format("--------------------------------------- End autoBuildContactor ---------------------------------"));

                Thread.Sleep(180000);    //3分钟后发送Mail

                logger.Info(string.Format("--------------------------------------- Start send contactor change ---------------------------------"));
                JobDataMap jobDataMap = context.MergedJobDataMap;
                string warnMailReceiver = jobDataMap.GetString("WarnMailReceiver").TrimEnd(';');
                autoRemindCollector(batchDeal, warnMailReceiver);
                logger.Info(string.Format("--------------------------------------- End send contactor change ---------------------------------"));


            }
            catch (Exception ex)
            {
                // or set to true if you want to refire
                logger.Error("SubmitAndBuildAgingDataJob,error!" + ex.Message, ex);
            }


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
                                SELECT DISTINCT c.CUSTOMER_NUM, c.DEAL, inv.LsrNameHist AS NAME, 'CS' AS title, 
                                ISNULL((SELECT TOP 1 EMAIL_ADDRESS FROM T_CONTACTOR with(nolock) WHERE TITLE = 'CS' AND NAME = inv.LsrNameHist),(SELECT TOP 1 EMAIL_ADDRESS FROM T_CONTACTOR_HIS with(nolock) WHERE TITLE = 'CS' AND NAME = inv.LsrNameHist))AS EMAIL_ADDRESS, 
                                'Auto Find' AS COMMENT, '1' AS IS_DEFAULT_FLG, c.CONTACT_LANGUAGE AS COMMUNICATION_LANGUAGE, c.Organization AS LEGAL_ENTITY, '1' AS TO_CC, c.SiteUseId AS SiteUseId, '1' AS IsCostomerContact FROM dbo.T_INVOICE_AGING AS inv WITH(NOLOCK)
                                JOIN dbo.T_CUSTOMER AS c WITH(NOLOCK) ON c.SiteUseId = inv.SiteUseId
                                WHERE ISNULL(c.COLLECTOR,'') <> '' 
                                AND inv.TRACK_STATES NOT IN ('014','016')
                                AND ISNULL(inv.LsrNameHist,'') <> ''
                                AND NOT EXISTS(SELECT 1 FROM dbo.T_CONTACTOR AS con WITH(NOLOCK) WHERE con.SiteUseId = c.SiteUseId AND con.TITLE = 'CS' AND con.NAME = inv.LsrNameHist)
                                AND (EXISTS (SELECT 1 FROM T_CONTACTOR AS CON1 WITH (NOLOCK) WHERE con1.TITLE = 'CS' AND con1.NAME = inv.LsrNameHist AND ISNULL(CON1.EMAIL_ADDRESS,'') <> '')
								OR EXISTS (SELECT 1 FROM T_CONTACTOR_HIS AS CON1 WITH (NOLOCK) WHERE con1.TITLE = 'CS' AND con1.NAME = inv.LsrNameHist AND ISNULL(CON1.EMAIL_ADDRESS,'') <> ''))
								UNION
                                SELECT DISTINCT c.CUSTOMER_NUM, c.DEAL, inv.FsrNameHist, 'Sales', 
                                ISNULL((SELECT TOP 1 EMAIL_ADDRESS FROM T_CONTACTOR with(nolock) WHERE TITLE = 'Sales' AND NAME = inv.FsrNameHist), (SELECT TOP 1 EMAIL_ADDRESS FROM T_CONTACTOR_HIS with(nolock) WHERE TITLE = 'Sales' AND NAME = inv.FsrNameHist)), 
                                'Auto Find', '1', c.CONTACT_LANGUAGE, c.Organization, '1', c.SiteUseId, '1' FROM dbo.T_INVOICE_AGING AS inv WITH(NOLOCK)
                                JOIN dbo.T_CUSTOMER AS c WITH(NOLOCK) ON c.SiteUseId = inv.SiteUseId
                                WHERE ISNULL(c.COLLECTOR,'') <> '' 
                                AND inv.TRACK_STATES NOT IN ('014','016')
                                AND ISNULL(inv.FsrNameHist,'') <> ''
                                AND NOT EXISTS(SELECT 1 FROM dbo.T_CONTACTOR AS con WITH(NOLOCK) WHERE con.SiteUseId = c.SiteUseId AND con.TITLE = 'Sales' AND con.NAME = inv.FsrNameHist)
                                AND (EXISTS (SELECT 1 FROM T_CONTACTOR AS CON1 WITH (NOLOCK) WHERE con1.TITLE = 'Sales' AND con1.NAME = inv.FsrNameHist AND ISNULL(CON1.EMAIL_ADDRESS,'') <> '')
								OR EXISTS (SELECT 1 FROM T_CONTACTOR_HIS AS CON1 WITH (NOLOCK) WHERE con1.TITLE = 'Sales' AND con1.NAME = inv.FsrNameHist AND ISNULL(CON1.EMAIL_ADDRESS,'') <> ''))
								) AS a";
            SqlHelper.ExcuteSql(strSQL, null);
        }

        private void autoRemindCollector(string deal, string warnMailReceiver)
        {
            //1、检查文件上传状态，如果有附件未上传，则不发PTP邮件
            string uploadResult = CheckFileUploadStatus();
            if (!string.IsNullOrEmpty(uploadResult))
            {
                if (Convert.ToInt32(DateTime.Now.DayOfWeek) != 1)
                {
                    Helper.Log.Info("**************************** send warn mail start ******************");
                    //星期一，没有AR数据，所以不发邮件
                    SendWarnMail(uploadResult, warnMailReceiver, deal);
                    Helper.Log.Info("**************************** send warn mail end ******************");
                }
                logger.Info("Need file upload,stop auto send mail,Detail : " + uploadResult);
            }
            else {
                if (Convert.ToInt32(DateTime.Now.DayOfWeek) != 1)
                {
                    Helper.Log.Info("**************************** send success mail start ******************");
                    //星期一，没有AR数据，所以不发邮件
                    SendWarnMailSuccess(warnMailReceiver, deal);
                    Helper.Log.Info("**************************** send success mail end ******************");
                }
            }

            logger.Info(string.Format("--------------------------------------- Check no cs/sales ---------------------------------"));

            //校验NoCsSales变化
            List<NoContactorSummary> NoContactorSummaryList = soaService.getNoContactorSummary();
            List<NoContactorSiteUseId> NoContactorSiteUseIdList = soaService.getNoContactorSiteUseId();
            List<NoContactor> NoContactorDetailList = soaService.getNoContactorDetail();
            if (NoContactorSummaryList.Count > 0)
            {
                logger.Info(string.Format("--------------------------------------- lsr changed ---------------------------------"));
                SendNoCsSalesMail(warnMailReceiver, NoContactorSummaryList, NoContactorSiteUseIdList, NoContactorDetailList, deal);
                logger.Info(string.Format("--------------------------------------- send finish ---------------------------------"));
            }

            //新客户信息推送
            List<NewCustomerRemdindingSum> newCustomerSum = soaService.getNewCustomerRemindingSum();
            List<NewCustomerRemdindingDetail> newCustomerDetail = soaService.getNewCustomerRemindingDetail();
            if (newCustomerSum.Count > 0)
            {
                logger.Info(string.Format("--------------------------------------- new customer ---------------------------------"));
                SendNewCustomerMail(warnMailReceiver, newCustomerSum, newCustomerDetail, deal);
                logger.Info(string.Format("--------------------------------------- send finish ---------------------------------"));
            }

        }
        
        /// <summary>
        /// 检查文件上传状态
        /// </summary>
        /// <param name="checkDate"></param>
        private string CheckFileUploadStatus()
        {
            DateTime nNowDate = DateTime.Now;

            //DEAL Parameter
            var paramInputDate = new SqlParameter
            {
                ParameterName = "@InputDate",
                Value = nNowDate,
                Direction = ParameterDirection.Input
            };

            object[] paramBuildList = new object[1];
            paramBuildList[0] = paramInputDate;

            Helper.Log.Info("Start: call p_CheckFileUploadStatus(procedure):@InputDate" + nNowDate.ToString());

            string result = "";
            DataTable nIdTable = CommonRep.ExecuteDataTable(CommandType.StoredProcedure, "p_CheckFileUploadStatus", paramBuildList.ToArray());
            if (nIdTable != null && nIdTable.Rows.Count > 0)
            {
                result = nIdTable.Rows[0][0].ToString();
            }

            Helper.Log.Info("End: call p_CheckFileUploadStatus(procedure):@InputDate" + nNowDate.ToString() + ",RETURN:" + result);

            return result;
        }

        /// <summary>
        /// 发送错误警告邮件
        /// </summary>
        private void SendWarnMail(string detailLog, string to, string deal)
        {
            string senderMailbox = mailService.GetWarningSenderMailAddress();
            string msg = "AR Aging Warning : " + detailLog;
            logger.Info("---------------------SendMailAddress:" + senderMailbox);
            MailTmp mail = new MailTmp();
            mail.Subject = "Auto AR Aging Faild Warnning";
            mail.Body = msg;
            mail.From = senderMailbox;
            mail.To = to;
            mail.Attachment = "";
            mail.Deal = deal;
            mail.Type = "OUT";
            mail.Category = "Sent";
            mail.MailBox = senderMailbox;
            mail.CreateTime = DateTime.Now;
            mail.Operator = "System";

            mailService.SendMail(mail);
        }

        /// <summary>
        /// 发送错误警告邮件
        /// </summary>
        private void SendWarnMailSuccess(string to, string deal)
        {
            string senderMailbox = mailService.GetWarningSenderMailAddress();
            string msg = "AR Download 成功！";
            logger.Info("---------------------SendMailAddress:" + senderMailbox);
            MailTmp mail = new MailTmp();
            mail.Subject = "Auto AR Aging Success Reminding";
            mail.Body = msg;
            mail.From = senderMailbox;
            mail.To = to;
            mail.Attachment = "";
            mail.Deal = deal;
            mail.Type = "OUT";
            mail.Category = "Sent";
            mail.MailBox = senderMailbox;
            mail.CreateTime = DateTime.Now;
            mail.Operator = "System";

            mailService.SendMail(mail);
        }

        private void SendNoCsSalesMail(string to, List<NoContactorSummary> NoContactorSummaryList, List<NoContactorSiteUseId> NoContactorSiteUseIdList, List<NoContactor> NoContactorDetailList, string deal)
        {
            string senderMailbox = mailService.GetWarningSenderMailAddress();
            StringBuilder sb = new StringBuilder();
            sb.Append("<p class=\"MsoNormal\">Dear All,\n");
            sb.Append("<br />");
            sb.Append(DateTime.Now.ToString("yyyy-MM-dd") + ", 无联系方式汇总如下, 详见附件。");
            sb.Append("\n");
            sb.Append("<Table cellspacing=\"0\" cellpadding=\"0\" style=\"font-family:'Microsoft YaHei';text-align:center\">" + Environment.NewLine);
            sb.Append("<tr style=\"background:#F0FFFF\"><th style=\"font-size:12px;border-left:#000000 solid 1px;border-top:#000000 solid 1px;border-bottom:#000000 solid 1px;min-width:20px\">#</th><th style=\"font-size:12px;border-left:#000000 solid 1px;border-top:#000000 solid 1px;border-bottom:#000000 solid 1px;min-width:50px\">Collector</th><th style=\"font-size:12px;border-left:#000000 solid 1px;border-right:#000000 solid 1px;border-top:#000000 solid 1px;border-bottom:#000000 solid 1px;min-width:100px\">Count</th></tr>\n");
            int i = 1;
            foreach (NoContactorSummary item in NoContactorSummaryList)
            {
                sb.Append("<tr><td style=\"font-size:12px;border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + i + "</td><td style=\"text-align:left;font-size:12px;border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + item.Collector + "</td><td style=\"text-align:left;font-size:12px;border-left:#000000 solid 1px;border-right:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + item.count + "</td></tr>\n");
                i++;
            }
            sb.Append("</table >\n");
            sb.Append("其余信息如有缺失也烦请一并维护：");
            sb.Append("<br />");
            sb.Append("Credit Officer");
            sb.Append("<br />");
            sb.Append("Finance Manager");
            sb.Append("<br />");
            sb.Append("Branch Manager");
            sb.Append("<br />");
            sb.Append("Sales Manager");
            sb.Append("<br />");
            sb.Append("Local Finance");
            sb.Append("<br />");
            sb.Append("Financial Controller");
            sb.Append("<br />");
            sb.Append("CS Manager");
            sb.Append("<br />");
            sb.Append("Finance Leader");
            sb.Append("<br />");
            sb.Append("Credit Manager");
            sb.Append("<br />");
            sb.Append("</p>");
            MailTmp mail = new MailTmp();
            mail.Subject = "***** IOTC - No Cs/Sales Reminding *****";
            mail.Body = sb.ToString();
            mail.From = senderMailbox;
            mail.To = to;
            mail.Attachment = soaService.CreateNoCsSalesRemindingAttachment(NoContactorSiteUseIdList, NoContactorDetailList); 
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
        private void SendNewCustomerMail(string to, List<NewCustomerRemdindingSum> newCustomerSum, List<NewCustomerRemdindingDetail> newCustomerDetail, string deal)
        {
            string senderMailbox = mailService.GetWarningSenderMailAddress();
            StringBuilder sb = new StringBuilder();
            sb.Append("<p class=\"MsoNormal\">Dear All,\n");
            sb.Append("<br />");
            sb.Append(DateTime.Now.ToString("yyyy-MM-dd") + " 新客户详情请查看附件。");
            sb.Append("<br />");
            sb.Append("请检查系统自动匹配Collector是否有误，并对【***未分配***】需要Collection的客户指定Collector,Region,Contact Language,IsActive：");
            sb.Append("\n");
            sb.Append("<Table cellspacing=\"0\" cellpadding=\"0\" style=\"font-family:'Microsoft YaHei';text-align:center\">" + Environment.NewLine);
            sb.Append("<tr style=\"background:#F0FFFF\"><th style=\"font-size:12px;border-left:#000000 solid 1px;border-top:#000000 solid 1px;border-bottom:#000000 solid 1px;min-width:20px\">#</th><th style=\"font-size:12px;border-left:#000000 solid 1px;border-top:#000000 solid 1px;border-bottom:#000000 solid 1px;min-width:50px\">Collector</th><th style=\"font-size:12px;border-left:#000000 solid 1px;border-top:#000000 solid 1px;border-bottom:#000000 solid 1px;border-right:#000000 solid 1px;min-width:100px\">Count</th></tr>\n");
            int i = 1;
            foreach (NewCustomerRemdindingSum item in newCustomerSum)
            {
                sb.Append("<tr><td style=\"font-size:12px;border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + i + "</td><td style=\"text-align:left;font-size:12px;border-left:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + item.Collector + "</td><td style=\"text-align:left;font-size:12px;border-left:#000000 solid 1px;border-right:#000000 solid 1px;border-bottom:#000000 solid 1px;\">" + item.count + "</td></tr>\n");
                i++;
            }
            sb.Append("</table >\n");
            sb.Append("其余信息如有缺失也烦请一并维护：");
            sb.Append("<br />");
            sb.Append("Credit Officer");
            sb.Append("<br />");
            sb.Append("Finance Manager");
            sb.Append("<br />");
            sb.Append("Branch Manager");
            sb.Append("<br />");
            sb.Append("Sales Manager");
            sb.Append("<br />");
            sb.Append("Local Finance");
            sb.Append("<br />");
            sb.Append("Financial Controller");
            sb.Append("<br />");
            sb.Append("CS Manager");
            sb.Append("<br />");
            sb.Append("Finance Leader");
            sb.Append("<br />");
            sb.Append("Credit Manager");
            sb.Append("<br />");
            sb.Append("</p>");
            MailTmp mail = new MailTmp();
            mail.Subject = "***** IOTC - New Customer List *****";
            mail.Body = sb.ToString();
            mail.From = senderMailbox;
            mail.To = to;
            mail.Attachment = soaService.CreateNewCustomerRemindingAttachment(newCustomerDetail);
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
