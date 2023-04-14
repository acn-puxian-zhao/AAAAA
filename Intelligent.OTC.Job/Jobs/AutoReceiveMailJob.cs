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
    public class AutoReceiveMailJob : BaseJob
    {
        public OTCRepository CommonRep { get; set; }
        public static string ReceiveMailServiceUrl;
        IMailService mailService = SpringFactory.GetObjectImpl<IMailService>("MailService");
        protected void init()
        {
            ReceiveMailServiceUrl = ConfigurationManager.AppSettings["ReceiveMailServiceUrl"];
            CommonRep = SpringFactory.GetObjectImpl<OTCRepository>("CommonRep");
        }

        protected override void ExecuteInternal(IJobExecutionContext context)
        {
            try
            {
                logger.Info("AutoReceiveMailJob,Executing !");
                init();
                autoReceiveMail();
                //autoBuildContactor();
            }
            catch (Exception ex)
            {
                logger.Error("AutoReceiveMailJob Error", ex);
            }
        }

        private void autoReceiveMail()
        {
            System.Diagnostics.Stopwatch nSw = new System.Diagnostics.Stopwatch();
            nSw.Start();

            //1、获取需要自动收邮件的账户
            List<T_MailAccount> nAccountList = getAutoReceivedAccounts();
            if (nAccountList == null || nAccountList.Count == 0)
            {
                logger.Info("There is no auto received accounts!");
                nSw.Stop();
                return;
            }

            //2、获取抓取前历史数据最大ID
            int lastMailId = GetLastMailId();
            
            //3、逐个调用收邮件方法
            
            for (int i = 0; i < nAccountList.Count; i++)
            {
                T_MailAccount nAccount = nAccountList[i];
                receiveMailByAccount(nAccount, lastMailId);
            }

            //4、记录日志
            logger.Info("Task assignment completed, waitting for results.");
            nSw.Stop();
            double nTotalTime = nSw.ElapsedMilliseconds;
            logger.Info(string.Format("Receive mail task completed,total spend {0} ms", nTotalTime));
            logger.Info("AutoReceiveMailJob,End !");
        }

        /// <summary>
        /// 根据客户获取邮件
        /// </summary>
        /// <param name="account"></param>
        /// <param name="lastId"></param>
        private void receiveMailByAccount(T_MailAccount account,int lastMailId)
        {
            try
            {
                logger.Info(string.Format("Receive mails from '{0}'", account.SenderMailAddress));

                // 1、获取邮件时间
                DateTime nLastTime;
                MailTmp lastMail = CommonRep.GetQueryable<MailTmp>().Where(x => x.MailBox == account.SenderMailAddress && x.Type == "IN").OrderByDescending(m => m.InternalDatetime).FirstOrDefault();
                if (lastMail == null || lastMail.InternalDatetime == null)
                {
                    nLastTime = default(DateTime);
                }
                else
                {
                    nLastTime = lastMail.InternalDatetime.Value;
                }

                // 2、调用 API 收邮件
                receiveMailByUserName(account.SenderMailAddress, nLastTime);

                // 3、检索收回的邮件
                List<Mail> nMailList = CommonRep.GetQueryable<Mail>().Where(x => x.Id > lastMailId && x.Type == "IN" && x.MailBox == account.SenderMailAddress).ToList();
                if (nMailList == null || nMailList.Count == 0)
                {
                    logger.Info(string.Format("No mail from '{0}'", account.SenderMailAddress));
                    return;
                }

                logger.Info(string.Format("Get {0} mails from '{1}'", nMailList.Count(), account.SenderMailAddress));

                // 4、按客户归类邮件，设置 Category , 同步收回的邮件至 T_MAIL_TMP 表，Customer Mail 表
                nMailList.ForEach(x => SaveMail(x));

                CommonRep.Commit();
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Failed to get {0}'s mail.", account.SenderMailAddress), ex);
            }
            
        }

        private void SaveMail(Mail mailmsg)
        {
            // 判断是否已经存在该邮件，如果已经存在则不重复保存
            if (!string.IsNullOrEmpty(mailmsg.FileId)) {
                MailTmp nSavedMailTmp = CommonRep.GetQueryable<MailTmp>().Where(x => x.FileId == mailmsg.FileId).FirstOrDefault();
                if (nSavedMailTmp != null)
                    return;
            }

            MailTmp nMailTmp = new MailTmp() {
                Deal = mailmsg.Deal,
                Subject = mailmsg.Subject,
                Body = mailmsg.Body,
                BodyFormat = mailmsg.BodyFormat,
                From = mailmsg.From,
                To = mailmsg.To,
                Cc = mailmsg.Cc,
                Attachment = mailmsg.Attachment,
                SavePath = mailmsg.SavePath,
                Operator = mailmsg.Operator,
                Category = "Unknow",
                CreateTime = mailmsg.CreateTime,
                UpdateTime = mailmsg.UpdateTime,
                Type = mailmsg.Type,
                Collector = mailmsg.Collector,
                MessageId = mailmsg.MessageId,
                InternalTime = mailmsg.InternalTime,
                MailBox = mailmsg.MailBox,
                InternalDatetime = mailmsg.InternalDatetime,
                FileId = mailmsg.FileId,
                BUSSINESS_REFERENCE = mailmsg.Bussiness_Reference,
                CUSTOMER_ASSIGNED_FLG = mailmsg.CustomerAssignedFlg,
                STATUS = mailmsg.Status
            };

            // 查询邮件中绑定的SiteUseId
            List<string> nContextSiteUseId = MailTemplateContext.GetSiteUseIdsInContext(mailmsg.Body);

            // 查询是否为特定客户邮件
            List <Contactor> nContactors = CommonRep.GetQueryable<Contactor>().Where(x => nContextSiteUseId.Contains(x.SiteUseId)).ToList();

            if(nContactors == null || nContactors.Count == 0)
                nContactors = CommonRep.GetQueryable<Contactor>().Where(x => x.EmailAddress == mailmsg.From).ToList();

            List<Contactor> listCont = nContactors.Select(o => new Contactor { SiteUseId = o.SiteUseId, CustomerNum = o.CustomerNum }).Distinct().ToList();
            if (listCont != null && listCont.Count > 0)
            {
                nMailTmp.Category = "CustomerNew";
                nMailTmp.CustomerMails = new List<CustomerMail>();
                //// 归档至客户下
                foreach (var iContactor in listCont)
                {
                    CustomerMail nCustomerMail = new CustomerMail()
                    {
                        MessageId = mailmsg.MessageId,
                        CustomerNum = iContactor.CustomerNum,
                        SiteUseId = iContactor.SiteUseId
                    };
                    CommonRep.Add(nCustomerMail);
                    nMailTmp.CustomerMails.Add(nCustomerMail);
                }
            }

            CommonRep.Add(nMailTmp);
            //CommonRep.Commit();
        }

        /// <summary>
        /// 提交更新邮件请求
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="lastTime"></param>
        /// <returns></returns>
        private string receiveMailByUserName(string userName,DateTime lastTime)
        {
            int timeOut = int.Parse(System.Configuration.ConfigurationManager.AppSettings["ReceiveMailRequestTimeout"]);
            Dictionary<string,string> nPostDict = new Dictionary<string, string>();
            nPostDict["userName"] = userName;
            nPostDict["lastReceivedTime"] = lastTime.ToString();
            HttpWebResponse response = HttpRequestHelper.CreatePostHttpResponse(ReceiveMailServiceUrl, nPostDict, Encoding.UTF8, timeOut);
            using (StreamReader stream = new StreamReader(response.GetResponseStream()))
            {
                return stream.ReadToEnd();
            }
        }
        

        /// <summary>
        /// 获取需要自动抓取的客户信息
        /// </summary>
        /// <returns></returns>
        protected List<T_MailAccount> getAutoReceivedAccounts()
        {
            List<T_MailAccount> nAccountList = null;
            nAccountList = CommonRep.GetQueryable<T_MailAccount>().Where(x => x.AutoReceive == true).ToList();
            
            return nAccountList;
        }

        private int GetLastMailId()
        {
            Mail mail = CommonRep.GetQueryable<Mail>().OrderByDescending(m => m.Id).FirstOrDefault(q => q.Type == "IN");
            return mail == null ? 0 : mail.Id;
        }
        private void autoBuildContactor()
        {

            //先将联系人表中已有的CS&Sales与联系人列表同步(可能有用户自己添加的，系统之前没有自动识别到的)
            buildContactorList();
            List<ContactorNameDto> listContactor = getNeedFindContactor();
            SendContactorNameDto sendContactor = new SendContactorNameDto();
            sendContactor.sender = mailService.GetWarningSenderMailAddress();
            sendContactor.names = listContactor;
            mailService.findContactor(sendContactor);
            
        }


        //同步已有联系人信息到联系人列表
        private void buildContactorList()
        {
            string sql = string.Format(@"INSERT INTO T_CONTACTOR_LIST
                                                    (
                                                        Name,
                                                        EMAIL_ADDRESS
                                                    )
                                                    SELECT DISTINCT
                                                           NAME,
                                                           EMAIL_ADDRESS 
                                                    FROM dbo.T_CONTACTOR WITH (NOLOCK)
                                                    WHERE TITLE IN ( 'CS', 'Sales' )
                                                          AND ISNULL(NAME, '') <> ''
                                                          AND ISNULL(EMAIL_ADDRESS, '') <> ''
                                                          AND NOT EXISTS
                                                    (
                                                        SELECT 1
                                                        FROM T_CONTACTOR_LIST AS LIST WITH (NOLOCK)
                                                        WHERE LIST.NAME = T_CONTACTOR.NAME
                                                              AND LIST.EMAIL_ADDRESS = T_CONTACTOR.EMAIL_ADDRESS
                                                    )
                                                ");

            SqlHelper.ExcuteSql(sql, null);
        }

        /// <summary>
        /// 获得需要查找的EID
        /// </summary>
        /// <returns></returns>
        private List<ContactorNameDto> getNeedFindContactor()
        {
            string sql = @"SELECT DISTINCT T.Name as name FROM (
                            SELECT DISTINCT LsrNameHist AS Name FROM dbo.T_INVOICE_AGING WITH (NOLOCK)
                            WHERE TRACK_STATES NOT IN ('014','016')
                            AND (SUBSTRING(UPPER(LsrNameHist),1,1) >= 'A' AND  SUBSTRING(UPPER(LsrNameHist),1,1) <= 'Z')
                            UNION
                            SELECT DISTINCT FsrNameHist AS Name FROM dbo.T_INVOICE_AGING WITH (NOLOCK)
                            WHERE TRACK_STATES NOT IN ('014','016')
                            AND (SUBSTRING(UPPER(FsrNameHist),1,1) >= 'A' AND  SUBSTRING(UPPER(FsrNameHist),1,1) <= 'Z')
                            ) AS T
                            WHERE NOT EXISTS(SELECT 1 FROM dbo.T_CONTACTOR_LIST WITH (NOLOCK) WHERE NAME = T.NAME) 
                                  and not exists (select 1 from T_Contactor_CanntResolve WITH (NOLOCK) where name = t.name)
                            order by t.name";
            DataTable dt = SqlHelper.ExcuteTable(sql, System.Data.CommandType.Text, null);
            return SqlHelper.GetList<ContactorNameDto>(dt);
        }

    }
}
