using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Intelligent.OTC.Common.Utils;
using System.Threading.Tasks;
using System.Net.Http.Formatting;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Domain.Dtos;


namespace Intelligent.OTC.Domain.DataModel
{
    public class MailServiceProxy
    {
        public MailServiceProxy(string mailServiceEndPoint)
        {
            this.mailServiceEndPoint = mailServiceEndPoint;
            controller = "MailService/";
            client = new HttpClient() { Timeout = new TimeSpan(0, 20, 0) };
        }

        HttpClient client = null;
        string mailServiceEndPoint { get; set; }
        string controller { get; set; }

        public List<MailRaw> GetMailRaw(string mailBox, long startFrom)
        {
            AssertUtils.ArgumentHasText(mailBox, "mail box");


            DateTime from = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(startFrom / 1000);
            Helper.Log.Info("Getting mail from mailBox:" + mailBox + ", Start time(UTC): " + from.ToString("yyyy/MM/dd HH:mm:ss"));

            try
            {
                Task<HttpResponseMessage> task = client.PostAsync(mailServiceEndPoint + controller
                    + "GetMailRaw?mailBox=" + mailBox
                    + (from == DateTime.MinValue ? "" : ("&lastMsgTime=" + startFrom)), null);

                task.Wait();

                //task.Result.EnsureSuccessStatusCode();

                if (!task.Result.IsSuccessStatusCode)
                {
                    throw new OTCServiceException(string.Format("[{0}] Failed to get mail. Detailed Error: {1}", mailBox, task.Result.Content.ReadAsStringAsync().Result));
                }

                var res = task.Result.Content.ReadAsAsync<List<MailRaw>>();
                res.Wait();

                return res.Result;
            }
            catch (Exception ex)
            {
                Helper.Log.Error("GetMailRaw method run failed", ex);
                throw;
            }
        }

        public bool RegistMailBox(string authenticateCode, string userMail)
        {
            AssertUtils.ArgumentHasText(authenticateCode, "authenticate Code");
            AssertUtils.ArgumentHasText(userMail, "user mail box");

            //Check Mail Format
            if (!MailFormatCheckHelper.checkMailFormat(userMail)) {
                Helper.Log.Info("mail address invalid");
                return false;
            }

            Task<HttpResponseMessage> task = client.PostAsync(mailServiceEndPoint + controller
                + "RegistMailBox?authenticateCode=" + authenticateCode
                + "&userMail=" + userMail, null);

            task.Wait();

            //task.Result.EnsureSuccessStatusCode();

            if (!task.Result.IsSuccessStatusCode)
            {
                throw new OTCServiceException(string.Format("[{0}] Failed to regist mail box. Detailed Error: {1}", userMail, task.Result.Content.ReadAsStringAsync().Result));
            }

            var res = task.Result.Content.ReadAsAsync<bool>();
            res.Wait();

            return res.Result;
        }
        public bool SendMailInfo(MailMessageDto mMessage)
        {
            AssertUtils.ArgumentNotNull(mMessage, "mmessage");
            //AssertUtils.ArgumentHasText(MMessage.MailBox, "user mail box");

            //mMessage.Sender = "albert_zq@outlook.com";
            mMessage.From = mMessage.Sender;
            Helper.Log.Info("SendMailInfo called: " + mailServiceEndPoint + "Send");

            Task<HttpResponseMessage> task = client.PostAsJsonAsync<MailMessageDto>(mailServiceEndPoint  + "Send", mMessage);

            task.Wait();

            //task.Result.EnsureSuccessStatusCode();

            if (!task.Result.IsSuccessStatusCode)
            {
                throw new OTCServiceException(string.Format("Failed to send mail. Detailed Error: {0}", task.Result.Content.ReadAsStringAsync().Result));
            }
           var xxx = task.Result.Content.ReadAsStringAsync();
            xxx.Wait();
            return true;

            //var res = task.Result.Content.ReadAsAsync<bool>();
            //res.Wait();

            //return res.Result;
        }

        public bool SendMailRaw(MailRaw rawMessage)
        {
            AssertUtils.ArgumentNotNull(rawMessage, "raw message");
            AssertUtils.ArgumentHasText(rawMessage.MailBox, "user mail box");

            Helper.Log.Info("SendMailRaw called: " + mailServiceEndPoint + controller + "SendMailRaw");

            Task<HttpResponseMessage> task = client.PostAsJsonAsync<MailRaw>(mailServiceEndPoint + controller
                + "SendMailRaw", rawMessage);

            task.Wait();

            //task.Result.EnsureSuccessStatusCode();

            if (!task.Result.IsSuccessStatusCode)
            {
                throw new OTCServiceException(string.Format("[{0}] Failed to send mail. Detailed Error: {1}", rawMessage.MailBox, task.Result.Content.ReadAsStringAsync().Result));
            }

            var res = task.Result.Content.ReadAsAsync<bool>();
            res.Wait();

            return res.Result;
        }

        public bool CheckMailBoxInitialized(string userMailBox)
        {
            return true;

            AssertUtils.ArgumentHasText(userMailBox, "user mail box");

            Task<HttpResponseMessage> task = client.PostAsync(mailServiceEndPoint + controller + "CheckMailBoxInitialized?userMailBox=" + userMailBox, null);

            task.Wait();

            //task.Result.EnsureSuccessStatusCode();

            if (!task.Result.IsSuccessStatusCode)
            {
                throw new OTCServiceException(string.Format("[{0}] Failed to send mail. Detailed Error: {1}", userMailBox, task.Result.Content.ReadAsStringAsync().Result));
            }

            var res = task.Result.Content.ReadAsAsync<bool>();
            res.Wait();

            return res.Result;
        }

        public bool MarkAsRead(UserMailInput usermail)
        {
            AssertUtils.ArgumentHasText(usermail.messageId, "message Id");
            AssertUtils.ArgumentHasText(usermail.userName, "user mail box");

            //Task<HttpResponseMessage> task = client.PostAsync(mailServiceEndPoint + controller
            //    + "MarkAsRead?mailBox=" + mailBox
            //    + "&msgId=" + msgId, null);
            Task<HttpResponseMessage> task = client.PostAsJsonAsync<UserMailInput>(mailServiceEndPoint + "MarkAsRead", usermail);


            task.Wait();

            //task.Result.EnsureSuccessStatusCode();

            if (!task.Result.IsSuccessStatusCode)
            {
                throw new OTCServiceException(string.Format("[{0}] Failed to mark mail as read. Detailed Error: {1}", usermail.userName, task.Result.Content.ReadAsStringAsync().Result));
            }

            //var res = task.Result.Content.ReadAsAsync<bool>();
            //res.Wait();

            return true;
        }

        /// <summary>
        /// Get Mail raw as file stream from Mail service.
        /// </summary>
        /// <param name="mailBox"></param>
        /// <param name="startFrom"></param>
        /// <returns></returns>
        public List<MailRaw> GetMailRaw2(string mailBox, long startFrom)
        {
            AssertUtils.ArgumentHasText(mailBox, "mail box");


            DateTime from = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(startFrom / 1000);
            Helper.Log.Info("Getting mail from mailBox:" + mailBox + ", Start time(UTC): " + from.ToString("yyyy/MM/dd HH:mm:ss"));

            Task<HttpResponseMessage> task = client.PostAsync(mailServiceEndPoint + controller
                + "GetMailRawAsFile?mailBox=" + mailBox
                + (from == DateTime.MinValue ? "" : ("&lastMsgTime=" + startFrom)), null);

            task.Wait();

            task.Result.EnsureSuccessStatusCode();

            if (!task.Result.IsSuccessStatusCode)
            {
                Helper.Log.Info(task.Result.Content.ReadAsStringAsync().Result);
                throw new OTCServiceException("Failed to get mail for mailbox:" + mailBox);
            }

            var res = task.Result.Content.ReadAsAsync<List<MailRaw>>();
            res.Wait();

            return res.Result;
        }

        public bool InitMailManagerInstance(string userName) {
            
            Helper.Log.Info("InitMailManagerInstance called: " + mailServiceEndPoint + "InitMailManagerInstance");

            Task<HttpResponseMessage> task = client.PostAsJsonAsync<string>(mailServiceEndPoint + "InitMailManagerInstance", userName);

            task.Wait();
            
            if (!task.Result.IsSuccessStatusCode)
            {
                throw new OTCServiceException(string.Format("Failed to init mail. Detailed Error: {0}", task.Result.Content.ReadAsStringAsync().Result));
            }
            var xxx = task.Result.Content.ReadAsStringAsync();
            xxx.Wait();
            return true;
        }

        public bool FindContactor(SendContactorNameDto finder) {

            Task<HttpResponseMessage> task = client.PostAsJsonAsync<SendContactorNameDto>(mailServiceEndPoint + "FindContactor", finder);


            task.Wait();

            if (!task.Result.IsSuccessStatusCode)
            {
                throw new OTCServiceException(string.Format("[{0}] Failed to FindContactor. Detailed Error: {1}", finder.sender, task.Result.Content.ReadAsStringAsync().Result));
            }

            return true;
        }
    }
}
