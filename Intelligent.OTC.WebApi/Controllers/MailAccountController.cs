using Intelligent.OTC.Business.Interfaces;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using System;
using System.Configuration;
using System.Linq;
using System.Web.Http;

namespace Intelligent.OTC.WebApi.Controllers
{
    public class MailAccountController : ApiController
    {
        [HttpPost]
        [Route("api/mail/saveMailAccount")]
        public string SaveMailAccount(T_MailAccount mailAccount)
        {
            if (String.IsNullOrEmpty(mailAccount.UserName))
            {
                return "Please Input UserName";
            }

            if (String.IsNullOrEmpty(mailAccount.UserId))
            {
                return "Please Input Old Password";
            }

            if (String.IsNullOrEmpty(mailAccount.Password))
            {
                return "Please Input New Password";
            }

            bool isMailGroup = Convert.ToBoolean(ConfigurationManager.AppSettings["IsEnableGroupMail"]);
            if (isMailGroup)
            {
                IMailAccountService mailAccountService = SpringFactory.GetObjectImpl<IMailAccountService>("MailAccountService");
                T_MailAccount mailAccountOld = mailAccountService.GetMailAccountBySendMailAddress(mailAccount.UserName).FirstOrDefault();
                if (mailAccountOld == null)
                {
                    return "UserName is not invalid.";
                }
                if (AESUtil.AESDecrypt(mailAccountOld.Password) != mailAccount.UserId) {
                    return "Old Password is not invalid.";
                }
                mailAccountOld.Password = AESUtil.AESEncrypt(mailAccount.Password);
                mailAccountService.UpdateMailAccount(mailAccountOld);
                return "Save success.";
            }
            else
            { 
                IMailAccountService mailAccountService = SpringFactory.GetObjectImpl<IMailAccountService>("MailAccountService");
                var mailAccountQ = mailAccountService.GetAllMailAccount(AppContext.Current.User.EID);
                //该登录用户没有邮箱
                if (mailAccountQ.Count() == 0)
                {
                    string mailDomain = mailAccount.UserName.Substring(mailAccount.UserName.IndexOf("@") + 1);
                    IMailServerService mailServerService = SpringFactory.GetObjectImpl<IMailServerService>("MailServerService");
                    var mailServerList = mailServerService.GetMailServer(mailDomain);
                    //Domain存在
                    if (mailServerList.Count() > 0)
                    {
                        T_MailServer mailServer = mailServerList.ToList()[0];
                        mailAccount.ServerId = mailServer.Id;
                        mailAccount.UserId = AppContext.Current.User.EID;
                        return mailAccountService.AddMailAccount(mailAccount);
                    }
                    else
                    {
                        return "MailDomain is not exist";
                    }
                }
                else
                {
                    string mailDomain = mailAccount.UserName.Substring(mailAccount.UserName.IndexOf("@") + 1);
                    IMailServerService mailServerService = SpringFactory.GetObjectImpl<IMailServerService>("MailServerService");
                    var mailServerList = mailServerService.GetMailServer(mailDomain);
                    //Domain存在
                    if (mailServerList.Count() > 0)
                    {
                        T_MailServer mailServer = mailServerList.ToList()[0];
                        mailAccount.Id = mailAccountQ.First().Id;
                        mailAccount.ServerId = mailServer.Id;
                        mailAccount.UserId = AppContext.Current.User.EID;
                        return mailAccountService.UpdateMailAccount(mailAccount);
                    }
                    else
                    {
                        return "MailDomain is not exist";
                    }
                }
            }
        }

        [HttpPost]
        [Route("api/mail/getMailAccount")]
        public T_MailAccount GetMailAccount()
        {
            IMailAccountService mailAccountService = SpringFactory.GetObjectImpl<IMailAccountService>("MailAccountService");
            return mailAccountService.GetAllMailAccount(AppContext.Current.User.EID).FirstOrDefault();
        }
    }
}
