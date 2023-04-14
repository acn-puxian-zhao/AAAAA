using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Intelligent.OTC.MailService.Controllers
{
    public class MailServiceController : Controller
    {
        /// <summary>
        /// Used for post release test porpuse.
        /// </summary>
        /// <returns></returns>
        public ActionResult GetTest()
        {
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="authenticateCode"></param>
        /// <param name="userMail"></param>
        /// <returns></returns>
        public ActionResult RegistMailBox(string authenticateCode, string userMail)
        {
            try
            {
                AssertUtils.ArgumentHasText(authenticateCode, "User Authentication Code");
                AssertUtils.ArgumentHasText(userMail, "User Mail Box");

                if (checkMailBoxInitialized(userMail))
                {
                    // do nothing if mail box already initialized.
                    return Json(true);
                }

                Helper.Log.Info(string.Format("Registing mail box: {0}", userMail));

                string url = ConfigurationManager.AppSettings["GoogleAuth"];
                string clientId = ConfigurationManager.AppSettings["ClientId"];
                string clientSecret = ConfigurationManager.AppSettings["ClientSecret"];
                string postData = string.Format("code={0}&client_id={1}&client_secret={2}&redirect_uri=urn:ietf:wg:oauth:2.0:oob&grant_type=authorization_code", authenticateCode, clientId, clientSecret);

                // Create a request using a URL that can receive a post. 
                WebRequest request = WebRequest.Create(url);
                // Set the Method property of the request to POST.

                request.Method = "POST";

                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                // Set the ContentType property of the WebRequest.
                request.ContentType = "application/x-www-form-urlencoded";
                // Set the ContentLength property of the WebRequest.
                request.ContentLength = byteArray.Length;
                // Get the request stream.
                Stream dataStream = request.GetRequestStream();
                // Write the data to the request stream.
                dataStream.Write(byteArray, 0, byteArray.Length);
                // Close the Stream object.
                dataStream.Close();
                // Get the response.
                WebResponse response = request.GetResponse();
                // Display the status.
               
                // Get the stream containing content returned by the server.
                dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                string responseFromServer = reader.ReadToEnd();

                // save the token file into FileDataStore
                string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                Helper.Log.Info("ApplicationData folder: " + path);
                using (StreamWriter sw = System.IO.File.CreateText(path + "\\Google.Apis.Auth\\Google.Apis.Auth.OAuth2.Responses.TokenResponse-" + userMail))
                {
                    sw.WriteLine(responseFromServer);
                }

                TokenResponse tmp = JsonConvert.DeserializeObject<TokenResponse>(responseFromServer);

                // Clean up the streams.
                reader.Close();
                dataStream.Close();
                response.Close();

                Helper.Log.Info(string.Format("Regist mail box: {0} complete.", userMail));
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                return Json(false);
            }

            return Json(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mailBox"></param>
        /// <param name="lastMsgTime">
        /// The start time of the retrieving message.
        /// Timestamp(UTC). If it is the last received message time, it will be UTC. 
        /// For the first time run(No existing last message time), we need to provide reasonable start time(Normally, today)</param>
        /// <returns></returns>
        public ActionResult GetMailRaw(string mailBox, long lastMsgTime)
        {
            // check if the from have complete the user initialization
            if (!checkMailBoxInitialized(mailBox))
            {
                Exception ex = new Exception(string.Format("[{0}]:User initialization has not been done.", mailBox));
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }

            Helper.Log.Info(string.Format("[{0}]:Start to retrieve mail.", mailBox));

            List<MailRaw> mailList = new List<MailRaw>();

            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                FileDataStore store = new FileDataStore(path + "\\Google.Apis.Auth");

                // extract seconds from the lastMsgTime(millean seconds)
                DateTime startFrom = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(lastMsgTime / 1000);

                var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    new ClientSecrets
                    {
                        ClientId = ConfigurationManager.AppSettings["ClientId"],
                        ClientSecret = ConfigurationManager.AppSettings["ClientSecret"],
                    },
                    new[] { GmailService.Scope.MailGoogleCom },
                    mailBox,
                    CancellationToken.None,
                    store).Result;

                var service = new GmailService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Intelligent OTC",
                });

                var request = service.Users.Messages.List("me");
                request.LabelIds = "INBOX";
                // The Q do not support datetime.
		        // https://developers.google.com/gmail/api/v1/reference/users/messages/list#try-it
                // https://support.google.com/mail/answer/7190?hl=en
                request.Q = string.Format("after:{0}", startFrom.AddHours(-8).ToString("yyyy/MM/d"));
                // The fields is not working for messages action.
                // request.Fields = "messages(id,internalDate),nextPageToken,resultSizeEstimate";

                var maxRetrieveCount = Convert.ToInt32(ConfigurationManager.AppSettings["MaxRetrieveCount"]);

                Helper.Log.Info(string.Format("[{0}]:request Excuted. with query condition: [{1}] ", mailBox, request.Q));

                List<string> messageIds = getEligibleMessageIds(lastMsgTime, service, request, mailBox);

                if (maxRetrieveCount > 0)
                {
                    messageIds = messageIds.Take(maxRetrieveCount).ToList();
                }

                Helper.Log.Info("[" + mailBox + "]:Found messages count: [" + messageIds.Count + "] with query condition: [" + request.Q + "] and maxRetriveCount: [" + maxRetrieveCount + "]");

                messageIds.ForEach(id =>
                {
                    var getReq = service.Users.Messages.Get("me", id);
                    getReq.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Raw;
                    Message msg = getReq.Execute();
                    mailList.Add(new MailRaw() { RawMsg = msg.Raw, InternalTime = msg.InternalDate.Value, MessageId = msg.Id });
                });


                Helper.Log.Info(string.Format(
                    "[{1}]:[{0}] mails retrieved with the filter: ['Internal Data > last msg time({2}(UTC:{3}))]'", 
                    mailList.Count, mailBox, lastMsgTime, startFrom.ToString("yyyy/MM/dd HH:mm:ss")));
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw;
            }
            return new LargeJsonResult() { Data = mailList, MaxJsonLength = int.MaxValue };
        }

        private static Dictionary<string, long> internalDateTmp = new Dictionary<string, long>();

        /// <summary>
        /// try to get all message internal dates. With internal dates, We can get a ordered list for the mails we will retrieve in the following actions.
        /// We have to go for this method. Becuase the prefered way (by using "field: messages(id, internalDate)" in the request) is not working for messages action (threads action seems works)
        /// For details, please theck the http://stackoverflow.com/questions/25484791/gmail-api-users-messages-list
        /// </summary>
        /// <param name="lastMsgTime"></param>
        /// <param name="service"></param>
        /// <param name="request"></param>
        /// <param name="maxResultCount"></param>
        /// <returns></returns>
        private static List<string> getEligibleMessageIds(long lastMsgTime, GmailService service, UsersResource.MessagesResource.ListRequest request, string mailBox)
        {
            Dictionary<string, long> tmp = new Dictionary<string, long>();
            List<string> res = new List<string>();

            // This is the bad solution, bu we have no choices!!
            // First, Gmail api do not support order by internal dates. Also, The results from messages action do not ensure the order of that results which means we have to go through all messages and get the ordered list for owerselfs.
            do
            {
                request.MaxResults = 1000;
                var response = request.Execute();
                request.PageToken = response.NextPageToken;
                try
                {
                    if (response.Messages == null && response.Messages.Count == 0)
                    {
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    Helper.Log.Error(ex.Message, ex);
                    continue;
                }

                foreach (var mes in response.Messages)
                {
                    long internalDate  = 0;

                    if (!internalDateTmp.ContainsKey(mailBox + mes.Id))
                    {
                        var getReq = service.Users.Messages.Get("me", mes.Id);
                        getReq.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Minimal;
                        Message msg = getReq.Execute();
                        if (msg.InternalDate != null)
                        {
                            internalDateTmp.Add(mailBox + mes.Id, msg.InternalDate.Value);
                        }
                        else
                        {
                            internalDateTmp.Add(mailBox + mes.Id, 0);
                        }
                    }
                    internalDate = internalDateTmp[mailBox + mes.Id];

                    //TODO: Protential risk. If their are mail with the same internal date with the lastMsgTime in DB, The mail will be skip!
                    if (internalDate > lastMsgTime)
                    {
                        tmp.Add(mes.Id, internalDate);
                    }
                }
            } while (!String.IsNullOrEmpty(request.PageToken));

            // order by internal date
            foreach (KeyValuePair<string, long> kv in tmp.OrderBy(kv => kv.Value))
            {
                res.Add(kv.Key);
            }

            return res;
        }

        public ActionResult MarkAsRead(string mailBox, string msgId)
        {
            if (!checkMailBoxInitialized(mailBox))
            {
                Exception ex = new Exception("User initialization has not been done.");
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            FileDataStore store = new FileDataStore(path + "\\Google.Apis.Auth");

            GmailService service = null;
            try
            {
                // create credential of the service account that has the permission to access the domain resources
                var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    new ClientSecrets
                    {
                        ClientId = ConfigurationManager.AppSettings["ClientId"],
                        ClientSecret = ConfigurationManager.AppSettings["ClientSecret"],
                    },
                    new[] { GmailService.Scope.MailGoogleCom },
                    mailBox,
                    CancellationToken.None,
                    store).Result;

                service = new GmailService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Gmail API",
                });

            }
            catch
            {
                Helper.Log.Info("Credential update error");
                throw;
            }


            // 1, Get all mails from inbox
            var request = service.Users.Messages.Get("me", msgId);
            request.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Minimal;
            Message msg = request.Execute();

            var markAsReadRequest = new ModifyThreadRequest { RemoveLabelIds = new[] { "UNREAD" } };
            service.Users.Threads.Modify(markAsReadRequest, "me", msg.ThreadId).Execute();

            return Json(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mailBox"></param>
        /// <param name="lastMsgTime"></param>
        /// <returns></returns>
        public FileStreamResult GetMailRawAsFile(string mailBox, long lastMsgTime)
        {
            JsonResult res = GetMailRaw(mailBox, lastMsgTime) as JsonResult;

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string dataStr = serializer.Serialize(res.Data);
            StringReader sr = new StringReader(dataStr);
            
            MemoryStream ms = new MemoryStream(ASCIIEncoding.Default.GetBytes(dataStr));

            return File(ms, "application/json");
        }

        public ActionResult CheckMailBoxInitialized(string userMailBox)
        {
            AssertUtils.ArgumentHasText(userMailBox, "user mail box");

            return Json(checkMailBoxInitialized(userMailBox));
        }

        private bool checkMailBoxInitialized(string userMailBox)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            if (!System.IO.File.Exists(path + "\\Google.Apis.Auth\\Google.Apis.Auth.OAuth2.Responses.TokenResponse-" + userMailBox))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mailBox"></param>
        /// <param name="rawMessage">must be base64 encoded but also web safe and also initially UTF8</param>
        public ActionResult SendMailRaw(MailRaw rawMessage)
        {
            try
            {
                AssertUtils.ArgumentNotNull(rawMessage, "message");

                if (!checkMailBoxInitialized(rawMessage.MailBox))
                {
                    Exception ex = new Exception("User initialization has not been done.");
                    Helper.Log.Error(ex.Message, ex);
                    throw ex;
                }

                Helper.Log.Info(string.Format("Start to send mail. From: {0}", rawMessage.MailBox));

                string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                FileDataStore store = new FileDataStore(path + "\\Google.Apis.Auth");

                GmailService service = null;
                try
                {
                    // create credential of the service account that has the permission to access the domain resources
                    var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        new ClientSecrets
                        {
                            ClientId = ConfigurationManager.AppSettings["ClientId"],
                            ClientSecret = ConfigurationManager.AppSettings["ClientSecret"],
                        },
                        new[] { GmailService.Scope.MailGoogleCom },
                        rawMessage.MailBox,
                        CancellationToken.None,
                        store).Result;

                    service = new GmailService(new BaseClientService.Initializer
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "Gmail API",
                    });

                }
                catch
                {
                    Helper.Log.Info("Credential update error");
                    throw;
                }

                service.Users.Messages.Send(new Message { Raw = rawMessage.RawMsg }, "me").Execute();

                Helper.Log.Info(string.Format("Complete to send mail. From: {0}", rawMessage.MailBox));
            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw;
            }

            return Json(true);
        }

    }
}