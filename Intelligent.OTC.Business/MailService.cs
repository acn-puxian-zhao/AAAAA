using EntityFramework.BulkInsert.Extensions;
using Google.Apis.Gmail.v1.Data;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.UnitOfWork;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.Domain.Partials;
using Intelligent.OTC.Domain.Repositories;
using MimeKit;
using Spring.Expressions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Linq.SqlClient;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Intelligent.OTC.Business
{
    public class MailService : IMailService
    {
        private string IsMail = ConfigurationManager.AppSettings["IsMail"].ToString();
        public OTCRepository CommonRep { get; set; }
        public IMailer Mailer { get; set; }

        public IQueryable<MailTemplate> GetMailTemplates()
        {
            return CommonRep.GetQueryable<MailTemplate>();
        }

        public List<MailTemplateDto> GetMailTemplates(string language, string type)
        {
            var query = CommonRep.GetQueryable<MailTemplate>();
            if (!string.IsNullOrWhiteSpace(language))
            {
                query = query.Where(o => o.Language == language);
            }
            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(o => o.Type == type);
            }
            var result = query.OrderBy(o => o.Id).ToList();
            var t = CommonRep.GetQueryable<SysTypeDetail>().Where(o=>o.TypeCode == "012");
            var l = CommonRep.GetQueryable<SysTypeDetail>().Where(o => o.TypeCode == "013");

            List<MailTemplateDto> dtos = new List<MailTemplateDto>();

            foreach (var item in result)
            {
                var itemDto = new MailTemplateDto()
                {
                    Id = item.Id,
                    Language = item.Language,
                    Type = item.Type,
                    Subject = item.Subject,
                    MainBody = item.MainBody,
                    Creater = item.Creater,
                    CreateDate = item.CreateDate
                };

                var item_l = l.FirstOrDefault(o=>o.DetailValue == item.Language);
                if (item_l != null)
                {
                    itemDto.LanguageName = item_l.DetailName;
                }

                var item_t = t.FirstOrDefault(o => o.DetailValue == item.Type);
                if (item_t != null)
                {
                    itemDto.TypeName = item_t.DetailName;
                }

                dtos.Add(itemDto);
            }

            return dtos;
        }

        public MailTemplate GetMailTemplateById(int templateId)
        {
            return CommonRep.FindBy<MailTemplate>(templateId);
        }

        public MailTemplate GetMailTemplate(string type)
        {
            var tpl = (from t in CommonRep.GetQueryable<MailTemplate>()
                       where t.Type == type
                       select t).FirstOrDefault();
            return tpl;
        }

        public MailTemplate GetMailTemplatebytype(string type, string lang)
        {
            var tpl = (from t in CommonRep.GetQueryable<MailTemplate>()
                       where t.Type == type && t.Language == lang
                       select t).FirstOrDefault();
            return tpl;
        }

        public string GetMailCountDistinct(string category)
        {
            string senderMailAddress = GetSenderMailAddress();
            int cnt = CommonRep.GetQueryable<MailTmp>().Where(x => x.MailBox == senderMailAddress && x.Category == category).Count();
            return cnt.ToString();
        }

        public void deleteFsrLsrChange() {
            CommonRep.GetDBContext().Database.ExecuteSqlCommand("TRUNCATE TABLE dbo.T_LSRFSR_CHANGE");
        }

        /// <summary>
        /// query mail count of all category 
        /// </summary>
        /// <returns></returns>
        public MailCountDto QueryMailCount()
        {
            MailCountDto result;
            result =  StaticCacheHelper.Get<MailCountDto>(AppContext.Current.User.EID, "mailcount");
            if (result == null || result.Total == 0)
            {
                string senderMailAddress = GetSenderMailAddress();
                string sql = string.Format(@"select isnull(sum(ISNULL(t.CustomerNew,0)),0) as CustomerNew ,isnull(sum(ISNULL(t.Unknow,0)),0) as Unknow,isnull(sum((case when t.COLLECTOR = '{1}' then isnull(t.Draft,0) else 0 end)),0) as Draft,
                                        isnull(sum(ISNULL(t.Sent,0)),0) as Sent,isnull(sum(ISNULL(t.Processed,0)),0) as Processed, isnull(sum(ISNULL(t.Pending,0)),0) as Pending from(
                                select o.category, o.collector, count(distinct o.ID) as count
                                from [dbo].[T_MAIL_TMP]  o  
                                where (o.MAIL_BOX = '{0}' or o.[FROM] = '{0}' or o.[cc] like '%{0}%' or o.[subject] like '%{1}%')
                                group by o.category, o.collector) as tmp
                                PIVOT(sum(tmp.count) FOR category in ([CustomerNew],[Unknow],[Draft],[Sent],[Processed],[Pending])) as t",
                                    senderMailAddress, AppContext.Current.User.EID);
                result = CommonRep.ExecuteSqlQuery<MailCountDto>(sql).FirstOrDefault() ;
                if (result == null)
                {
                    result = new MailCountDto();
                }
                StaticCacheHelper.Add(AppContext.Current.User.EID, "mailcount", 300, result);
            }

            return result;
        }

        /// <summary>
        /// All mail in OTC platform is going through the process of creation from template.
        /// Defaultly, This method use the template passed in and create mail instance by taking the registed context parameters and calculate it towards the template.
        /// The output of this method is a new mail instance created from the ParseTemplate method.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="registContextCallBack"></param>
        /// <returns></returns>
        public MailTmp GetInstanceFromTemplate(MailTemplate template, Action<ITemplateParser> registContextCallBack, string Region = "")
        {
            return GetInstanceFromTemplate(template, registContextCallBack, AppContext.Current.User, Region);
        }

        public MailTmp GetInstanceFromTemplate(MailTemplate template, Action<ITemplateParser> registContextCallBack, SysUser User, string Region = "")
        {
            // 1, create the mail instance.
            MailTmp ins = new MailTmp();
            ins.From = GetSenderMailAddress(Region);
            ins.Subject = template.Subject;
            ins.MessageId = Guid.NewGuid().ToString();
            ins.Type = "OUT";
            ins.Category = "Draft";


            // 2, regist outer references.
            string insBody = string.Empty;
            ITemplateParser parser = new TemplateParser();
            if (registContextCallBack != null)
            {
                registContextCallBack(parser);
            }

            // by default regist common context.
            parser.RegistContext("mailContext", new MailTemplateContext() { Parser = parser, CommonRep = this.CommonRep });
            parser.RegistContext("appContext.CurrentUser", User);

            // 3, purse the body
            parser.ParseTemplate(template.MainBody, out insBody);
            ins.Body = insBody;


            return ins;
        }

        public string GetSenderMailAddressByOperator(string strOperator)
        {
            return GetSenderMailAddress(strOperator, "");
        }

        public string GetSenderMailAddress(string Region = "")
        {
            return GetSenderMailAddress(AppContext.Current.User.EID, AppContext.Current.User.Email);
        }

        public string GetSenderMailAddress(string userId, string email, string Region = "")
        {
            string strMailAddress = "";
            if (!string.IsNullOrEmpty(Region))
            {
                //如果有区域参数，按区域取(用于MailJOB，无Collector登录的情况)
                strMailAddress = CommonRep.GetQueryable<SysTypeDetail>()
                    .Where(o => o.TypeCode == "044" && o.DetailName == Region)
                    .Select(x => x.DetailValue2).FirstOrDefault();
            }
            else if (!string.IsNullOrEmpty(userId))
            {
                //没有参数，按Collector获得其所使用的组邮箱
                strMailAddress = CommonRep.GetQueryable<SysTypeDetail>()
                    .Where(o => o.TypeCode == "045" && o.DetailName == userId)
                    .Select(x => x.DetailValue2).FirstOrDefault();
            }
            else
            {
                strMailAddress = bool.Parse(ConfigurationManager.AppSettings["IsEnableGroupMail"]) == true ? ConfigurationManager.AppSettings["GroupMailAddress"] : email;
            }

            return strMailAddress;
        }

        public string GetWarningSenderMailAddress()
        {
            return GetWarningSenderMailAddress(AppContext.Current.User.Email);
        }

        public string GetWarningSenderMailAddress(string mail)
        {
            string strMailAddress = bool.Parse(ConfigurationManager.AppSettings["IsEnableGroupMail"]) == true ? ConfigurationManager.AppSettings["GroupMailAddress"] : mail;

            return strMailAddress;
        }

        public void MarkAsRead(string msgId)
        {
            bool isMail = false;
            if (bool.TryParse(ConfigurationManager.AppSettings["IsMail"], out isMail) && isMail)
            {
                UserMailInput usermail = new UserMailInput();
                var uMail = (from m in CommonRep.GetQueryable<Mail>() where (m.MessageId == msgId) select m).FirstOrDefault();
                usermail.userName = uMail.MailBox;
                usermail.messageId = uMail.FileId;
                if (!string.IsNullOrEmpty(usermail.messageId))
                {
                    Mailer.MarkAsRead(usermail);
                }
                else
                {
                    Helper.Log.Info("FileId is Null");
                }
            }
        }

        public SysTypeDetail getMailSoaInfoByAlert(string type, string region) {
            return CommonRep.GetQueryable<SysTypeDetail>().Where(o => o.TypeCode == "047" && o.DetailName == type && o.Description == region).FirstOrDefault();
        }

        public MailTmp SendMail(MailTmp mail, string TempleteLanguage = "", string Collector = "", bool lb_FactSend = false)
        {
            try
            {
                //Mail是直接发送，还是生成Draft
                bool isDraftMail = false;
                Boolean.TryParse(ConfigurationManager.AppSettings["IsDraftMail"] as string, out isDraftMail);

                bool isMail = false;
                if (bool.TryParse(ConfigurationManager.AppSettings["IsMail"], out isMail) && isMail)
                {
                    string WorkEnv = ConfigurationManager.AppSettings["WorkEnv"];
                    if (!string.IsNullOrEmpty(WorkEnv))
                    {
                        mail.Subject = WorkEnv + mail.Subject;
                    }
                    // 2, send mail
                    if (!isDraftMail || lb_FactSend)
                    {
                        eSenderMail(mail);
                    }

                }
                if (isDraftMail && !lb_FactSend)
                {
                    mail.Category = "Draft";
                } else
                {
                    mail.Category = "Sent";
                }

                // 3, save mail
                return SaveMail(mail, Collector);

            }
            catch (Exception e)
            {
                Helper.Log.Error(e.Message, e);
                throw new MailServiceException(e);
            }
        }

        public MailTmp SaveMailAsDraft(MailTmp mail, string Collector = "")
        {
            try
            { 
                mail.Category = "Draft";
                // 3, save mail
                return SaveMail(mail, Collector);

            }
            catch (Exception e)
            {
                Helper.Log.Error(e.Message, e);
                throw new MailServiceException(e);
            }
        }

        public void UpdateDateAfterSendMail(MailTmp mail)
        {
            // 3 Update Customer Last Send Date
            List<ContactHistory> nContractHistory = CommonRep.GetQueryable<ContactHistory>().Where(x => x.ContactType == "Mail" && x.ContactId == mail.MessageId && x.CustomerNum != null && x.SiteUseId != null).ToList();
            if (nContractHistory != null && nContractHistory.Count > 0)
            {
                nContractHistory.ForEach(x =>
                {
                    Customer nCustomer = CommonRep.GetQueryable<Customer>().Where(c => c.CustomerNum == x.CustomerNum && c.SiteUseId == x.SiteUseId).FirstOrDefault();
                    if (nCustomer != null)
                    {
                        nCustomer.LastSendDate = DateTime.Now;
                        CommonRep.Save(nCustomer);
                    }
                    CollectorAlert nAlert = CommonRep.GetQueryable<CollectorAlert>().Where(c => c.CustomerNum == x.CustomerNum && c.SiteUseId == x.SiteUseId && c.Status == "Initialized").FirstOrDefault();
                    if (nAlert != null)
                    {
                        nAlert.Status = "Finish";
                        CommonRep.Save(nAlert);
                    }
                });
                CommonRep.Commit();
            }
        }

        /// <summary>
        /// tmp ->mailmessage
        /// </summary>
        private void eSenderMail(MailTmp mail)
        {
            try
            {
                var msginfo = new MailMessageDto();
                FileService fs = SpringFactory.GetObjectImpl<FileService>("FileService");
                //mailmessage dto fuzhi
                msginfo.Body = mail.Body;
                msginfo.CC = mail.Cc;
                msginfo.From = mail.From;
                msginfo.Encoding = Encoding.UTF8.ToString();
                msginfo.MessageId = mail.MessageId;
                msginfo.Sender = mail.From;
                msginfo.Subject = mail.Subject;
                msginfo.To = mail.To;
                msginfo.IsBodyHtml = true;

                if (mail.Attachments != null && mail.Attachments.Count > 0)
                {
                    List<AppFile> appFiels = fs.GetAppFiles(mail.Attachments);
                    List<MailAttachmentDto> nAttachments = new List<MailAttachmentDto>();
                    foreach (var f in appFiels)
                    {
                        MailAttachmentDto nAttach = new MailAttachmentDto();
                        nAttach.Name = f.FileName;
                        nAttach.FileName = f.PhysicalPath;
                        nAttach.Content = ReadFileAsByte(f.PhysicalPath);
                        nAttachments.Add(nAttach);
                    }
                    msginfo.Attachs = nAttachments.ToArray();
                }
                Mailer.SendMailInfo(msginfo);
            }
            catch (Exception e)
            {
                Helper.Log.Error(e.Message, e);
                throw new MailServiceException(e);
            }
        }

        public static byte[] ReadFileAsByte(string path)
        {
            //读文件成二进制流
            FileStream stream = new FileInfo(path).OpenRead();
            var bufferLength = stream.Length;
            byte[] bufferFile = new byte[bufferLength];
            stream.Read(bufferFile, 0, Convert.ToInt32(bufferLength));

            return bufferFile;
        }
        private void innerSendMail(MailTmp mailInstance)
        {
            try
            {
                var message = new Message { Raw = getRawMessage(mailInstance) };

                Mailer.SendMailRaw(new MailRaw() { MailBox = mailInstance.From, RawMsg = message.Raw });
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw new MailServiceException(ex);
            }
        }

        public string getRawMessageWithAttachment(MailTmp mailInstance)
        {
            FileService fs = SpringFactory.GetObjectImpl<FileService>("FileService");
            var msg = new MailMessage();
            msg.Subject = mailInstance.Subject;
            if (!string.IsNullOrEmpty(mailInstance.Cc))
            {
                string[] mailcc = mailInstance.Cc.Split(';');
                foreach (var cc in mailcc)
                {
                    //Check mail address
                    if (!MailFormatCheckHelper.checkMailFormat(cc))
                    {
                        continue;
                    }
                    var dist = Helper.AddressToDistributor(cc);
                    msg.CC.Add(new MailAddress(dist.Address, Helper.GetMsgDisplayName(dist.DisplayName)));
                }
            }

            string[] mailto = mailInstance.To.Split(';');
            foreach (var mt in mailto)
            {
                //Check mail address
                if (!MailFormatCheckHelper.checkMailFormat(mt))
                {
                    continue;
                }
                var dist = Helper.AddressToDistributor(mt);
                msg.To.Add(new MailAddress(dist.Address, Helper.GetMsgDisplayName(dist.DisplayName)));
            }

            msg.From = new MailAddress(mailInstance.From, Helper.GetMsgDisplayName(mailInstance.DisplayName));
            msg.BodyEncoding = Encoding.UTF8;
            msg.Body = mailInstance.Body;

            msg.IsBodyHtml = true;

            if (mailInstance.Attachments != null && mailInstance.Attachments.Count > 0)
            {
                List<AppFile> appFiels = fs.GetAppFiles(mailInstance.Attachments);

                foreach (var f in appFiels)
                {
                    msg.Attachments.Add(new Attachment(File.OpenRead(f.PhysicalPath), new System.Net.Mime.ContentType("utf-8")));
                }
            }
          
            string newn = msg.ToString();

            // add into file table
            OTCTimerRepository TimerRep = new OTCTimerRepository() { UOW = new EFUnitOfWork() };
            fs = new FileService() { CommonRep = TimerRep };
            MemoryStream ms = new MemoryStream(newn.Length);
            StreamWriter sw = new StreamWriter(ms);
            sw.Write(newn);
            sw.Flush();
            ms.Position = 0;
            AppFile file = fs.AddAppFile(mailInstance.Subject + ".eml", ms, FileType.SentMail);
            //fileid need to save in mail table
            mailInstance.FileId = file.FileId;
            return getBaseUrlEncoded(newn);
        }

        private string getRawMessage(MailTmp mailInstance)
        {
            FileService fs = SpringFactory.GetObjectImpl<FileService>("FileService");
            var msg = new MailMessage();
            msg.Subject = mailInstance.Subject;
            if (!string.IsNullOrEmpty(mailInstance.Cc))
            {
                string[] mailcc = mailInstance.Cc.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var cc in mailcc)
                {
                    //Check mail address
                    if (!MailFormatCheckHelper.checkMailFormat(cc))
                    {
                        continue;
                    }
                    var dist = Helper.AddressToDistributor(cc);
                    msg.CC.Add(new MailAddress(dist.Address, Helper.GetMsgDisplayName(dist.DisplayName)));
                }
            }

            string[] mailto = mailInstance.To.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var mt in mailto)
            {
                //Check mail address
                if (!MailFormatCheckHelper.checkMailFormat(mt))
                {
                    continue;
                }
                var dist = Helper.AddressToDistributor(mt);
                msg.To.Add(new MailAddress(dist.Address, Helper.GetMsgDisplayName(dist.DisplayName)));
            }

            msg.From = new MailAddress(mailInstance.From, Helper.GetMsgDisplayName(mailInstance.DisplayName));
            //msg.BodyEncoding = Encoding.UTF8;
            msg.IsBodyHtml = true;
            ///
            MimeMessage mimeMsg = MimeMessage.CreateFromMailMessage(msg);
            var multipart = new Multipart("mixed");
            var body = new TextPart("html")
            {
                Text = mailInstance.Body,
                ContentTransferEncoding = ContentEncoding.Base64
            };

            multipart.Add(body);

            if (mailInstance.Attachments != null && mailInstance.Attachments.Count > 0)
            {
                List<AppFile> appFiels = fs.GetAppFiles(mailInstance.Attachments);

                foreach (var f in appFiels)
                {
                    var attachment = new MimePart("application/octet-stream");
                    attachment.ContentObject = new ContentObject(File.OpenRead(f.PhysicalPath), ContentEncoding.Default);
                    attachment.ContentDisposition = new ContentDisposition(ContentDisposition.Attachment);
                    attachment.ContentTransferEncoding = ContentEncoding.Base64;
                    attachment.FileName = f.FileName;
                    multipart.Add(attachment);
                }
            }

            mimeMsg.Body = multipart;
            string newn = mimeMsg.ToString();

            // add into file table
            OTCTimerRepository TimerRep = new OTCTimerRepository() { UOW = new EFUnitOfWork() };
            fs = new FileService() { CommonRep = TimerRep };
            MemoryStream ms = new MemoryStream(newn.Length);
            StreamWriter sw = new StreamWriter(ms);
            sw.Write(newn);
            sw.Flush();
            ms.Position = 0;
            AppFile file = fs.AddAppFile(mailInstance.Subject + ".eml", ms, FileType.SentMail);
            //fileid need to save in mail table
            mailInstance.FileId = file.FileId;

            return getBaseUrlEncoded(newn);
        }

        private string getBaseUrlEncoded(string input)
        {
            var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(inputBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }

        public void SaveOrUpdateTemplate(MailTemplate template)
        {
            template.Deal = AppContext.Current.User.Deal;
            if (template.Id > 0)
            {
                // update
                var existing = CommonRep.FindBy<MailTemplate>(template.Id);
                ObjectHelper.CopyObjectWithUnNeed(template, existing, new string[] { "Id" });
                CommonRep.Save(existing);
            }
            else
            {
                // insert
                template.CreateDate = AppContext.Current.User.Now;
                template.Creater = AppContext.Current.User.EID;
                CommonRep.Add(template);
            }
            CommonRep.Commit();
        }

        public void ProcessDealMailBoxs(string deal)
        {
            // get mail box for all collectors
            XcceleratorEntities wfDbContext = new XcceleratorEntities();
            List<SysUser> users = wfDbContext.T_USERS.Where(e => e.T_USER_EMPLOYEE.T_ORG_TEAM.TEAM_NAME == "Collection" && e.T_USER_EMPLOYEE.T_ORG_DEAL.DEAL_NAME == deal)
                                    .Select(u => new SysUser()
                                    {
                                        EID = u.USER_CODE,
                                        Name = u.T_USER_EMPLOYEE.USER_NAME,
                                        Email = u.T_USER_EMPLOYEE.USER_MAIL,
                                        Deal = u.T_USER_EMPLOYEE.T_ORG_DEAL.DEAL_NAME,
                                        DealId = u.T_USER_EMPLOYEE.DEAL_ID.ToString()
                                    }).ToList();

            Helper.Log.Info("Start to process user mail box, total: " + users.Count);

            foreach (var user in users)
            {
                if (user.Email == null)
                {
                    Helper.Log.Info(string.Format("User: {0} mail is empty", user.EID));
                    continue;
                }

                // check if the from have complete the user initialization
                if (Mailer.CheckMailBoxInitialized(user.Email))
                {
                    Helper.Log.Info(string.Format("User: {0} mail box: initialized. Start to process.", user.EID, user.Email));
                    ProcessMailBox(user.Email, deal);
                }
                else
                {
                    Helper.Log.Info(string.Format("User: {0} mail box: {1} havn't initialized, Please complete the initialization in 'User initialization' page", user.EID, user.Email));
                }
            }
        }

        /// <summary>
        /// This method will be called from Global timer
        /// </summary>
        public void ProcessMailBox(string mailBox, string deal)
        {
            OTCTimerRepository TimerRep = new OTCTimerRepository() { UOW = new EFUnitOfWork() };
            // Retrieve mail from last msg time.
            MailRaw lastMail = TimerRep.GetDbSet<MailRaw>().Where(r => r.MailBox == mailBox && r.Deal == deal).OrderByDescending(r => r.InternalTime).FirstOrDefault();
            long lastMsgTime = 0;
            if (lastMail != null)
            {
                // UTC timestamp.
                lastMsgTime = lastMail.InternalTime.Value;
            }
            else
            {
                // if we never download any mail for this mailbox, we only seek for the mail received start from today.
                // generate the milliseconds same as gmail internal date logic.
                lastMsgTime = Convert.ToInt64(((TimeSpan)(DateTime.Now.Date.AddHours((Helper.GetRegionTimeSheft(deal)) * -1) - new DateTime(1970, 1, 1, 0, 0, 0))).TotalMilliseconds);
            }


            List<MailRaw> raws = new List<MailRaw>();
            try
            {
                raws = Mailer.GetMailRaw(mailBox, lastMsgTime);

                // we should not get file service instance from spring container with "SpringFactory.GetObjectImpl<>" method. 
                // As that is the only single instance we have in our container and we should not distory its CommonRep property.
                FileService fs = new FileService() { CommonRep = TimerRep };
                raws.ForEach(r =>
                {
                    string rawstring = r.RawMsg.Replace("-", "+").Replace("_", "/");
                    string mailRaw = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(rawstring));
                    MemoryStream ms = new MemoryStream(mailRaw.Length);
                    StreamWriter sw = new StreamWriter(ms);
                    sw.Write(mailRaw);
                    sw.Flush();
                    ms.Position = 0;

                    // add into file table
                    AppFile file = fs.AddAppFile(r.MessageId + ".eml", ms, FileType.ReceivedMail);

                    // add into mail raw
                    TimerRep.GetDbSet<MailRaw>().Add(new MailRaw()
                    {
                        FileId = file.FileId,
                        MailBox = mailBox,
                        ProcessFlg = 0,
                        InternalTime = r.InternalTime,
                        MessageId = r.MessageId,
                        Deal = deal
                    });

                });

                TimerRep.Commit();

                // start the rest asyn processing
                ThreadPool.QueueUserWorkItem(processMail, new { mailBox, deal });
            }
            catch (Exception ex)
            {
                Helper.Log.Error(string.Format("Error happened while loading mail message from Gmail mailbox: [{0}]", mailBox), ex);
                // Return to let other mail box get chance to process.
                return;
            }

        }

        private void processMail(dynamic startObj)
        {
            try
            {
                OTCTimerRepository TimerRep = new OTCTimerRepository() { UOW = new EFUnitOfWork() };

                string mailBox = startObj.mailBox;
                string deal = startObj.deal;

                List<MailRaw> raws = new List<MailRaw>();

                raws = TimerRep.GetDbSet<MailRaw>().Where(mr => mr.ProcessFlg == 0 && mr.MailBox == mailBox && mr.Deal == deal).ToList();

                if (raws == null || raws.Count() == 0)
                {
                    Helper.Log.Info(string.Format("No message from Gmail mailbox: [{0}]", mailBox));
                    return;
                }

                FileService fs = new FileService() { CommonRep = TimerRep };
                raws.ForEach(raw =>
                {
                    raw.ProcessFlg = 1;
                    TimerRep.Commit();
                    try
                    {
                        // Get mail raw
                        AppFile f = fs.GetAppFile(raw.FileId);

                        // Process raw
                        MailTmp m = processMailRaw(raw, f.PhysicalPath);
                        m.MailBox = mailBox;
                        m.Deal = deal;
                        m.CreateTime = Helper.GetRegionNow(deal);
                        m.UpdateTime = m.CreateTime;

                        // process mail auto assignment
                        List<MailTmp> mailList = autoAssignMessage(m, deal, TimerRep);

                        // add new mails
                        TimerRep.GetDbSet<MailTmp>().AddRange(mailList);

                        // update information from the persor
                        raw.ProcessFlg = 2;
                        TimerRep.Commit();
                    }
                    catch (Exception ex)
                    {
                        // log mail raw error information and containue to process the next mail raw
                        Helper.Log.Error("Error happened while prcessing the raw mail in mail box:" + mailBox, ex);
                        raw.ProcessFlg = 3;
                        raw.ProcessError = ex.ToString();
                        TimerRep.Commit();
                    }
                });

            }
            catch (Exception ex)
            {
                Helper.Log.Error("processMail failed", ex);
            }
        }

        public void RetryAllMail(dynamic startObj)
        {
            OTCTimerRepository TimerRep = new OTCTimerRepository() { UOW = new EFUnitOfWork() };

            string mailBox = startObj.mailBox;
            string deal = startObj.deal;

            List<MailRaw> raws = new List<MailRaw>();

            raws = TimerRep.GetDbSet<MailRaw>().Where(mr => mr.ProcessFlg == 3 && mr.MailBox == mailBox && mr.Deal == deal).ToList();

            if (raws == null || raws.Count() == 0)
            {
                Helper.Log.Info(string.Format("No message from Gmail mailbox: [{0}]", mailBox));
                return;
            }

            FileService fs = new FileService() { CommonRep = TimerRep };
            raws.ForEach(raw =>
            {
                raw.ProcessFlg = 1;
                TimerRep.Commit();
                try
                {
                    // Get mail raw
                    AppFile f = fs.GetAppFile(raw.FileId);

                    // Process raw
                    MailTmp m = processMailRaw(raw, f.PhysicalPath);
                    m.MailBox = mailBox;
                    m.Deal = deal;
                    m.CreateTime = Helper.GetRegionNow(deal);
                    m.UpdateTime = m.CreateTime;

                    // process mail auto assignment
                    List<MailTmp> mailList = autoAssignMessage(m, deal, TimerRep);

                    // add new mails
                    TimerRep.GetDbSet<MailTmp>().AddRange(mailList);

                    // update information from the persor
                    raw.ProcessFlg = 2;
                    TimerRep.Commit();
                }
                catch (Exception ex)
                {
                    // log mail raw error information and containue to process the next mail raw
                    Helper.Log.Error("Error happened while prcessing the raw mail in mail box:" + mailBox, ex);
                    raw.ProcessFlg = 3;
                    raw.ProcessError = ex.ToString();
                    TimerRep.Commit();
                }
            });

        }

        public void RetryOneMail(string mailBox, string deal, string messageId)
        {
            OTCTimerRepository TimerRep = new OTCTimerRepository() { UOW = new EFUnitOfWork() };
            List<MailRaw> raws = new List<MailRaw>();

            raws = TimerRep.GetDbSet<MailRaw>().Where(mr => mr.MessageId == messageId).ToList();

            if (raws == null || raws.Count() == 0)
            {
                Helper.Log.Info(string.Format("No message with message id: {0}", messageId));
                return;
            }

            FileService fs = new FileService() { CommonRep = TimerRep };
            raws.ForEach(raw =>
            {
                raw.ProcessFlg = 1;
                TimerRep.Commit();
                try
                {
                    // Get mail raw
                    AppFile f = fs.GetAppFile(raw.FileId);

                    // Process raw
                    MailTmp m = processMailRaw(raw, f.PhysicalPath);
                    m.MailBox = mailBox;
                    m.Deal = deal;
                    m.CreateTime = Helper.GetRegionNow(deal);
                    m.UpdateTime = m.CreateTime;

                    // process mail auto assignment
                    List<MailTmp> mailList = autoAssignMessage(m, deal, TimerRep);

                    // add new mails
                    TimerRep.GetDbSet<MailTmp>().AddRange(mailList);

                    // update information from the persor
                    raw.ProcessFlg = 2;
                    TimerRep.Commit();
                }
                catch (Exception ex)
                {
                    // log mail raw error information and containue to process the next mail raw
                    Helper.Log.Error("Error happened while prcessing the raw mail in mail box:" + mailBox, ex);
                    raw.ProcessFlg = 3;
                    raw.ProcessError = ex.ToString();
                    TimerRep.Commit();
                }
            });
        }

        public MailTmp processMailRaw(MailRaw raw, string rawPhysicalPath)
        {
            // load message
            var rawStr = File.ReadAllText(rawPhysicalPath);

            return ProcessMailRaw(raw, rawStr);
        }

        public MailTmp ProcessMailRaw(MailRaw raw, string mailRaw)
        {
            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms);
            sw.Write(mailRaw);
            sw.Flush();
            ms.Position = 0;
            MimeMessage mimeMsg = null;

            try
            {
                mimeMsg = MimeMessage.Load(ms);
            }
            catch (Exception ex)
            {
                Helper.Log.Error("Failed to load message stream into MimeMessage", ex);

                // trying to read message subject and add a mail record with minimum information.
                Helper.Log.Info("trying to read message subject and add a mail record with minimum information.");
                string subject = string.Empty;
                var subIdx = mailRaw.IndexOf("Subject", StringComparison.OrdinalIgnoreCase);
                if (subIdx > 0)
                {
                    subIdx = subIdx + 8;
                    var subEndIdx = mailRaw.IndexOf("\r\n", subIdx);
                    subject = mailRaw.Substring(subIdx, subEndIdx - subIdx);
                }

                if (!string.IsNullOrEmpty(subject))
                {
                    MailTmp min = new MailTmp();
                    min.Subject = subject;
                    min.Attachment = string.Empty;
                    min.InternalTime = raw.InternalTime;
                    min.InternalDatetime = Helper.GetDatetimeFromMailInternalTime(min.InternalTime.Value);
                    min.MessageId = raw.MessageId;
                    min.Category = "CustomerNew";
                    min.Operator = "System";
                    min.Type = "IN";
                    return min;
                }
                else
                {
                    throw;
                }
            }

            sw.Close();

            Helper.Log.Info("Msg: " + raw.MessageId + " created");

            // Parse mail instance from mimeMsg
            MailTmp m = new MailTmp();
            m.From = mimeMsg.From.ToString();
            m.To = mimeMsg.To.ToString();
            m.Cc = mimeMsg.Cc.ToString();
            m.Subject = mimeMsg.Subject;
            if (!string.IsNullOrEmpty(mimeMsg.HtmlBody))
            {
                m.Body = mimeMsg.HtmlBody.ToString();
                m.BodyFormat = "HTML";
            }
            else
            {
                m.Body = mimeMsg.TextBody;
                m.BodyFormat = "TXT";
            }
            m.Attachment = string.Empty;
            try
            {
                processAttachments(mimeMsg, raw.MessageId, fileId =>
                {
                    m.Attachment += (fileId + ",");
                });
                processBodyParts(mimeMsg, (contentId, fileId) =>
                {
                    int inx = m.Body.IndexOf("cid:" + contentId);
                    if (inx >= 0)
                    {
                        // build the file api used for front end to reference the file resource.
                        string fileRetrivalApi = string.Format("{0}/api/appFiles?fileId={1}", ConfigurationManager.AppSettings["OTC"], fileId);

                        // replace content id reference with file api reference. 
                        m.Body = m.Body.Replace("cid:" + contentId, fileRetrivalApi);
                    }
                    else
                    {
                        Helper.Log.Info("Not found the reference to embaded content: " + "cid:" + contentId);
                    }
                });
            }
            catch (Exception ex)
            {
                Helper.Log.Error("Failed to load attachments/bodyparts from message stream.", ex);
            }
            m.InternalTime = raw.InternalTime;
            m.InternalDatetime = Helper.GetDatetimeFromMailInternalTime(m.InternalTime.Value);
            m.MessageId = raw.MessageId;
            m.Category = "CustomerNew";
            m.Operator = "System";
            m.Type = "IN";

            return m;
        }

        private void processBodyParts(MimeMessage mimeMsg, Action<string, string> replaceBodyDlg)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            OTCTimerRepository TimerRep = new OTCTimerRepository() { UOW = new EFUnitOfWork() };

            FileService fs = new FileService() { CommonRep = TimerRep };

            foreach (var item in mimeMsg.BodyParts)
            {
                // Recognize the unusual content type.
                if (item.ContentType.MediaType.ToUpper() == "TEXT"
                    || item.IsAttachment)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(item.ContentId))
                {
                    // Only save the content with content Id, As the content Id is unique reference which is usually referenced by text parts.
                    continue;
                }

                string contentId = item.ContentId;
                string name = item.ContentId;

                MimePart part = item as MimePart;
                if (part != null)
                {
                    StreamReader sr = new StreamReader(part.ContentObject.Stream);
                    var partStream = sr.ReadToEnd();
                    var baseArr = Convert.FromBase64String(partStream);

                    MemoryStream tmp = new MemoryStream();
                    tmp.Write(baseArr, 0, baseArr.Length);
                    tmp.Flush();
                    tmp.Position = 0;

                    var file = fs.AddAppFile(name, tmp, FileType.MailBodyPart, contentId: contentId, contentType: part.ContentType.MimeType);

                    replaceBodyDlg(contentId, file.FileId);
                }
            }
        }

        public static List<MailTmp> autoAssignMessage(MailTmp m, string deal, OTCTimerRepository timerRep)
        {
            List<MailTmp> mailList = new List<MailTmp>();
            List<CustomerMail> custMailList = new List<CustomerMail>();
            string fromAddress = Helper.AddressToDistributor(m.From).Address;
            Helper.Log.Info("Trying to assign the mail to customers. From: " + fromAddress);

            XcceleratorEntities wfDbContext = new XcceleratorEntities();
            Helper.Log.Info("MailBox: " + m.MailBox);
            var empId = (from u in wfDbContext.T_USER_EMPLOYEE
                         where u.USER_MAIL == m.MailBox
                         select u.ID).FirstOrDefault();

            Helper.Log.Info("empId: " + empId);

            var user = (from u in wfDbContext.T_USERS
                        where u.USER_EMPLOYEE_ID == empId
                        select u.USER_CODE).FirstOrDefault();


            Helper.Log.Info("mailBoxcollectorEid: " + user);

            //1.1,get the customer by fromAddress
            //########### update by pxc 20160331 check customer Contactor && Group Contactor ############ s
            //@@@@@ get customer contactors union get group contactors
            var customerNums = ((from con in timerRep.GetDbSet<Contactor>()
                                 where con.EmailAddress == fromAddress && con.Deal == deal
                                 && (con.GroupCode == "" || con.GroupCode == null)
                                 select con.CustomerNum)
                                .Union(
                                from con in timerRep.GetDbSet<Contactor>().Where(c => c.EmailAddress == fromAddress && c.Deal == deal
                                && (c.GroupCode != "" && c.GroupCode != null))
                                join grp in timerRep.GetDbSet<Customer>().Where(g => g.IsActive == true && g.ExcludeFlg == "0" && g.Deal == deal) on new { BillGroupCode = con.GroupCode } equals new { grp.BillGroupCode }
                                select grp.CustomerNum)
                                ).Distinct();

            Helper.Log.Info("customerNumCount: " + customerNums.Count());

            //########### update by pxc 20160331 check customer Contactor && Group Contactor ############ e

            CustomerMail custMail = new CustomerMail();

            if (customerNums == null || customerNums.Count() == 0)
            {
                int atIndex = fromAddress.IndexOf("@");
                string mailDomain = fromAddress.Remove(0, atIndex + 1);
                //1.2,get the customerNum from Domain by fromAdd
                customerNums = (from con in timerRep.GetDbSet<ContactorDomain>()
                                where con.MailDomain == mailDomain && con.Deal == deal
                                select con.CustomerNum).Distinct();

                Helper.Log.Info("[Domain]customerNumCount: " + customerNums.Count());
            }

            //INSERT DATE TO CUSTOMER_MAIL
            if (customerNums != null && customerNums.Count() > 0)
            {
                //2.get the customers that collector eid is mailbox eid
                var CollectorCustomerNums = (from cus in timerRep.GetDbSet<Customer>()
                                             where customerNums.Contains(cus.CustomerNum) && cus.Collector == user
                                             select cus.CustomerNum).Distinct();
                Helper.Log.Info("outLoopCollectorMapCustomerCount:" + CollectorCustomerNums.Count());

                //INSERT DATA TO  T_CUSTOMER_MAIL
                if (CollectorCustomerNums != null && CollectorCustomerNums.Count() > 0)
                {
                    foreach (var custNum in CollectorCustomerNums)
                    {
                        Helper.Log.Info("[matched]custNum:" + custNum);
                        custMail = new CustomerMail();
                        custMail.MessageId = m.MessageId;
                        custMail.CustomerNum = custNum.ToString();
                        custMailList.Add(custMail);
                        Helper.Log.Info("[matched]custMailListCount:" + custMailList.Count());
                    }
                }
            }

            //PARENT TABLE T_MAIL_TMP
            MailTmp copiedMail = new MailTmp();
            ObjectHelper.CopyObject(m, copiedMail);
            copiedMail.Category = "CustomerNew";
            Helper.Log.Info("copiedMail:" + copiedMail.MessageId);
            //CHILD TABLE T_CUSTOMER_MAIL
            copiedMail.CustomerMails = custMailList;
            Helper.Log.Info("custMailListCount:" + custMailList.Count());
            mailList.Add(copiedMail);
            Helper.Log.Info("mailListCount:" + mailList.Count());
            return mailList;
        }


        private string processAttachments(MimeMessage mimeMsg, string msgId, Action<string> attachmentCallBack)
        {
            OTCTimerRepository TimerRep = new OTCTimerRepository() { UOW = new EFUnitOfWork() };

            string attachmentFilesIds = string.Empty;
            string messageDes = string.Empty;
            Helper.Log.Info("foreach (MimeEntity item in mimeMsg.Attachments)");
            foreach (MimeEntity item in mimeMsg.Attachments)
            {
                MemoryStream attStream = null;
                Stream tmp = null;
                try
                {
                    string name = string.Empty;
                    if (item.ContentType.MediaType.ToUpper() == "MESSAGE"
                        && item.ContentType.MediaSubtype.ToUpper() == "RFC822")
                    {
                        //continue;
                        tmp = new MemoryStream();
                        if ((item as MimeKit.MessagePart).Message != null)
                        {
                            (item as MimeKit.MessagePart).Message.WriteTo(tmp);
                            tmp.Flush();
                            tmp.Position = 0;
                            name = (item as MimeKit.MessagePart).Message.Subject + ".eml";
                        }

                        attStream = tmp as MemoryStream;
                    }
                    else
                    {
                        MimePart part = item as MimePart;
                        if (part != null)
                        {
                            messageDes = string.Format("Mail message:{0} attachments: {1}, Size: {2}", msgId, part.ContentDisposition.FileName, part.ContentDisposition.Size);

                            tmp = part.ContentObject.Stream;
                            name = part.FileName;

                            StreamReader sr = new StreamReader(tmp);
                            var partStream = sr.ReadToEnd();
                            if (part.ContentObject.Encoding == ContentEncoding.Base64)
                            {
                                var baseArr = Convert.FromBase64String(partStream);
                                attStream = new MemoryStream();
                                attStream.Write(baseArr, 0, baseArr.Length);
                                attStream.Flush();
                                attStream.Position = 0;
                            }
                            else
                            {
                                if (part.ContentObject.Encoding == ContentEncoding.QuotedPrintable)
                                {
                                    //TODO: special decoder is needed. see detail: http://stackoverflow.com/questions/2226554/c-class-for-decoding-quoted-printable-encoding
                                }
                                attStream = new MemoryStream();
                                StreamWriter sw = new StreamWriter(attStream);
                                sw.Write(partStream);
                                sw.Flush();
                                attStream.Position = 0;
                            }
                        }
                    }

                    FileService fs = new FileService() { CommonRep = TimerRep };

                    var file = fs.AddAppFile(name, attStream, FileType.MailAttachment);

                    attachmentCallBack(file.FileId);
                }
                catch (Exception ex)
                {
                    Helper.Log.Error("Error happened while processing the attachment. ", ex);
                    Helper.Log.Info(messageDes);
                    throw;
                }
                finally
                {
                    attStream.Close();
                    tmp.Close();
                }
            }

            if (!string.IsNullOrEmpty(attachmentFilesIds))
            {
                attachmentFilesIds = attachmentFilesIds.TrimEnd(',');
            }

            return attachmentFilesIds;
        }

        protected class TemplateParser : ITemplateParser
        {
            Dictionary<string, object> objectRegister = new Dictionary<string, object>();

            public void RegistContext(string objkey, object contextObj)
            {
                if (!objectRegister.ContainsKey(objkey))
                {
                    objectRegister.Add(objkey, contextObj);
                }
            }

            public void ParseTemplate(string template, out string templateInstance)
            {
                // gether tokens within template
                IBaseDataService bdService = SpringFactory.GetObjectImpl<IBaseDataService>("BaseDataService");
                List<SysTypeDetail> tokens = bdService.GetSysTypeDetail("011").ToList();

                string regexFormat = @"~[\w]+~";
                MatchCollection matches = Regex.Matches(template, regexFormat);

                templateInstance = template;
                int matchIdx = 0;
                foreach (Match m in matches)
                {
                    templateInstance = templateInstance.Replace(m.Value, "{" + matchIdx + "}");
                    matchIdx++;
                }

                List<string> res = new List<string>();
                foreach (Match m in matches)
                {
                    string token = m.Value.Trim('~');
                    SysTypeDetail tokenDetail = tokens.Find(t => t.DetailName == token);
                    if (tokenDetail == null)
                    {
                        throw new OTCServiceException(string.Format("Token [{0}] cannot be parse to system invocation because there is no matching defination", tokenDetail.DetailName));
                    }

                    string tokenValue = tokenDetail.DetailValue;
                    tokenValue = string.IsNullOrEmpty(tokenValue) ? "" : tokenValue;
                    if (tokenValue != null)
                    {
                        //Helper.Log.Info(string.Format("Token value [{0}] retrieved for token [{1}]", tokenValue, token));

                        // reflect method/property call to mail service
                        string resValue = string.Empty;
                        object resObj = new object();
                        if (objectRegister.TryGetValue(tokenValue, out resObj))
                        {
                            if (!string.IsNullOrEmpty(tokenDetail.DetailValue2))
                            {
                                // If resObj is null. which means no obj to evaluate on. The expression can work on itself.
                                resValue = ExpressionEvaluator.GetValue(resObj, tokenDetail.DetailValue2) as string;
                            }
                        }

                        res.Add(resValue);
                    }
                }

                // fill into template instance
                templateInstance = string.Format(templateInstance, res.ToArray());
            }

            public object GetContext(string objkey)
            {
                if (objectRegister.ContainsKey(objkey))
                {
                    object res = null;
                    if (objectRegister.TryGetValue(objkey, out res))
                    {
                        return res;
                    }
                    else
                    {
                        return null;
                    }
                }

                return null;
            }
        }

        public MailTmp GetMailInstance(int templateId)
        {
            return GetMailInstance(templateId, parser => { });
        }
        public MailTmp GetMailInstance(int templateId, Action<ITemplateParser> registContextDlg)
        {
            AssertUtils.IsTrue(templateId > 0);

            MailTemplate template = GetMailTemplateById(templateId);

            if (template == null)
            {
                throw new OTCServiceException("Template does not exist for id: [" + templateId + "]");
            }

            MailTmp mailInstance = GetInstanceFromTemplate(template, (parser) =>
            {
                parser.RegistContext("collector", AppContext.Current.User);

                if (registContextDlg != null)
                {
                    registContextDlg(parser);
                }
            });

            return mailInstance;
        }
        public MailTmp GetMailInstancebyCusnum(string type, string language)
        {
            return GetMailInstancebyCusnum(type, language, parser => { });
        }

        public MailTmp GetMailInstancebyCusnum(string type, string language, Action<ITemplateParser> registContextDlg)
        {
            MailTemplate template = GetMailTemplatebytype(type, language);

            if (template == null)
            {
                Exception ex = new OTCServiceException("Template does not exist");
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }

            MailTmp mailInstance = GetInstanceFromTemplate(template, (parser) =>
            {
                parser.RegistContext("collector", AppContext.Current.User);

                if (registContextDlg != null)
                {
                    registContextDlg(parser);
                }
            });

            return mailInstance;
        }

        public MailTmp GetMailInstance(MailTemplateType type, Action<ITemplateParser> registContextDlg)
        {
            MailTemplate template = GetMailTemplate(Helper.EnumToCode<MailTemplateType>(type));

            if (template == null)
            {
                Exception ex = new OTCServiceException("Template does not exist for [" + type + "] type of mail.");
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }

            MailTmp mailInstance = GetInstanceFromTemplate(template, (parser) =>
            {
                parser.RegistContext("collector", AppContext.Current.User);

                if (registContextDlg != null)
                {
                    registContextDlg(parser);
                }
            });

            return mailInstance;
        }

        /// <summary>
        /// MailDetail Use ,choose One mail record accroding Table Id to get this maildetail
        /// </summary>
        /// <param name="orginalMailId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public MailTmp GetMailInstance(int orginalMailId, string type)
        {
            MailTmp orginalMail = (from m in CommonRep.GetQueryable<MailTmp>().Include<MailTmp, ICollection<CustomerMail>>(m => m.CustomerMails)
                                   where m.Id == orginalMailId
                                   select m).FirstOrDefault();

            switch (type)
            {
                case "RE":
                case "FW":
                    if (orginalMail == null)
                    {
                        throw new OTCServiceException("Orginal mail cannot be found for mail id: " + orginalMailId);
                    }

                    return getRepAndFwMail(orginalMail, type);
                case "NE":
                    return getNewMail();
                case "VI":
                    if (orginalMail == null)
                    {
                        throw new OTCServiceException("Orginal mail cannot be found for mail id: " + orginalMailId);
                    }

                    return orginalMail;
                default:
                    if (orginalMail == null)
                    {
                        throw new OTCServiceException("Orginal mail cannot be found for mail id: " + orginalMailId);
                    }

                    throw new OTCServiceException(string.Format("Not supported mail type: [{0}]!", type));
            }
        }

        //add by zhangYu customerContact REPLY/FORWARD/VIEW
        /// <summary>
        /// Get the Mail Instance (Replay/forward)
        /// </summary>
        /// <param name="id">ID of T_mail</param>
        /// <param name="type">RE:eplay / FW:Forward /VI:View</param>
        /// <returns></returns>
        private MailTmp getRepAndFwMail(MailTmp orginalMail, string type)
        {
            string orginalMailBody = getOrginalMailBody(orginalMail);

            if (type == "RE")
            {
                MailTmp newMail = GetMailInstance(MailTemplateType.Reply, (parser) =>
                {
                    parser.RegistContext("body", orginalMailBody);
                });
                newMail.Deal = AppContext.Current.User.Deal;
                newMail.Attachment = orginalMail.Attachment;
                newMail.Subject = "RE:" + orginalMail.Subject;
                newMail.To = orginalMail.From;
                newMail.Cc = orginalMail.Cc;
                newMail.CustomerMails = new List<CustomerMail>();
                foreach (var cm in orginalMail.CustomerMails)
                {
                    CustomerMail newCM = new CustomerMail()
                    {
                        CustomerNum = cm.CustomerNum,
                        MessageId = cm.MessageId,
                        SiteUseId = cm.SiteUseId
                    };

                    newMail.CustomerMails.Add(newCM);
                }
                return newMail;
            }

            if (type == "FW")
            {
                MailTmp newMail = GetMailInstance(MailTemplateType.Foward, (parser) =>
                {
                    parser.RegistContext("body", orginalMailBody);
                });
                newMail.Subject = "FW:" + orginalMail.Subject;
                newMail.Attachment = orginalMail.Attachment;
                newMail.CustomerMails = new List<CustomerMail>();
                foreach (var cm in orginalMail.CustomerMails)
                {
                    CustomerMail newCM = new CustomerMail()
                    {
                        CustomerNum = cm.CustomerNum,
                        MessageId = cm.MessageId,
                        SiteUseId = cm.SiteUseId
                    };

                    newMail.CustomerMails.Add(newCM);
                }
                return newMail;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="language"></param>
        /// <param name="type"></param>
        /// <param name="collectorEID"></param>
        /// <param name="templateId"></param>
        /// <returns></returns>
        public MailTmp GetMailInstance(string customerNums,string siteUseIds, int templateId = 0)
        {
            MailTmp res = null;

            // 2, retrieve template based on customer information and hint.
            IMailService ms = SpringFactory.GetObjectImpl<IMailService>("MailService");
            MailTemplate tpl = null;
            if (templateId > 0)
            {
                tpl = ms.GetMailTemplateById(templateId);
            }
            else
            {
                tpl = ms.GetMailTemplate(Helper.EnumToCode<MailTemplateType>(MailTemplateType.Confirm_PTP));
            }

            if (tpl != null)
            {
                res = ms.GetInstanceFromTemplate(tpl, (parser) =>
                {
                    // 1, contactNames used in SOA template
                    ContactService cs = SpringFactory.GetObjectImpl<ContactService>("ContactService");
                    IList<Contactor> contactors = cs.GetContactsByCustomers(customerNums, siteUseIds);
                    string contactNames = string.Empty;
                    foreach (Contactor cont in contactors)
                    {
                        contactNames += (cont.Name + ", ");
                    }
                    contactNames = contactNames.TrimEnd(',');

                    parser.RegistContext("contactNames", contactNames);

                    // 2, collector
                    parser.RegistContext("collector", AppContext.Current.User);
                });

                //Add by zhangYu
                res.From = GetSenderMailAddress();
            }
            else
            {
                Exception ex = new OTCServiceException("No matching template was found!", System.Net.HttpStatusCode.NotFound);
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }

            return res;
        }

        /// <summary>
        /// Return a new create mail used to send out.
        /// </summary>
        /// <returns></returns>
        private MailTmp getNewMail()
        {
            var tmp = GetMailInstance(MailTemplateType.New, null);

            return tmp;
        }

        private static string getOrginalMailBody(MailTmp oldMail)
        {
            string oldMailBody = string.Empty;


            if (oldMail.BodyFormat != null && oldMail.BodyFormat.Trim().ToUpper() == "HTML")
            {
                // trim off "body"
                int start = oldMail.Body.IndexOf("<body>");
                int end = oldMail.Body.IndexOf("</body>");
                if (start > 0 && end > 0)
                {
                    oldMailBody = oldMail.Body.Substring(start + 6, end - (start + 6));
                }
                else
                {
                    oldMailBody = oldMail.Body;
                }
            }
            else if (oldMail.BodyFormat != null && oldMail.BodyFormat.Trim().ToUpper() == "TXT")
            {
                oldMailBody = oldMail.Body.Replace("\r\n", "<br/>");
            }
            else
            {
                oldMailBody = oldMail.Body;
            }

            return oldMailBody;
        }

        //added by zhangYu genarateSoa sendMail
        public void BulkSaveMail(List<MailTmp> mailInstance)
        {
            (CommonRep.GetDBContext() as OTCEntities).BulkInsert(mailInstance);
        }

        /// <summary>
        /// Mail can be update and save in any status no mater its type is OUT or IN.
        /// </summary>
        /// <param name="mailInstance"></param>
        public MailTmp SaveMail(MailTmp mailInstance, string collector = "")
        {

            AssertUtils.ArgumentNotNull(mailInstance, "Mail Instance");
            if (string.IsNullOrWhiteSpace(mailInstance.MessageId))
            {
                mailInstance.MessageId = Guid.NewGuid().ToString();
            }

            if (mailInstance.Id == 0)
            {
                mailInstance.Deal = AppContext.Current.User == null ? "" :  AppContext.Current.User.Deal;
                mailInstance.Operator = AppContext.Current.User == null ? mailInstance.Operator : AppContext.Current.User.EID;
                mailInstance.Collector = string.IsNullOrEmpty(collector) ? mailInstance.Operator : collector;
                mailInstance.MailBox = GetSenderMailAddressByOperator(mailInstance.Operator);
                mailInstance.CreateTime = AppContext.Current.User == null ? DateTime.Now : AppContext.Current.User.Now;
                mailInstance.UpdateTime = AppContext.Current.User == null ? DateTime.Now : AppContext.Current.User.Now;

                CommonRep.Add<MailTmp>(mailInstance);
                CommonRep.Commit();
            }
            else
            {

                var existing = CommonRep.GetDbSet<CustomerMail>().Where(cm => cm.MessageId == mailInstance.MessageId);
                CommonRep.GetDbSet<CustomerMail>().RemoveRange(existing.AsEnumerable());
                CommonRep.Commit();

                MailTmp m = CommonRep.GetDbSet<MailTmp>().Where(t => t.Id == mailInstance.Id).Include<MailTmp, ICollection<CustomerMail>>(t => t.CustomerMails).FirstOrDefault();
                m.UpdateTime = AppContext.Current.User == null ? DateTime.Now : AppContext.Current.User.Now;
                ObjectHelper.CopyObjectWithUnNeed(mailInstance, m);
                CommonRep.Commit();

                mailInstance = m;
            }

            Helper.Log.Info("************************** SaveMail end ***********************");
            return mailInstance;
        }

        public MailTmp GetMailByMessageId(string messageId)
        {
            MailTmp ma = new MailTmp();
            string bodyFormat = "";

            ma = CommonRep.GetQueryable<MailTmp>().Where(m => m.MessageId == messageId).FirstOrDefault();
            //Body 
            if (ma.BodyFormat != null)
            {
                bodyFormat = ma.BodyFormat.Trim();
                if (bodyFormat.ToUpper() == "HTML")
                {
                    int start = ma.Body.IndexOf("<body");
                    int index = ma.Body.IndexOf(">", start);
                    ma.Body = ma.Body.Insert(index + 1, "<br/><br/><hr width=100% size=1>");
                }
                else if (bodyFormat.ToUpper() == "TXT")
                {

                    ma.Body = "<br/><br/>" +
                                "<hr width=100% size=1>" +
                                "<br/>" + ma.Body.Replace("\r\n", "<br/>");

                }
                else
                {
                    //ma.Body
                }
            }

            return ma;
        }

        /// <summary>
        /// Update Selected Mail Status To Processed
        /// </summary>
        /// <param name="id"></param>
        public void UpdateMailCategory(List<int> mailIds, string category)
        {
            int[] ids = mailIds.ToArray();
            var UpdateMailSql = string.Format("");

            UpdateMailSql = string.Format(@"
                    UPDATE T_MAIL_TMP SET CATEGORY = '{1}' WHERE ID IN ({0});
                ", string.Join(",", ids), category);

            // update category
            CommonRep.GetDBContext().Database.ExecuteSqlCommand(UpdateMailSql);

            if (category == "Processed")
            {
                List<MailTmp> mails = CommonRep.GetQueryable<MailTmp>().Where(m => mailIds.Contains(m.Id)).ToList();
                if (mails.Count > 0)
                {
                    mails.ForEach(m =>
                    {
                        try
                        {
                            this.MarkAsRead(m.MessageId);
                        }
                        catch (Exception ex)
                        {
                            Helper.Log.Error("MarkAsRead run failed.", ex);
                        }
                    });
                }
            }
            else if (category == "Unknow")
            {
                List<MailTmp> mails = CommonRep.GetQueryable<MailTmp>().Where(m => mailIds.Contains(m.Id)).ToList();
                if (mails.Count > 0)
                {
                    foreach (var imail in mails)
                    {
                        List<string> mailMsgIds = mails.Select(x => x.MessageId).ToList();
                        bool hasCust = CommonRep.GetQueryable<CustomerMail>().Where(x => x.MessageId == imail.MessageId).Count() > 0;
                        if (hasCust == true)
                            imail.Category = "CustomerNew";
                    }
                    CommonRep.Commit();
                }
            }

        }

        /// <summary>
        /// assign customer and assigned_flag
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cusNum"></param>
        public void UpdateMailReferenceAndAssignedFlg(string id, string cusNum, string siteUseId)
        {
            if (string.IsNullOrEmpty(cusNum) || string.IsNullOrEmpty(siteUseId))
                return;

            int nCmCnt = CommonRep.GetQueryable<CustomerMail>().Where(x => x.MessageId == id && x.CustomerNum == cusNum && x.SiteUseId == siteUseId).Count();

            if (nCmCnt > 0)
                return;

            CustomerMail cm = new CustomerMail();
            cm.MessageId = id;
            cm.CustomerNum = cusNum;
            cm.SiteUseId = siteUseId;

            CommonRep.Add(cm);

            MailTmp mailTmp = CommonRep.GetQueryable<MailTmp>().Where(x => x.MessageId == id).FirstOrDefault();

            if (mailTmp.Category == "Unknow")
                mailTmp.Category = "CustomerNew";

            // CommonRep.AddRange(cmList);
            CommonRep.Commit();

        }

        /// <summary>
        /// added by zhangYU
        /// </summary>
        /// <param name="id"></param>
        private void deleteMail(int id)
        {
            deleteMails(new List<int>() { id });
        }

        private void deleteMails(List<int> mailIds)
        {
            var mails = CommonRep.GetQueryable<MailTmp>().Where(m => mailIds.Contains(m.Id));

            CommonRep.RemoveRange(mails);
            CommonRep.Commit();
        }


        /// <summary>
        /// added by alex
        /// Delete All Selected Mails
        /// </summary>
        /// <param name="id"></param>
        public void DeleteSelectedMail(List<int> mailIds)
        {
            int id = 0;                             //邮件id
            int count = 0;                          //count数
            string fileId = "";                     //文件id
            string[] lstAttachment;                 //附件id列表

            try
            {
                //循环mail id 进行删除
                for (int i = 0; i < mailIds.Count; i++)
                {
                    //mail id 取得
                    id = mailIds[i];
                    //mail 取得
                    MailTmp mail = CommonRep.GetQueryable<MailTmp>().Where(m => m.Id == id).FirstOrDefault();
                    if(mail.Category.ToLower() != "draft") { 
                        //mail自动生成的message id不为空
                        if (!string.IsNullOrEmpty(mail.MessageId))
                        {
                            //contact history取得相关mail的条数
                            count = CommonRep.GetQueryable<ContactHistory>().Where(c => c.ContactId == mail.MessageId).Count();
                            if (count > 0)
                            {
                                //有相关的数据继续下一个循环
                                continue;
                            }
                            //invoice log取得相关mail的条数
                            count = CommonRep.GetQueryable<InvoiceLog>().Where(o => o.ProofId == mail.MessageId).Count();
                            if (count > 0)
                            {
                                //有相关的数据继续下一个循环
                                continue;
                            }
                            //dispute history取得相关mail的条数
                            count = CommonRep.GetQueryable<DisputeHis>().Where(d => d.EmailId == mail.MessageId).Count();
                            if (count > 0)
                            {
                                //有相关的数据继续下一个循环
                                continue;
                            }
                        }
                    }

                    //文件id取得
                    fileId = mail.FileId;
                    FileService fileService = SpringFactory.GetObjectImpl<FileService>("FileService");

                    //file id 是否为空判断
                    if (string.IsNullOrEmpty(fileId))
                    {
                        //关联 T_MAIL_RAW 表取 file id
                        MailRaw mailRow = CommonRep.GetQueryable<MailRaw>().Where(m => m.MessageId == mail.MessageId).FirstOrDefault();
                        if (mailRow != null)
                        {
                            fileId = mailRow.FileId;
                        }
                    }

                    if (!string.IsNullOrEmpty(fileId))
                    {
                        //删除邮件中包含的文件
                        try
                        {
                            fileService.DeleteAppFile(fileId);
                            AppFile appFile = CommonRep.GetQueryable<AppFile>().Where(o => o.FileId == fileId).FirstOrDefault();
                            CommonRep.Remove(appFile);
                        }
                        catch (Exception e)
                        {
                            Helper.Log.Error("Delete All Selected Mail:", e);
                        }
                    }

                    //附件id取得
                    if (!string.IsNullOrEmpty(mail.Attachment))
                    {
                        //有附件的场合
                        lstAttachment = mail.Attachment.Split(',');
                        //循环删除附件
                        for (var j = 0; j < lstAttachment.Length; j++)
                        {
                            //删除邮件中包含的所有附件
                            try
                            {
                                fileService.DeleteAppFile(lstAttachment[j]);
                                AppFile appFile = CommonRep.GetQueryable<AppFile>().Where(o => o.FileId == lstAttachment[j]).FirstOrDefault();
                                CommonRep.Remove(appFile);
                            }
                            catch (Exception e)
                            {
                                Helper.Log.Error("Delete All Selected Mail:", e);
                            }

                        }
                    }

                    //以上都没有相关数据的进行mail删除
                    // The customer mails are deleted cascade.
                    CommonRep.Remove(mail);
                    CommonRep.Commit();
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// Get all mails(personal & customer related mails) for all user
        /// </summary>
        /// <returns></returns>
        public IEnumerable<MailDto> GetMailList(string customerNum, string customerName)
        {
            string nMailAddress = GetSenderMailAddress();
            IEnumerable<MailDto> mails = null;
            if (string.IsNullOrWhiteSpace(customerNum) && string.IsNullOrWhiteSpace(customerName))
            {
                #region without customer related query condition
                mails = (from o in CommonRep.GetQueryable<MailTmp>()
                         join cm in CommonRep.GetQueryable<CustomerMail>() on o.MessageId equals cm.MessageId into MailCustomer
                         from mailcustomer2 in MailCustomer.DefaultIfEmpty()
                         join c in CommonRep.GetQueryable<Customer>() on new { CustomerNum = mailcustomer2.CustomerNum, SiteUseId = mailcustomer2.SiteUseId } equals new { CustomerNum = c.CustomerNum, SiteUseId = c.SiteUseId }
                         into temp
                         from tt in temp.DefaultIfEmpty()
                         where o.Deal == AppContext.Current.User.Deal
                          && o.MailBox == nMailAddress

                         select new MailDto
                         {
                             From = o.From,
                             To = o.To,
                             Cc = o.Cc,
                             Subject = o.Subject,
                             Id = o.Id,
                             CreateTime = o.CreateTime,
                             UpdateTime = o.UpdateTime,
                             InternalDatetime = o.InternalDatetime,
                             Category = o.Category,
                             Type = o.Type,
                             MessageId = o.MessageId,
                             FileId = o.FileId,
                             CustomerMails = o.CustomerMails,
                             MailTime = o.Type == "IN" ? o.InternalDatetime : o.CreateTime,
                             MailBox = o.MailBox,
                             CustomerName = tt.CustomerName,
                             CustomerNum = tt.CustomerNum,
                             SiteUseId = tt.SiteUseId,
                             Body = o.Body,
                         }).OrderByDescending(m => m.CreateTime);
                #endregion
            }
            else
            {
                List<string> custs = new List<string>();
                if (!string.IsNullOrEmpty(customerNum))
                {
                    custs = customerNum.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                if (customerName == null)
                {
                    customerName = string.Empty;
                }
                CustomerService cs = SpringFactory.GetObjectImpl<CustomerService>("CustomerService");
                mails = (from o in CommonRep.GetQueryable<MailTmp>()
                         join cm in CommonRep.GetQueryable<CustomerMail>() on o.MessageId equals cm.MessageId into MailCustomer
                         from mailcustomer2 in MailCustomer.DefaultIfEmpty()
                         join c in CommonRep.GetQueryable<Customer>() on new { CustomerNum = mailcustomer2.CustomerNum, SiteUseId = mailcustomer2.SiteUseId } equals new { CustomerNum = c.CustomerNum, SiteUseId = c.SiteUseId }
                         into temp
                         from tt in temp.DefaultIfEmpty()
                         where o.Deal == AppContext.Current.User.Deal
                          && o.MailBox == nMailAddress
                         from oc in o.CustomerMails
                         join c in cs.GetCustomerMasterForCurrentUser() on new { CustomerNum = oc.CustomerNum, Deal = o.Deal, SiteUseId = oc.SiteUseId } equals new { CustomerNum = c.CustomerNum, Deal = c.Deal, SiteUseId = c.siteUseId }
                             into grps
                         where grps.Any(g =>
                             (custs.Contains(g.CustomerNum) || string.IsNullOrEmpty(customerNum))
                             && (g.CustomerName.IndexOf(customerName) >= 0 || string.IsNullOrEmpty(customerName)))
                         select new MailDto
                         {
                             From = o.From,
                             To = o.To,
                             Cc = o.Cc,
                             Subject = o.Subject,
                             Id = o.Id,
                             CreateTime = o.CreateTime,
                             UpdateTime = o.UpdateTime,
                             InternalDatetime = o.InternalDatetime,
                             Category = o.Category,
                             Type = o.Type,
                             MessageId = o.MessageId,
                             FileId = o.FileId,
                             CustomerMails = o.CustomerMails,
                             MailTime = o.Type == "IN" ? o.InternalDatetime : o.CreateTime,
                             MailBox = o.MailBox,
                             CustomerName = tt.CustomerName,
                             CustomerNum = tt.CustomerNum,
                             SiteUseId = tt.SiteUseId
                         }).OrderByDescending(m => m.CreateTime);

            }

            return mails;
        }

        /// <summary>
        /// get mails by page
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public MailDtoPage QueryMails(MailQueryDto dto)
        {
            MailDtoPage mailList = new MailDtoPage();
            string nMailAddress = GetSenderMailAddress();
            List<MailDto> mails = null;
            List<MailDto> mails1 = null;
            string sql = string.Empty;
            string sql1 = string.Empty;

            string filterExtend = "";
            if (!string.IsNullOrWhiteSpace(dto.Subject))
            {
                filterExtend += string.Format(@" and o.[subject] like N'%{0}%'", dto.Subject);
            }
            if (!string.IsNullOrWhiteSpace(dto.Body))
            {
                filterExtend += string.Format(@" and o.[Body] like N'%{0}%'", dto.Body);
            }
            if (!string.IsNullOrWhiteSpace(dto.From))
            {
                filterExtend += string.Format(@" and o.[From] like N'%{0}%'", dto.From);
            }
            if (!string.IsNullOrWhiteSpace(dto.To))
            {
                filterExtend += string.Format(@" and o.[To] like N'%{0}%'", dto.To);
            }
            if (!string.IsNullOrWhiteSpace(dto.SiteUseId))
            {
                filterExtend += string.Format(@" and o.[SiteUseId] like N'%{0}%'", dto.SiteUseId);
            }

            if (dto.Start != null && dto.Start >= Convert.ToDateTime("1900-01-01 00:00:00"))
            {
                filterExtend += string.Format(@" and o.[CREATE_TIME] >= '{0}'", dto.Start.ToString("yyyy-MM-dd") + " 00:00:00");
            }
            if (dto.end != null && dto.end >= Convert.ToDateTime("1900-01-01 00:00:00"))
            {
                filterExtend += string.Format(@" and o.[CREATE_TIME] <= '{0}'", dto.end.ToString("yyyy-MM-dd") + " 23:59:59");
            }

            var s = AppContext.Current.User.Id;

            if (string.IsNullOrWhiteSpace(dto.CustomerNum) && string.IsNullOrWhiteSpace(dto.CustomerName))
            {
                if (dto.Category == "draft")
                {
                    sql = string.Format(@"WITH tbTmp_CTE as (
                                                select ROW_NUMBER() over(order by o.CREATE_TIME desc) as RowNumber, o.[from] as 'From', o.[to] as 'To', o.cc as 'Cc', o.[subject] as 'Subject' ,o.ID,o.CREATE_TIME as CreateTime,o.UPDATE_TIME as UpdateTime, o.INTERNAL_DATETIME as InternalDatetime,o.CATEGORY as Category,o.[TYPE] as 'Type', 
                                                                        (case when o.TYPE = 'IN' then o.INTERNAL_DATETIME else o.CREATE_TIME end) as MailTime, o.MESSAGE_ID as MessageId,o.FILEID as FileId,o.MAIL_BOX as MailBox,o.BODY as Body
                                                  from[dbo].[T_MAIL_TMP]  o WITH (NOLOCK)
                                                                    where o.CATEGORY = '{0}' and o.Collector = '{4}' and o.deal = '{1}' and (o.MAIL_BOX = '{2}' or o.[from] = '{2}' or o.[cc] like '%{2}%' or o.[subject] like '%{4}%') {3} 
                                                ) SELECT *
                                                FROM tbTmp_CTE WITH (NOLOCK)
                                                WHERE  NOT EXISTS(SELECT 1 FROM tbTmp_CTE AS c WITH (NOLOCK) WHERE c.id =  tbTmp_CTE.id AND tbTmp_CTE.RowNumber > c.RowNumber ) and RowNumber BETWEEN {5} AND {6};", dto.Category, AppContext.Current.User.Deal, nMailAddress, filterExtend, AppContext.Current.User.EID, dto.Skip, dto.Skip + dto.PageSize);

                    mails = CommonRep.ExecuteSqlQuery<MailDto>(sql).ToList();
                    sql1 = string.Format(@"WITH tbTmp_CTE as (
                                                select ROW_NUMBER() over(order by o.CREATE_TIME desc) as RowNumber, o.[from] as 'From', o.[to] as 'To', o.cc as 'Cc', o.[subject] as 'Subject' ,o.ID,o.CREATE_TIME as CreateTime,o.UPDATE_TIME as UpdateTime, o.INTERNAL_DATETIME as InternalDatetime,o.CATEGORY as Category,o.[TYPE] as 'Type', 
                                                                        (case when o.TYPE = 'IN' then o.INTERNAL_DATETIME else o.CREATE_TIME end) as MailTime, o.MESSAGE_ID as MessageId,o.FILEID as FileId,o.MAIL_BOX as MailBox,o.BODY as Body
                                                  from[dbo].[T_MAIL_TMP]  o WITH (NOLOCK)
                                                                    where o.CATEGORY = '{0}' and o.Collector = '{4}' and o.deal = '{1}' and (o.MAIL_BOX = '{2}' or o.[from] = '{2}' or o.[cc] like '%{2}%' or o.[subject] like '%{4}%') {3} 
                                                ) SELECT *
                                                FROM tbTmp_CTE WITH (NOLOCK)
                                                WHERE  NOT EXISTS(SELECT 1 FROM tbTmp_CTE AS c WITH (NOLOCK) WHERE c.id =  tbTmp_CTE.id AND tbTmp_CTE.RowNumber > c.RowNumber ) ;", dto.Category, AppContext.Current.User.Deal, nMailAddress, filterExtend, AppContext.Current.User.EID);

                    mails1 = CommonRep.ExecuteSqlQuery<MailDto>(sql1).ToList();
                }
                else
                {
                    sql = string.Format(@"WITH tbTmp_CTE as (
                                                select ROW_NUMBER() over(order by o.CREATE_TIME desc) as RowNumber, o.[from] as 'From', o.[to] as 'To', o.cc as 'Cc', o.[subject] as 'Subject' ,o.ID,o.CREATE_TIME as CreateTime,o.UPDATE_TIME as UpdateTime, o.INTERNAL_DATETIME as InternalDatetime,o.CATEGORY as Category,o.[TYPE] as 'Type', 
                                                                        (case when o.TYPE = 'IN' then o.INTERNAL_DATETIME else o.CREATE_TIME end) as MailTime, o.MESSAGE_ID as MessageId,o.FILEID as FileId,o.MAIL_BOX as MailBox,o.BODY as Body
                                                  from[dbo].[T_MAIL_TMP]  o WITH (NOLOCK)
                                                                    where o.CATEGORY = '{0}' and o.deal = '{1}' and (o.MAIL_BOX = '{2}' or o.[from] = '{2}' or o.[cc] like '%{2}%' or o.[subject] like '%{4}%') {3} 
                                                ) SELECT *
                                                FROM tbTmp_CTE WITH (NOLOCK)
                                                WHERE  NOT EXISTS(SELECT 1 FROM tbTmp_CTE AS c WITH (NOLOCK) WHERE c.id =  tbTmp_CTE.id AND tbTmp_CTE.RowNumber > c.RowNumber ) and RowNumber BETWEEN {5} AND {6};", dto.Category, AppContext.Current.User.Deal, nMailAddress, filterExtend, AppContext.Current.User.EID, dto.Skip, dto.Skip + dto.PageSize);

                    mails = CommonRep.ExecuteSqlQuery<MailDto>(sql).ToList();
                    sql1 = string.Format(@"WITH tbTmp_CTE as (
                                                select ROW_NUMBER() over(order by o.CREATE_TIME desc) as RowNumber, o.[from] as 'From', o.[to] as 'To', o.cc as 'Cc', o.[subject] as 'Subject' ,o.ID,o.CREATE_TIME as CreateTime,o.UPDATE_TIME as UpdateTime, o.INTERNAL_DATETIME as InternalDatetime,o.CATEGORY as Category,o.[TYPE] as 'Type', 
                                                                        (case when o.TYPE = 'IN' then o.INTERNAL_DATETIME else o.CREATE_TIME end) as MailTime, o.MESSAGE_ID as MessageId,o.FILEID as FileId,o.MAIL_BOX as MailBox,o.BODY as Body
                                                  from[dbo].[T_MAIL_TMP]  o WITH (NOLOCK)
                                                                    where o.CATEGORY = '{0}' and o.deal = '{1}' and (o.MAIL_BOX = '{2}' or o.[from] = '{2}' or o.[cc] like '%{2}%' or o.[subject] like '%{4}%') {3} 
                                                ) SELECT *
                                                FROM tbTmp_CTE WITH (NOLOCK)
                                                WHERE  NOT EXISTS(SELECT 1 FROM tbTmp_CTE AS c WITH (NOLOCK) WHERE c.id =  tbTmp_CTE.id AND tbTmp_CTE.RowNumber > c.RowNumber ) ;", dto.Category, AppContext.Current.User.Deal, nMailAddress, filterExtend, AppContext.Current.User.EID);

                    mails1 = CommonRep.ExecuteSqlQuery<MailDto>(sql1).ToList();
                }
            }
            else
            {
                XcceleratorService collecotr = SpringFactory.GetObjectImpl<XcceleratorService>("XcceleratorService");
                long ueId = collecotr.GetUserOrganization(AppContext.Current.User.EID).Id;

                sql = string.Format(@"WITH tbTmp_CTE as (
                                               select ROW_NUMBER() over(order by o.CREATE_TIME desc) as RowNumber, o.[from] as 'From', o.[to] as 'To', o.cc as 'Cc', o.[subject] as 'Subject' ,o.ID,o.CREATE_TIME as CreateTime,o.UPDATE_TIME as UpdateTime, o.INTERNAL_DATETIME as InternalDatetime,o.CATEGORY as Category,o.[TYPE] as 'Type', 
                                                (case when o.TYPE = 'IN' then o.INTERNAL_DATETIME else o.CREATE_TIME end) as MailTime, o.MESSAGE_ID as MessageId,o.FILEID as FileId,o.MAIL_BOX as MailBox,c.CUSTOMER_NAME as CustomerName,c.CUSTOMER_NUM as CustomerNum,c.SiteUseId,o.BODY as Body
                                                from[dbo].[T_MAIL_TMP]  o WITH (NOLOCK)
                                                left join[dbo].[T_CUSTOMER_MAIL] cm WITH (NOLOCK) on o.MESSAGE_ID = cm.MESSAGE_ID
                                                left join[dbo].[T_CUSTOMER] c WITH (NOLOCK) on c.CUSTOMER_NUM = cm.CUSTOMER_NUM and c.SiteUseId = cm.SiteUseId
                                                left join dbo.T_CONTACTOR ct WITH (NOLOCK) on c.CUSTOMER_NUM = ct.CUSTOMER_NUM 
                                                inner join (select c1.customer_num, c1.CUSTOMER_NAME , c1.SiteUseId, c1.DEAL,CHARINDEX(c1.CUSTOMER_NUM , '{0}') as mark 
                                                from dbo.[T_CUSTOMER] c1 WITH (NOLOCK)
                                                inner join [dbo].T_users u WITH (NOLOCK) on u.user_code = c1.collector
                                                left join dbo.T_USER_EMPLOYEE ue WITH (NOLOCK) on ue.ID = u.USER_EMPLOYEE_ID
                                                where ue.id='{1}' or ue.direct_manager_id = '{1}') tmp on tmp.CUSTOMER_NUM = c.CUSTOMER_NUM and tmp.SiteUseId = c.SiteUseId and tmp.DEAL = c.SiteUseId
                                                where o.CATEGORY = '{2}' and o.deal = '{3}' and (o.MAIL_BOX = '{4}' or o.[from] = '{4}') {5} and tmp.CUSTOMER_NAME like '%{6}%'  and tmp.mark>0 and  ([dbo].[InList](o.[FROM],o.[to],o.[cc],'{7}') >0 or o.[subject] like '%{7}%')
                                                ) SELECT *
                                                FROM tbTmp_CTE WITH (NOLOCK)
                                                WHERE  NOT EXISTS(SELECT 1 FROM tbTmp_CTE AS c WITH (NOLOCK) WHERE c.id =  tbTmp_CTE.id AND tbTmp_CTE.RowNumber > c.RowNumber ) and RowNumber BETWEEN {8} AND {9};", dto.CustomerNum, ueId, dto.Category, AppContext.Current.User.Deal, nMailAddress, filterExtend, dto.CustomerName, AppContext.Current.User.EID, dto.Skip, dto.Skip + dto.PageSize);

                mails = CommonRep.ExecuteSqlQuery<MailDto>(sql).ToList();
            }

            mailList.mailList = mails;
            mailList.listCount = mails1.Count();
            return mailList;
        }


        public IEnumerable<CustomerMasterData> GetCustomerByMessageId(int mailId)
        {
            MailTmp mail = CommonRep.GetQueryable<MailTmp>().Where(x => x.Id == mailId).FirstOrDefault();
            IQueryable<CustomerMasterData> custMails = (from x in CommonRep.GetQueryable<CustomerMail>()
                                                        join y in CommonRep.GetDbSet<CustomerMasterData>()
                                                        on new { CustomerNum = x.CustomerNum, SiteUseId = x.SiteUseId } equals new { CustomerNum = y.CustomerNum, SiteUseId = y.SiteUseId }
                                                        where x.MessageId == mail.MessageId
                                                        select y);

            return custMails;

        }

        public IEnumerable<CustomerMasterData> GetCustomers(string mailCustNums, string siteUseId)
        {
            return CommonRep.GetDbSet<CustomerMasterData>().Where(o => o.CustomerName == mailCustNums && o.SiteUseId == siteUseId);

        }

        //cusnum, siteuse id
        public IEnumerable<MailInvoiceDto> GetInvoiceByMailId(List<CustomerKey> customer)
        {
            if (customer == null || customer.Count == 0)
                return new List<MailInvoiceDto>();
            
            List<string> custNumList = customer.Select(x => x.CustomerNum).ToList();
            List<string> siteUseIdList = customer.Select(x => x.SiteUseId).ToList();

            var invList1 = CommonRep.GetQueryable<InvoiceAging>().Where(m => m.TrackStates != "014" && m.TrackStates != "016"
            && custNumList.Contains(m.CustomerNum) && siteUseIdList.Contains(m.SiteUseId));

            var invList2 = from x in invList1
                           join x2 in CommonRep.GetQueryable<SysTypeDetail>().Where(xx=>xx.TypeCode == "029")
                           on x.TrackStates equals x2.DetailValue
                           into tmp 
                           from x3 in tmp.DefaultIfEmpty()
                           join y in CommonRep.GetQueryable<T_INVOICE_VAT>()
                           on x.InvoiceNum equals y.Trx_Number
                           into tmp1
                           from z in tmp1.DefaultIfEmpty()
                           join o in CommonRep.GetQueryable<DisputeInvoice>()
                           on x.InvoiceNum equals o.InvoiceId
                           into tmp2
                           from p in tmp2.DefaultIfEmpty()
                           join q in CommonRep.GetQueryable<Dispute>()
                           on p.DisputeId equals q.Id
                           into tmp3
                           from r in tmp3.DefaultIfEmpty()
                           select new MailInvoiceDto() {
                               Id = x.Id,
                               InvoiceNum = x.InvoiceNum,
                               InvoiceDate=x.InvoiceDate,
                               CustomerNum=x.CustomerNum,
                               CustomerName=x.CustomerName,
                               Class=x.Class,
                               SiteUseId=x.SiteUseId,
                               LegalEntity=x.LegalEntity,
                               DueDate=x.DueDate,
                               CreditTrem=x.CreditTrem,
                               Currency=x.Currency,
                               DaysLateSys=x.DaysLateSys,
                               BalanceAmt=x.BalanceAmt,
                               WoVat_AMT=x.WoVat_AMT,
                               AgingBucket=x.AgingBucket,
                               Eb=x.Eb,
                               Ebname=x.Ebname,
                               TRACK_DATE=x.TRACK_DATE,
                               PtpDate=x.PtpDate,
                               Comments=x.Comments,
                               CollectorName = x.CollectorName,
                               TrackStatesName = x3.DetailName,
                               vatNo  = z.VATInvoice,
                               vatDate = z.VATInvoiceDate,
                               Payment_Date = x.Payment_Date
                           };

            return invList2.AsQueryable<MailInvoiceDto>();
        }

        public IEnumerable<InvoiceAging> GetInvoiceByInputNums(string mailMsgIdForInv, string inputNums)
        {
            var customer = CommonRep.GetQueryable<CustomerMail>().Where(o => o.MessageId == mailMsgIdForInv).ToList();
            List<InvoiceAging> newinvlist = new List<InvoiceAging>();
            foreach (var cust in customer)
            {
                var suid = cust.SiteUseId;
                var custnum = cust.CustomerNum;
                var inv = CommonRep.GetQueryable<InvoiceAging>().Where(m => m.CustomerNum == custnum && m.SiteUseId == suid && m.TrackStates != "014" && m.TrackStates != "016").ToList();
                InvoiceAging newinv = new InvoiceAging();
                if (inv.Count > 0)
                {
                    foreach (var invo in inv)
                    {
                        newinv = new InvoiceAging();
                        newinv.AgingBucket = invo.AgingBucket;
                        newinv.ArBalanceAmtPeroid = invo.ArBalanceAmtPeroid;
                        newinv.BalanceAmt = invo.BalanceAmt;
                        newinv.CallId = invo.CallId;
                        newinv.Class = invo.Class;
                        newinv.CollectorContact = invo.CollectorContact;
                        newinv.CollectorName = invo.CollectorName;
                        newinv.Comments = invo.Comments;
                        newinv.CreateDate = invo.CreateDate;
                        newinv.CreditTrem = invo.CreditTrem;
                        newinv.Id = invo.Id;
                        newinv.Currency = invo.Currency;
                        newinv.CustomerName = invo.CustomerName;
                        newinv.CustomerNum = invo.CustomerNum;
                        newinv.DueDate = invo.DueDate;
                        newinv.Eb = invo.Eb;
                        newinv.DaysLateSys = invo.DaysLateSys;
                        newinv.FinishedStatus = invo.FinishedStatus;
                        newinv.InvoiceDate = invo.InvoiceDate;
                        newinv.InvoiceNum = invo.InvoiceNum;
                        newinv.LegalEntity = invo.LegalEntity;
                        newinv.MailId = invo.MailId;
                        newinv.PaidAmt = invo.PaidAmt;
                        newinv.PtpDate = invo.PtpDate;
                        newinv.Sales = invo.Sales;
                        newinv.SiteUseId = invo.SiteUseId;
                        newinv.TrackStates = !string.IsNullOrEmpty(invo.TrackStates) == false ? "" : Helper.CodeToEnum<TrackStatus>(invo.TrackStates).ToString().Replace("_", " ");
                        newinv.TRACK_DATE = invo.TRACK_DATE;
                        newinv.UpdateDate = invo.UpdateDate;
                        newinv.WoVat_AMT = invo.WoVat_AMT;
                        newinvlist.Add(newinv);
                    }
                }
                else
                {
                    newinv = new InvoiceAging();
                    newinvlist.Add(newinv);
                }
            }

            return newinvlist.Where(m => m.InvoiceNum.Contains(inputNums));
        }
        
        public void RemoveCus(string messageId, string CusNum, string siteUseId)
        {
            var existing = CommonRep.GetDbSet<CustomerMail>().Where(cm => cm.MessageId == messageId && cm.CustomerNum == CusNum && cm.SiteUseId == siteUseId);
            CommonRep.GetDbSet<CustomerMail>().RemoveRange(existing.AsEnumerable());
            CommonRep.Commit();
            var mail = (from m in CommonRep.GetQueryable<CustomerMail>() where m.MessageId == messageId select m.CustomerNum).Count();
            if (mail == 0)
            {
                MailTmp Mailtmp = CommonRep.GetQueryable<MailTmp>().Where(c => c.MessageId == messageId).FirstOrDefault();

                if (Mailtmp.Category == "CustomerNew")
                    Mailtmp.Category = "Unknow";
                
                CommonRep.Save(Mailtmp);
                CommonRep.Commit();
            }
        }
        public string getCustomerLanguageByCusnum(string custnum, string siteUseId)
        {
            var deal = AppContext.Current.User.Deal;
            var templang = "";
            if (custnum == "" || custnum == null || custnum == "undefined" || siteUseId == "" || siteUseId == null || siteUseId == "undefined")
            {
                templang = ConfigurationManager.AppSettings["DefaultLanguageCode"];
            }
            else
            {
                List<string> customerNums = custnum.Split(',').ToList<string>();
                var defaultcustnum = customerNums[0];
                List<string> listsiteUseId = siteUseId.Split(',').ToList<string>();
                var defaultsiteUseId = listsiteUseId[0];
                var cus = CommonRep.GetQueryable<Customer>().Where(o => o.CustomerNum == defaultcustnum && o.SiteUseId == defaultsiteUseId && o.Deal == deal).FirstOrDefault();
                templang = cus.ContactLanguage;
                if (templang == null || templang == "")
                {
                    templang = ConfigurationManager.AppSettings["DefaultLanguageCode"];
                }
            }

            return templang;
        }
        public List<string> getCustomerLegalEntityByCusnum(string custnum, string siteUseId)
        {
            var deal = AppContext.Current.User.Deal;
            var templang = "";
            List<string> customerNums = custnum.Split(',').ToList<string>();
            var defaultcustnum = customerNums[0];
            List<string> listsiteUseId = siteUseId.Split(',').ToList<string>();
            var defaultsiteUseId = listsiteUseId[0];
            var legalEntity = CommonRep.GetQueryable<Customer>().Where(o => o.CustomerNum == defaultcustnum && o.SiteUseId == defaultsiteUseId && o.Deal == deal)
                .Select(t=>t.Organization)
                .Distinct().ToList();

            return legalEntity;
        }
        public List<string> getCustomerRegionByCusnum(string custnum, string siteUseId)
        {
            var deal = AppContext.Current.User.Deal;
            var templang = "";
            List<string> customerNums = custnum.Split(',').ToList<string>();
            var defaultcustnum = customerNums[0];
            List<string> listsiteUseId = siteUseId.Split(',').ToList<string>();
            var defaultsiteUseId = listsiteUseId[0];
            var legalEntity = CommonRep.GetQueryable<Customer>().Where(o => o.CustomerNum == defaultcustnum && o.SiteUseId == defaultsiteUseId && o.Deal == deal)
                .Select(t => t.Region)
                .Distinct().ToList();

            return legalEntity;
        }
        public void UpdateCusMails(string messageId, string CusNums)
        {
            var existing = CommonRep.GetDbSet<CustomerMail>().Where(cm => cm.MessageId == messageId);
            CommonRep.GetDbSet<CustomerMail>().RemoveRange(existing.AsEnumerable());
            CommonRep.Commit();

            if (!string.IsNullOrEmpty(CusNums))
            {
                List<string> customerNums = CusNums.Split(',').ToList<string>();


                List<CustomerMail> newAdds = new List<CustomerMail>();
                CustomerMail newCus = new CustomerMail();
                foreach (var item in customerNums)
                {
                    newCus = new CustomerMail();
                    newCus.CustomerNum = item;
                    newCus.MessageId = messageId;
                    newAdds.Add(newCus);
                }

                CommonRep.AddRange(newAdds);
                CommonRep.Commit();
            }

        }

        //校验选中的发票，To的是否为同一人
        public Boolean CheckMailToOnly(string toTitle, string alertType, List<int> intIds)
        {
            List<string> toNameList = new List<string>();
            
            if (toTitle == "CS")
            {
                toNameList = (from a in CommonRep.GetQueryable<InvoiceAging>()
                              where a.TrackStates != "014"
                                 && a.TrackStates != "016"
                                 && intIds.Contains(a.Id)
                              group a by new { a.LsrNameHist } into k
                              select k.Key.LsrNameHist).ToList();
            }
            else if (toTitle == "Sales")
            {
                toNameList = (from a in CommonRep.GetQueryable<InvoiceAging>()
                              where a.TrackStates != "014"
                                 && a.TrackStates != "016"
                                 && intIds.Contains(a.Id)
                              group a by new { a.FsrNameHist } into k
                              select k.Key.FsrNameHist).ToList();
            }
            else {
                toNameList = (from a in CommonRep.GetQueryable<InvoiceAging>()
                              join b in CommonRep.GetQueryable<V_CUSTOMER_CONTACTOR_TITLE>() on new { SiteUseId = a.SiteUseId } equals new { SiteUseId = b.SiteUseId }
                              into ez
                              from ezs in ez.DefaultIfEmpty()
                              where a.TrackStates != "014"
                                 && a.TrackStates != "016"
                                 && intIds.Contains(a.Id)
                                 && ezs.TITLE == toTitle
                              group ezs by new { ezs.CONTACT } into k
                              select k.Key.CONTACT).ToList();
            }
            if (toNameList == null || toNameList.Count != 1) {
                return false;
            }

            string toName = toNameList[0];
            List<int> toInvsList = (from a in CommonRep.GetQueryable<InvoiceAging>()
                                 join b in CommonRep.GetQueryable<V_CUSTOMER_CONTACTOR_TITLE>() on new { SiteUseId = a.SiteUseId } equals new { SiteUseId = b.SiteUseId }
                                 into ez
                                 from ezs in ez.DefaultIfEmpty()
                                 where a.TrackStates != "014"
                                    && a.TrackStates != "016"
                                    && intIds.Contains(a.Id)
                                    && ezs.TITLE == toTitle
                                    && ezs.CONTACT == toName
                                    group a by new { a.Id } into k
                                 select k.Key.Id).ToList();

            if (toInvsList == null || intIds.Count != toInvsList.Count) {
                return false;
            }

            return true;
        }

        public string getMailToContactName(string toTitle, string alertType, List<int> intIds) {

            string toName = (from a in CommonRep.GetQueryable<InvoiceAging>()
                                    join b in CommonRep.GetQueryable<V_CUSTOMER_CONTACTOR_TITLE>() on new { SiteUseId = a.SiteUseId } equals new { SiteUseId = b.SiteUseId}
                                    into ez
                                    from ezs in ez.DefaultIfEmpty()
                                    where a.TrackStates != "014"
                                       && a.TrackStates != "016"
                                       && intIds.Contains(a.Id)
                                       && ezs.TITLE == toTitle
                                    group ezs by new { ezs.CONTACT } into k
                                    select k.Key.CONTACT).FirstOrDefault();
            return toName;
        }

        //校验选中的发票，To的是否为同一人
        public List<int> CheckMailToFactInv(string toTitle, string toName, string alertType, List<int> intIds, string strTempleteLanguage)
        {
            List<int> listInvs = new List<int>();

            List<InvoiceAging> toInvsList = (from a in CommonRep.GetQueryable<InvoiceAging>()
                                  join b in CommonRep.GetQueryable<V_CUSTOMER_CONTACTOR_TITLE>() on new { SiteUseId = a.SiteUseId } equals new { SiteUseId = b.SiteUseId }
                                  where a.TrackStates != "014"
                                      && a.TrackStates != "016"
                                      && intIds.Contains(a.Id)
                                      && b.TITLE == toTitle
                                      && toName == b.CONTACT
                                  select a).ToList();

            int intPeriodEndDays = 0;
            DateTime dt_now = Convert.ToDateTime(AppContext.Current.User.Now.ToString("yyyy-MM-dd 00:00:00"));
            PeriodControl period = CommonRep.GetQueryable<PeriodControl>().Where(O=>dt_now >= O.PeriodBegin && dt_now <= O.PeriodEnd ).FirstOrDefault();
            DateTime dt_PeriodEndDate = period.EndDate;
            if (dt_PeriodEndDate == null) {
                throw new OTCServiceException("Period error !");
            }
            else { 
                intPeriodEndDays = (Convert.ToDateTime(dt_PeriodEndDate.ToString("yyyy-MM-dd")) - AppContext.Current.User.Now).Days + 1;
            }
            dt_PeriodEndDate = Convert.ToDateTime(dt_PeriodEndDate.ToString("yyyy-MM-dd 23:59:59"));

            DateTime dt_nowS = Convert.ToDateTime(AppContext.Current.User.Now.ToString("yyyy-MM-dd") + " 00:00:00");
            DateTime dt_nowE = Convert.ToDateTime(AppContext.Current.User.Now.ToString("yyyy-MM-dd") + " 23:59:59");

            switch (alertType)
            {
                case "001":   //Wave1(All)
                    if (strTempleteLanguage == "007" || strTempleteLanguage == "0071")
                    {
                        listInvs = (from inv in toInvsList
                                    where inv.Class == "INV"
                                    select inv.Id).ToList();
                    }
                    else
                    {
                        listInvs = (from inv in toInvsList
                                    select inv.Id).ToList();
                    }
                    break;
                case "002":   //Wave2(all overdue)
                    if (strTempleteLanguage == "006" || strTempleteLanguage == "008")
                    {
                        listInvs = (from inv in toInvsList
                                    where inv.Class == "INV" 
                                    select inv.Id).ToList();
                    }
                    else if (strTempleteLanguage == "007" || strTempleteLanguage == "0071")
                    {
                        listInvs = (from inv in toInvsList
                                    where inv.Class == "INV"
                                    select inv.Id).ToList();
                    }
                    else
                    {
                        listInvs = (from inv in toInvsList
                                    where SqlMethods.DateDiffDay(inv.DueDate, dt_PeriodEndDate) >= 0
                                    select inv.Id).ToList();
                    }
                    break;
                case "003":   //Wave3(60+未响应)
                case "004":   //Wave4
                    if (strTempleteLanguage == "006" || strTempleteLanguage == "008")
                    {
                        listInvs = (from inv in toInvsList
                                    where inv.Class == "INV"
                                    select inv.Id).ToList();
                    }
                    else if (strTempleteLanguage == "007" || strTempleteLanguage == "0071")
                    {
                        listInvs = (from inv in toInvsList
                                    where inv.Class == "INV"
                                    && SqlMethods.DateDiffDay(inv.DueDate, dt_PeriodEndDate) >= 0
                                    select inv.Id).ToList();
                    }
                    else
                    {
                        listInvs = (from inv in toInvsList
                                    where 
                                      (SqlMethods.DateDiffDay(inv.DueDate, dt_PeriodEndDate) >= 60)
                                     && (inv.OverdueReason == null ? "" : inv.OverdueReason) == ""
                                     && (inv.PtpDate == null ? "" : inv.PtpDate.ToString()) == ""
                                     && (inv.Comments == null ? "" : inv.Comments.ToString()) == ""
                                    select inv.Id).ToList();
                    }
                    break;
                case "005":   //PMT
                    var siteList = (from inv in toInvsList
                                    join c in CommonRep.GetQueryable<CustomerAging>() on new { SiteUseId = inv.SiteUseId } equals new { SiteUseId = c.SiteUseId }
                                    select c.SiteUseId).ToList();
                    toInvsList = (from inv in CommonRep.GetQueryable<InvoiceAging>()
                                  where inv.TrackStates != "014" && inv.TrackStates != "016" && siteList.Contains(inv.SiteUseId)
                                  select inv).ToList<InvoiceAging>();

                    var notPMT = CommonRep.GetQueryable<SysTypeDetail>().Where(o => o.TypeCode == "048").Select(o => o.DetailName).DefaultIfEmpty().ToList();
                    listInvs = (from inv in toInvsList
                                join c in CommonRep.GetQueryable<CustomerAging>() on new { SiteUseId = inv.SiteUseId } equals new { SiteUseId = c.SiteUseId }
                                where ((inv.Class == "PMT" && !notPMT.Contains(c.CreditTrem == null ? "" : c.CreditTrem)
                                ) || inv.Class != "PMT")
                                select inv.Id).DefaultIfEmpty().ToList();
                    var listInvsINV = (from inv in toInvsList
                                       where inv.Class == "INV"
                                       select inv.Id).ToList();
                    var listInvsPMT = (from inv in toInvsList
                                       join c in CommonRep.GetQueryable<CustomerAging>() on new { SiteUseId = inv.SiteUseId } equals new { SiteUseId = c.SiteUseId }
                                       where (inv.Class == "PMT" && !notPMT.Contains(c.CreditTrem == null ? "" : c.CreditTrem)
                                       )
                                       select inv.Id).ToList();
                    if (listInvsINV == null || listInvsPMT == null || listInvsINV.Count == 0 || listInvsPMT.Count == 0)
                    {
                        listInvs = new List<int>();
                    }
                    break;
            }

            return listInvs;
        }

        public string getMailResponseDate(string alertType) {

            var periodId = (from a in CommonRep.GetQueryable<PeriodControl>()
                 where a.PeriodBegin <= AppContext.Current.User.Now && AppContext.Current.User.Now <= a.PeriodEnd
                 select a.Id).FirstOrDefault();

            if (periodId == 0) {
                return AppContext.Current.User.Now.ToString("yyyy-MM-dd");
            }

            StringBuilder sbselect = new StringBuilder();
            StringBuilder sbOderby = new StringBuilder();
            sbselect.Append(@" SELECT dbo.GetWaveLastDay(@alertType,@periodId) ");

            SqlParameter[] paramForSQL_Detail = new SqlParameter[2];
            SqlParameter param1 = new SqlParameter("@alertType", Convert.ToInt32(alertType));
            paramForSQL_Detail[0] = param1;
            SqlParameter param2 = new SqlParameter("@periodId", periodId);
            paramForSQL_Detail[1] = param2;
            string dtResponseDate = SqlHelper.ExcuteScalar<string>(sbselect.ToString(), paramForSQL_Detail);

            if (string.IsNullOrEmpty(dtResponseDate)) {
                return AppContext.Current.User.Now.ToString("yyyy-MM-dd");
            }

            return dtResponseDate;
        }

        public string getContactorMailByInv(List<int> invs, string title) {
            List<string> sname = new List<string>();
            List<string> scs = new List<string>();
            List<string> creditofficers = new List<string>();
            List<string> ssales = new List<string>();
            if (title == "CS")
            {
                var cs = from c in CommonRep.GetQueryable<Contactor>()
                         join inv in CommonRep.GetQueryable<InvoiceAging>()
                         on new { SiteUseId = c.SiteUseId, Name = c.Name, Title = c.Title } equals new { SiteUseId = inv.SiteUseId, Name = inv.LsrNameHist, Title = "CS" }
                         where invs.Contains(inv.Id)
                         group c by c.EmailAddress into g
                         select new { Key = g.Key };
                sname = (from c in cs
                                select c.Key).ToList();

            }
            else if (title == "Sales")
            {
                var sales = from c in CommonRep.GetQueryable<Contactor>()
                         join inv in CommonRep.GetQueryable<InvoiceAging>()
                         on new { SiteUseId = c.SiteUseId, Name = c.Name, Title = c.Title } equals new { SiteUseId = inv.SiteUseId, Name = inv.FsrNameHist, Title = "Sales" }
                            where invs.Contains(inv.Id)
                            group c by c.EmailAddress into g
                            select new { Key = g.Key };
                sname = (from c in sales
                             select c.Key).ToList();
            }
            else if (title == "CS;Sales")
            {
                var cs = from c in CommonRep.GetQueryable<Contactor>()
                         join inv in CommonRep.GetQueryable<InvoiceAging>()
                         on new { SiteUseId = c.SiteUseId, Name = c.Name, Title = c.Title } equals new { SiteUseId = inv.SiteUseId, Name = inv.LsrNameHist, Title = "CS" }
                         where invs.Contains(inv.Id)
                         group c by c.EmailAddress into g
                         select new { Key = g.Key };
                scs = (from c in cs
                         select c.Key).ToList();
                var sales = from c in CommonRep.GetQueryable<Contactor>()
                            join inv in CommonRep.GetQueryable<InvoiceAging>()
                            on new { SiteUseId = c.SiteUseId, Name = c.Name, Title = c.Title } equals new { SiteUseId = inv.SiteUseId, Name = inv.FsrNameHist, Title = "Sales" }
                            where invs.Contains(inv.Id)
                            group c by c.EmailAddress into g
                            select new { Key = g.Key };
                ssales = (from c in sales
                         select c.Key).ToList();
                sname = scs;
                sname.AddRange(ssales);
            }
            else if (title == "Credit Officer;Collector")
            {
                var creditofficer = from c in CommonRep.GetQueryable<Contactor>()
                            join inv in CommonRep.GetQueryable<InvoiceAging>()
                            on new { SiteUseId = c.SiteUseId,Title = c.Title } equals new { SiteUseId = inv.SiteUseId, Title = "Credit Officer" }
                            where invs.Contains(inv.Id)
                            group c by c.EmailAddress into g
                            select new { Key = g.Key };
                creditofficers = (from c in creditofficer
                       select c.Key).ToList();
                sname = creditofficers;
                sname.Add(AppContext.Current.User.Email);
            }
            string strMailAddress = "";
            if(sname.Count > 0) {
                strMailAddress = string.Join(";", sname.ToArray());
            }
            return strMailAddress;
        }

        public void findContactor(SendContactorNameDto finder) {
            Mailer.FindContactor(finder);
        } 
    }
}
