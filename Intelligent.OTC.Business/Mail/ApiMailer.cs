using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.Repository;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;


namespace Intelligent.OTC.Business
{
    public class ApiMailer: IMailer
    {
        public void MarkAsRead(UserMailInput usermail)
        {
            if (ConfigurationManager.AppSettings["isMail"] == "true")
            {
                MailServiceProxy proxy = new MailServiceProxy(ConfigurationManager.AppSettings["MailserviceEndPoint"]);
                proxy.MarkAsRead(usermail);
            }
        }

        public List<MailRaw> GetMailRaw(string mailBox, long lastMsgTime)
        {
            MailServiceProxy proxy = new MailServiceProxy(ConfigurationManager.AppSettings["MailserviceEndPoint"]);
            List<MailRaw> raws = new List<MailRaw>();
            raws = proxy.GetMailRaw(mailBox, lastMsgTime);

            return raws;
        }

        public bool SendMailRaw(MailRaw rawMessage)
        {
            if (ConfigurationManager.AppSettings["IsMail"] == "true")
            {
                MailServiceProxy proxy = new MailServiceProxy(ConfigurationManager.AppSettings["MailserviceEndPoint"]);
                return proxy.SendMailRaw(rawMessage);
            }
            else
            {
                return true;
            }
        }

        public bool SendMailInfo(MailMessageDto mMessage)
        {

            if (ConfigurationManager.AppSettings["IsMail"] == "true")
            {
                MailServiceProxy proxy = new MailServiceProxy(ConfigurationManager.AppSettings["MailserviceEndPoint"]);
                return proxy.SendMailInfo(mMessage);
            }
            else
            {
                return true;
            }
        }

        public bool CheckMailBoxInitialized(string userMailBox)
        {
            MailServiceProxy proxy = new MailServiceProxy(ConfigurationManager.AppSettings["MailserviceEndPoint"]);
            return proxy.CheckMailBoxInitialized(userMailBox);
        }

        public bool InitMailManagerInstance(string userName)
        {
            MailServiceProxy proxy = new MailServiceProxy(ConfigurationManager.AppSettings["MailserviceEndPoint"]);
            return proxy.InitMailManagerInstance(userName);
        }

        public void FindContactor(SendContactorNameDto finder)
        {
            if (ConfigurationManager.AppSettings["isMail"] == "true")
            {
                MailServiceProxy proxy = new MailServiceProxy(ConfigurationManager.AppSettings["MailserviceEndPoint"]);
                proxy.FindContactor(finder);
            }
        }
    }
}
