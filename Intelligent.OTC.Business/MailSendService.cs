using Intelligent.OTC.Business.Interfaces;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using Intelligent.OTC.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Intelligent.OTC.Business
{
    public class MailSendService : IMailSendService
    {
        public OTCRepository CommonRep { get; set; }

        IMailService mailService = SpringFactory.GetObjectImpl<IMailService>("MailService");

        public void sendTaskMail(string taskId)
        {
            sendTaskMail(taskId, AppContext.Current.User.EID, AppContext.Current.User.Email, "");
        }

        public void sendTaskMail(string taskId, string userId, string email, string aAttachment)
        {
            //查询Task信息
            CaTaskService caTaskService = SpringFactory.GetObjectImpl<CaTaskService>("CaTaskService");
            XcceleratorService collecotr = SpringFactory.GetObjectImpl<XcceleratorService>("XcceleratorService");

            CaTaskDto task = caTaskService.getCaTaskById(taskId);

            //获取MailTemplate
            MailTemplate mailTemplate = mailService.GetMailTemplatebytype("011", "001");//发送task的mail

            string senderMailbox = mailService.GetWarningSenderMailAddress(email);
            string deal = ConfigurationManager.AppSettings["BatchDeal"].ToString();
            //创建Mail
            MailTmp mail = new MailTmp();

            if (mailTemplate != null)
            {
                List<string> toList = new List<string>();
                List<string> ccList = new List<string>();
                try
                {
                    mail = mailService.GetInstanceFromTemplate(mailTemplate, (parser) =>
                    {
                        parser.RegistContext("ContactName", task.CreateUser);
                        parser.RegistContext("collector", userId);//定义signature用的
                        parser.RegistContext("templatelang", mailTemplate.Language);//定义signature用的
                        string customermessage = "It is hereby announced that ";
                        string taskType = task.TaskType;//taks.Type
                        switch (taskType)//根据不同的task类型，拼装不同正文内容
                        {
                            case "1":
                                customermessage += "the BS task you created named ";
                                break;
                            case "2":
                                customermessage += "the PMT Detail task you created named ";
                                break;
                            case "3":
                                customermessage += "the indifity task you created named ";
                                break;
                            case "4":
                                customermessage += "the unkown task you created named ";
                                break;
                            case "5":
                                customermessage += "the recon task you created named ";
                                break;
                            case "7":
                                customermessage += "the auto recon task you created named ";
                                break;
                            default:
                                customermessage += "the task you created named ";
                                parser.RegistContext("taskType", "Task");
                                break;
                        }
                        customermessage = customermessage + task.TaskName + " has been completed. Please check it.";
                        parser.RegistContext("customerMessage", customermessage);

                    }, null,"");


                    T_MailAccount mailAccount = GetMailAccountByUserId(task.CreateUser).FirstOrDefault();
                    toList.Add(mailAccount.UserName);//test
                    mail.Subject = mailTemplate.Subject;
                    mail.From = senderMailbox;
                    mail.To = string.Join(";", toList);
                    mail.Deal = deal;
                    mail.Type = "OUT";
                    mail.Category = "Sent";
                    mail.MailBox = senderMailbox;
                    mail.CreateTime = DateTime.Now;
                    mail.Operator = "System";
                    if (!string.IsNullOrEmpty(aAttachment))
                    {
                        mail.Attachment = aAttachment;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            //发送Mail
            mailService.SendMail(mail);
        }


        public void sendTaskFinishedMail(string strBody, string strEid)
        {
            sendTaskFinishedMail(strBody, strEid, "");
        }

        public void sendTaskFinishedMail(string strBody, string strEid, string fileId)
        {
            string senderMailbox = mailService.GetSenderMailAddressByOperator(strEid);
            string deal = ConfigurationManager.AppSettings["BatchDeal"].ToString();
            //创建Mail
            MailTmp mail = new MailTmp();
            string strTo = "";
            XcceleratorService user = SpringFactory.GetObjectImpl<XcceleratorService>("XcceleratorService");
            T_USER_EMPLOYEE userEmployee = user.GetUserOrganization(strEid);
            if (userEmployee != null)
            {
                strTo = userEmployee.USER_MAIL;
            }
            try
            {
                mail.Deal = deal;
                mail.Subject = "Cash Application Task Finished Alert";
                mail.Body = strBody;
                mail.From = senderMailbox;
                mail.To = strTo;
                mail.Type = "OUT";
                mail.Category = "Sent";
                mail.MailBox = senderMailbox;
                mail.CreateTime = DateTime.Now;
                mail.Operator = "System";
                if (!string.IsNullOrEmpty(fileId))
                {
                    mail.Attachment = fileId;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            //发送Mail
            MailTmp mailinstance = mailService.SendMail(mail);

        }

        public void sendCustomerBankMail(CaBankStatementDto bank, CustomerMenuDto customer)
        {
            //根据操作人，获得邮件语言模板
            string strEID = AppContext.Current.User.EID;
            var nDefaultLanguage = (from ca in CommonRep.GetDbSet<SysTypeDetail>()
                                    where ca.TypeCode == "045" && ca.DetailName == strEID
                                    select ca.Description).FirstOrDefault().ToString();
            if (string.IsNullOrEmpty(nDefaultLanguage))
            {
                nDefaultLanguage = "010";
            }

            string senderMailbox = mailService.GetSenderMailAddressByOperator(strEID);
            string deal = ConfigurationManager.AppSettings["BatchDeal"].ToString(); ;
            //创建Mail
            MailTmp mail = new MailTmp();
            MailTemplate tpl = null;
            IMailService ms = SpringFactory.GetObjectImpl<IMailService>("MailService");
            tpl = ms.GetMailTemplatebytype("007", nDefaultLanguage);

            List<string> toList = new List<string>();
            List<string> ccList = new List<string>();
            try
            {
                if (tpl != null)
                {
                    try
                    {
                        mail = ms.GetInstanceFromTemplate(tpl, (parser) =>
                        {
                            
                            //生成附件并取得附近名和币种的合计值
                            System.Data.DataTable reportItemList = new System.Data.DataTable();
                            if (customer.OnlyReconResult == true)
                            {
                                //唯一结果
                                string tablesql = string.Format(@"SELECT T_CUSTOMER.CUSTOMER_NAME AS CustomerNAME
                                                                        ,T_CUSTOMER.CUSTOMER_NUM AS AccntNumber
                                                                        ,T_CUSTOMER.SiteUseId AS SiteUseId
                                                                        ,T_INVOICE_AGING.SellingLocationCode AS SellingLocationCode
                                                                        ,T_INVOICE_AGING.CLASS AS CLASS
                                                                        ,T_INVOICE_AGING.INVOICE_NUM AS TrxNum
                                                                        ,CONVERT(VARCHAR(10), T_INVOICE_AGING.INVOICE_DATE,120) AS TrxDate
                                                                        ,CONVERT(VARCHAR(10), T_INVOICE_AGING.DUE_DATE,120) AS DueDate
                                                                        ,T_INVOICE_AGING.CREDIT_TREM AS PaymentTermName
                                                                        ,T_INVOICE_AGING.CreditLmt AS OverCreditLmt
                                                                        ,T_INVOICE_AGING.FuncCurrCode AS FuncCurrCode
                                                                        ,T_INVOICE_AGING.CURRENCY AS InvCurrCode
                                                                        ,T_INVOICE_AGING.FsrNameHist AS SalesName
                                                                        ,T_INVOICE_AGING.DAYS_LATE_SYS AS DueDays
                                                                        ,T_INVOICE_AGING.BALANCE_AMT AS AmtRemaining
                                                                        ,T_INVOICE_VAT.VATInvoiceTotalAmount AS AmountWoVat
                                                                        ,T_INVOICE_AGING.AgingBucket AS AgingBucket
                                                                        ,T_CUSTOMER.CREDIT_TREM AS PaymentTermDesc
                                                                        ,T_INVOICE_AGING.SellingLocationCode2 AS SellingLocationCode2
                                                                        , T_CUSTOMER.Ebname AS Ebname
                                                                   FROM T_CA_ReconDetail with (nolock)
                                                                        LEFT JOIN T_CUSTOMER with (nolock) ON T_CA_ReconDetail.SiteUseId = T_CUSTOMER.SiteUseId
                                                                        LEFT JOIN T_INVOICE_AGING with (nolock) ON T_CA_ReconDetail.SiteUseId = T_INVOICE_AGING.SiteUseId AND T_CA_ReconDetail.InvoiceNum = T_INVOICE_AGING.INVOICE_NUM
                                                                        LEFT JOIN T_INVOICE_VAT with (nolock) ON T_INVOICE_VAT.Trx_Number = T_INVOICE_AGING.INVOICE_NUM
                                                                        WHERE ReconId = '{0}'", customer.ReconId);
                                reportItemList = SqlHelper.ExcuteTable(tablesql, System.Data.CommandType.Text, null);
                            }
                            else {
                                //不唯一结果
                                string tablesql = string.Format(@"SELECT T_CUSTOMER.CUSTOMER_NAME AS CustomerNAME
                                                                        ,T_CUSTOMER.CUSTOMER_NUM AS AccntNumber
                                                                        ,T_CUSTOMER.SiteUseId AS SiteUseId
                                                                         FROM dbo.T_CA_CustomerIdentify with (nolock)
                                                                        LEFT JOIN T_CUSTOMER with (nolock) ON T_CA_CustomerIdentify.CUSTOMER_NUM = T_CUSTOMER.CUSTOMER_NUM
                                                                        WHERE T_CA_CustomerIdentify.id = '{0}'", customer.Id);
                                reportItemList = SqlHelper.ExcuteTable(tablesql, System.Data.CommandType.Text, null);

                            }
                            //正文表格
                            string reportStr = "";
                            if (reportItemList != null)
                            {
                                if (reportItemList.Rows.Count > 0)
                                {
                                    InvoiceService invServ = SpringFactory.GetObjectImpl<InvoiceService>("InvoiceService");
                                    reportStr += invServ.GetHTMLTableByDataTable(reportItemList);
                                }
                            }
                            DateTime dtValueDate = bank.VALUE_DATE == null ? DateTime.Now : Convert.ToDateTime(bank.VALUE_DATE);
                            parser.RegistContext("Collector", AppContext.Current.User.EID);
                            parser.RegistContext("templatelang", nDefaultLanguage);
                            parser.RegistContext("attachmentInfo", reportStr);
                            parser.RegistContext("CurrentDate", dtValueDate.ToString("MM/dd/yyyy"));
                        });
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                }
                else
                {
                    Helper.Log.Info("------------------------- tpl is null");
                }
                //查询收件人列表
                string queryTosql = string.Format(@"SELECT distinct EMAIL_ADDRESS as EmailAddress
                                                          FROM V_CA_Contactor 
                                                          WHERE CUSTOMER_NUM = '{0}' and title = 'CS'", customer.CustomerNum);
                List<CaContactorDto> toContactorList = SqlHelper.GetList<CaContactorDto>(SqlHelper.ExcuteTable(queryTosql.ToString(), System.Data.CommandType.Text)); ;
                foreach (CaContactorDto c in toContactorList)
                {
                    toList.Add(c.EmailAddress);
                }
                string queryCcsql = string.Format(@"SELECT distinct EMAIL_ADDRESS as EmailAddress
                                                          FROM V_CA_Contactor 
                                                          WHERE CUSTOMER_NUM = '{0}' and title = 'Sales'", customer.CustomerNum);
                List<CaContactorDto> ccContactorList = SqlHelper.GetList<CaContactorDto>(SqlHelper.ExcuteTable(queryCcsql.ToString(), System.Data.CommandType.Text)); ;
                foreach (CaContactorDto c in ccContactorList)
                {
                    ccList.Add(c.EmailAddress);
                }

                //cc增加当前操作人
                ccList.Add(AppContext.Current.User.Email);

                mail.From = senderMailbox;
                mail.To = string.Join(";", toList);
                mail.Cc = string.Join(";", ccList);
                mail.Deal = deal;
                mail.Type = "OUT";
                mail.Category = "Sent";
                mail.MailBox = senderMailbox;
                mail.CreateTime = DateTime.Now;
                mail.Operator = "System";
            }
            catch (Exception ex)
            {
                throw ex;
            }


            //发送Mail
            MailTmp mailinstance = mailService.SendMail(mail);

            //更新发送Mail时间和MailId
            string strUpdateSQL = string.Format(@"Update T_CA_CustomerIdentify set MailId = '{0}',MailDate=getdate() where id = '{1}'", mailinstance.MessageId, customer.Id);
            SqlHelper.ExcuteSql(strUpdateSQL);
        }

        private IQueryable<T_MailAccount> GetMailAccountByUserId(string strUserId)
        {
            var result = CommonRep.GetQueryable<T_MailAccount>().Where(u => u.UserId == strUserId);
            return result;
        }
    }
}
