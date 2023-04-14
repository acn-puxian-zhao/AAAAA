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
    public class SendCaMailJob : BaseJob
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

                string strID = jobDataMap.GetString("ID");
                string strTASKID = jobDataMap.GetString("TASKID");
                string strEID = jobDataMap.GetString("EID");
                string strBSID = jobDataMap.GetString("BSID");
                string strTransNumber = jobDataMap.GetString("TransNumber");
                string strAlertType = jobDataMap.GetString("AlertType");
                string strLegalEntity = jobDataMap.GetString("LegalEntity");
                string strCustomerNum = jobDataMap.GetString("CustomerNum");
                string strSiteUseId = jobDataMap.GetString("SiteUseId");
                string strTOTITLE = jobDataMap.GetString("TOTITLE");
                string strCCTITLE = jobDataMap.GetString("CCTITLE");
                string strIndexFile = jobDataMap.GetString("IndexFile");
                string strBusinessId = jobDataMap.GetString("BusinessId");

                if (!string.IsNullOrEmpty(strID))
                    logger.Info(string.Format("Excute SendCaMailJob ID is:{0}", strID));
                if (!string.IsNullOrEmpty(strEID))
                    logger.Info(string.Format("Excute SendCaMailJob EID is:{0}", strEID));
                if (!string.IsNullOrEmpty(strBSID))
                    logger.Info(string.Format("Excute SendCaMailJob BSID is:{0}", strBSID));
                if (!string.IsNullOrEmpty(strTransNumber))
                    logger.Info(string.Format("Excute SendCaMailJob TransNumber is:{0}", strTransNumber));
                if (!string.IsNullOrEmpty(strAlertType))
                    logger.Info(string.Format("Excute SendCaMailJob AlertType is:{0}", strAlertType));
                if (!string.IsNullOrEmpty(strLegalEntity))
                    logger.Info(string.Format("Excute SendCaMailJob LegalEntity is:{0}", strLegalEntity));
                if (!string.IsNullOrEmpty(strCustomerNum))
                    logger.Info(string.Format("Excute SendCaMailJob CustomerNum is:{0}", strCustomerNum));
                if (!string.IsNullOrEmpty(strSiteUseId))
                    logger.Info(string.Format("Excute SendCaMailJob SiteUseId is:{0}", strSiteUseId));
                if (!string.IsNullOrEmpty(strTOTITLE))
                    logger.Info(string.Format("Excute SendCaMailJob TOTITLE is:{0}", strTOTITLE));
                if (!string.IsNullOrEmpty(strCCTITLE))
                    logger.Info(string.Format("Excute SendCaMailJob CCTITLE is:{0}", strCCTITLE));
                if (!string.IsNullOrEmpty(strBusinessId))
                    logger.Info(string.Format("Excute SendCaMailJob BusinessId is:{0}", strBusinessId));

                CaMailAlertDto alertKey = new CaMailAlertDto() {
                    ID = strID,
                    EID = strEID,
                    BSID = strBSID,
                    TransNumber = strTransNumber,
                    AlertType = strAlertType,
                    LegalEntity = strLegalEntity,
                    CustomerNum = strCustomerNum,
                    SiteUseId = strSiteUseId,
                    TOTITLE = strTOTITLE,
                    CCTITLE = strCCTITLE,
                    IndexFile = strIndexFile,
                    businessId = strBusinessId
                };
                                
                init();

                sendMailTask(alertKey, index);

            }
            catch (Exception ex)
            {
                logger.Error(string.Format("SendCaMailError:{0},{1}", 0, true), ex);
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
        private void sendMailTask(CaMailAlertDto alertKey, int index)
        {
            try
            {
                logger.Info("-------------------------(1)START SEND --------------------------" + index);
                MailTmp nMail = generateMailByAlert(alertKey);
                if (nMail == null) { return; }
                string strBusinessId = alertKey.businessId;
                if (string.IsNullOrEmpty(strBusinessId) || strBusinessId == "") {
                    strBusinessId = Guid.NewGuid().ToString().Replace("-", "");
                }
                logger.Info("*************** BusinessId:" + strBusinessId);
                //发送邮件(保存为draft)
                int mailId = 0;
                if (nMail != null && !string.IsNullOrEmpty(nMail.To))
                {
                    mailId = sendMail(nMail, alertKey);
                }
                else {
                    Helper.Log.Info("******************* nMail is null******************");
                }
                //由直接发送改为通知MailAdvisor接口发送
                string mailAdvisorSQL = @" if not exists (select 1 from T_MailAdvisor_CASendMail where IOCBusinessId = @IOCBusinessId)
                                           insert into T_MailAdvisor_CASendMail (Id,
                                                                                BusinessId,
                                                                                IOCBusinessId,
                                                                                MailId,
                                                                                Subject,
                                                                                Body,
                                                                                BodyFormat,
                                                                                [From],
                                                                                [To],
                                                                                [Cc],
                                                                                Attachment,
                                                                                CreateUser,
                                                                                CreateDate,
                                                                                Status,
                                                                                CAProcessFlag,
                                                                                MAProcessFlag)
                                                                        Values ( newid(), @BusinessId, @IOCBusinessId, @MailId, @Subject, @Body, @BodyFormat, @From, @To, @Ccer, @Attachment, @CreateUser, getdate(), 'Initialized',0,0)";
                logger.Info("111111111111111111111");
                logger.Info("strBusinessId:" + strBusinessId);
                logger.Info("alertKey.ID:" + alertKey.ID);
                logger.Info("mailId:" + mailId);
                logger.Info("Subject:" + nMail.Subject == null ? "" : nMail.Subject);
                logger.Info("Body:" + nMail.Body == null ? "" : nMail.Body);
                logger.Info("BodyFormat:" + nMail.BodyFormat == null ? "" : nMail.BodyFormat);
                logger.Info("From:" + nMail.From == null ? "" : nMail.From);
                logger.Info("To:" + nMail.To == null ? "" : nMail.To);
                logger.Info("Cc:" + nMail.Cc == null ? "" : nMail.Cc);
                logger.Info("AttachmentPath:" + nMail.AttachmentPath == null ? "" : nMail.AttachmentPath);
                logger.Info("EID:" + alertKey.EID == null ? "" : alertKey.EID);

                SqlParameter[] parm = {
                                        new SqlParameter("@BusinessId", strBusinessId),
                                        new SqlParameter("@IOCBusinessId", alertKey.ID),
                                        new SqlParameter("@MailId", mailId),
                                        string.IsNullOrEmpty(nMail.Subject) ? new SqlParameter("@Subject",  DBNull.Value) : new SqlParameter("@Subject", nMail.Subject),
                                        string.IsNullOrEmpty(nMail.Body) ? new SqlParameter("@Body",  DBNull.Value) : new SqlParameter("@Body", nMail.Body),
                                        string.IsNullOrEmpty(nMail.BodyFormat) ?  new SqlParameter("@BodyFormat", "HTML") : new SqlParameter("@BodyFormat", nMail.BodyFormat),
                                        string.IsNullOrEmpty(nMail.From) ? new SqlParameter("@From",  DBNull.Value) : new SqlParameter("@From", nMail.From),
                                        string.IsNullOrEmpty(nMail.To) ? new SqlParameter("@To",  DBNull.Value) : new SqlParameter("@To", nMail.To),
                                        string.IsNullOrEmpty(nMail.Cc) ? new SqlParameter("@Ccer",  DBNull.Value) : new SqlParameter("@Ccer",nMail.Cc),
                                        string.IsNullOrEmpty(nMail.AttachmentPath) ? new SqlParameter("@Attachment",  DBNull.Value) :  new SqlParameter("@Attachment", nMail.AttachmentPath),
                                        string.IsNullOrEmpty(alertKey.EID) ?  new SqlParameter("@CreateUser", "Admin") : new SqlParameter("@CreateUser", alertKey.EID)
                                    };
                logger.Info("22222222222222222");
                try {
                    SqlHelperMailAdvisor.ExcuteSql(mailAdvisorSQL, parm);
                } catch (Exception ex) {
                    logger.Error(ex);
                }

                logger.Info("-------------------------(2)MailAdvisor Interface --------------------------" + index);

            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Fail to send mail, LegalEntity:{0}, CustomerNumber:{1}, ToTitle:{2}, CCTtitle:{3}", alertKey.LegalEntity, alertKey.CustomerNum, alertKey.TOTITLE, alertKey.CCTITLE), ex);
            }
            finally {
                string strID = alertKey.ID.Replace(",", "','");
                string strResetSQL = string.Format("Update T_CA_MailAlert set islocked = 0, lockeddate = null WHERE ID in ('{0}')", alertKey.ID);
                SqlHelper.ExecuteNonQuery(System.Data.CommandType.Text, strResetSQL, null);
            }

        }

        public MailTmp RenderInstance(MailTmp instance, string language)
        {
            //soaFlg
            instance.soaFlg = "1";
            instance.MailType = "005" + "," + language;

            return instance;
        }

        /// <summary>
        /// 根据客户生成邮件
        /// </summary>
        /// <param name="customerKey"></param>
        /// <returns></returns>
        private MailTmp generateMailByAlert(CaMailAlertDto alertKey)
        {
            //根据操作人，获得邮件语言模板
            List<string> listId = alertKey.ID.Split(',').ToList();
            string strIdin = alertKey.ID.Replace(",", "','");
            MailTmp nMail = null;
            try
            {
                var nDefaultLanguage = (from ca in CommonRep.GetDbSet<SysTypeDetail>()
                                        where ca.TypeCode == "045" && ca.DetailName == alertKey.EID
                                        select ca.Description).FirstOrDefault().ToString();
                if (string.IsNullOrEmpty(nDefaultLanguage))
                {
                    nDefaultLanguage = "010";
                }
                string strSQL = string.Format("update t_ca_mailalert set comment = '' where id in ('{0}')", strIdin);
                SqlHelper.ExcuteSql(strSQL);

                // 获取联系人
                ContactService service = SpringFactory.GetObjectImpl<ContactService>("ContactService");
                List<ContactorDto> collectorList = new List<ContactorDto>();
                if (alertKey.AlertType == "008")
                {
                    List<string> listBsId = alertKey.BSID.Split(',').ToList();
                    List<string> listSiteUseId = new List<string>();
                    foreach (string id in listBsId)
                    {
                        string strSql = string.Format(@"SELECT distinct SiteUseId FROM T_CA_ReconDetail
                                             WHERE ReconId IN (
                                              select TOP 1 t_ca_recon.ID 
                                              from t_ca_recon 
                                              left join t_ca_reconbs on t_ca_recon.id = t_ca_reconbs.ReconId
                                              where t_ca_reconbs.BANK_STATEMENT_ID = '{0}'
                                              and t_ca_recon.GroupType not like 'UN%'
                                                    and t_ca_recon.GroupType not like 'NM%'
                                              ORDER BY t_ca_recon.CREATE_DATE DESC) ", id);
                        DataTable dt = SqlHelper.ExcuteTable(strSql, System.Data.CommandType.Text, null);
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                listSiteUseId.Add(dt.Rows[0]["SiteUseId"].ToString());
                            }
                        }
                    }
                    string siteUseIdAll = "";
                    if (listSiteUseId.Count > 0)
                    {
                        siteUseIdAll = string.Join(",", listSiteUseId.ToArray());
                    }
                    collectorList = service.GetCaPmtMailContacts(alertKey.LegalEntity, alertKey.CustomerNum, siteUseIdAll, alertKey.TOTITLE, alertKey.CCTITLE, alertKey.EID).ToList();
                }
                else
                {
                    collectorList = service.GetCaPmtMailContacts(alertKey.LegalEntity, alertKey.CustomerNum, "", alertKey.TOTITLE, alertKey.CCTITLE, alertKey.EID).ToList();
                }
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

                var userEmployeeMail = (from user in XcceleratorRep.GetDbSet<T_USER_EMPLOYEE>()
                                        where user.USER_NAME == alertKey.EID
                                        select user.USER_MAIL).FirstOrDefault().ToString();
                //CC中，指定了邮箱地址
                string[] strCC = alertKey.CCTITLE.Split(',');
                foreach (string cc in strCC)
                {
                    if (cc == "Operator")
                    {
                        nCC.Add(userEmployeeMail);
                    }
                    if (cc.IndexOf("@") >= 0)
                    {
                        nCC.Add(cc);
                    }
                }
                string[] strTO = alertKey.TOTITLE.Split(',');
                foreach (string to in strTO)
                {
                    if (to == "Operator")
                    {
                        nTo.Add(userEmployeeMail);
                    }
                    if (to.IndexOf("@") >= 0)
                    {
                        nTo.Add(to);
                    }
                }

                if (nTo.Count == 0)
                {
                    strIdin = alertKey.ID.Replace(",", "','");
                    strSQL = string.Format("update t_ca_mailalert set comment = 'No Contactor', ISLOCKED = 0 where id in ('{0}')", strIdin);
                    SqlHelper.ExcuteSql(strSQL);

                    /*
                    //获得当前EID的邮箱,发送失败通知邮件
                    string strFrom = mailService.GetSenderMailAddressByOperator(alertKey.EID);
                    string strTo = mailService.GetSenderMailAddressByOperator(alertKey.EID);
                    if (userEmployeeMail != null)
                    {
                        strTo = userEmployeeMail;
                        MailTmp mail = new MailTmp();
                        try
                        {
                            string strBody = "";
                            string strSubject = "";
                            if (alertKey.AlertType == "006")
                            {
                                strSubject = "No Contactor - Post Confirm Mail ";
                            }
                            if (alertKey.AlertType == "008")
                            {
                                strSubject = "No Contactor - Clear Confirm Mail";
                            }
                            strBody = "Transaction Number: " + alertKey.TransNumber + "\r\n" +
                                      "Customer Number: " + alertKey.CustomerNum + "\r\n" +
                                      "SiteUseId: " + alertKey.SiteUseId + "\r\n" +
                                      "无" + alertKey.TOTITLE + "联系人";
                            mail.Subject = strSubject;
                            mail.Body = strBody;
                            mail.From = strFrom;
                            mail.To = strTo;
                            mail.Deal = "Arrow";
                            mail.Type = "OUT";
                            mail.Category = "Sent";
                            mail.MailBox = strFrom;
                            mail.CreateTime = DateTime.Now;
                            mail.Operator = "System";
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        //发送Mail
                        MailTmp mailinstance = mailService.SendMail(mail);
                    }
                    */

                    return null;
                }
                //PMT Mail
                if (alertKey.AlertType == "006")
                {
                    nMail = soaService.GetCaPmtMailInstance(alertKey.EID, alertKey.ID, alertKey.BSID, alertKey.LegalEntity, alertKey.CustomerNum, alertKey.SiteUseId, alertKey.AlertType, nDefaultLanguage, alertKey.IndexFile);
                }
                //Clear Confirm mail
                if (alertKey.AlertType == "008")
                {
                    nMail = soaService.GetCaClearMailInstance(alertKey.EID, alertKey.ID, alertKey.BSID, alertKey.LegalEntity, alertKey.CustomerNum, alertKey.SiteUseId, alertKey.AlertType, nDefaultLanguage, alertKey.IndexFile);
                }
                if (nMail == null) {
                    return null;
                }
                nMail = RenderInstance(nMail, nDefaultLanguage); 
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
                if (!string.IsNullOrEmpty(alertKey.EID))
                {
                    Helper.Log.Info("********* alertKey.EID: " + alertKey.EID);
                    var groupMailBox = (from ca in CommonRep.GetDbSet<SysTypeDetail>()
                                        where ca.TypeCode == "045" && ca.DetailName == alertKey.EID
                                        select ca.DetailValue2).FirstOrDefault().ToString();
                    nMail.From = groupMailBox;
                }
            }
            catch (Exception ex)
            {
                Helper.Log.Info(ex.Message);
            }
            finally
            {
                List<string> listSQL = new List<string>();
                foreach (string id in listId)
                {
                    listSQL.Add(string.Format("Update T_CA_MailAlert set islocked = 0, lockeddate = null WHERE id in ('{0}')", strIdin));
                }
                if (listSQL.Count > 0)
                {
                    SqlHelper.ExcuteListSql(listSQL);
                }

            }
            return nMail;
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="mail"></param>
        private int sendMail(MailTmp mail, CaMailAlertDto alertKey)
        {
            try
            {
                return soaService.sendCaPmtMailSaveInfoToDB(mail, alertKey.ID.ToString());
            }
            catch (Exception ex) {
                throw ex;
            }
        }


        /// <summary>
        /// 根据客户获取自动发送邮件的 Invoice Id List 
        /// </summary>
        /// <param name="custNum"></param>
        /// <param name="siteUseId"></param>
        /// <returns></returns>
        private List<int> GetSendInvIdListNew(string strLegalEntity, string alertCustomerNum)
        {
            return soaService.GetCaPmtMailSendInvoice(strLegalEntity, alertCustomerNum);
        }


    }
}
