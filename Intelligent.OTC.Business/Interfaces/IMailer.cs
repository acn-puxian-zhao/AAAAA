using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;

namespace Intelligent.OTC.Business
{
    public interface IMailer
    {
        /// <summary>
        /// Mark the mail as read.
        /// </summary>
        /// <param name="mailBox"></param>
        /// <param name="msgId"></param>
        void MarkAsRead(UserMailInput usermail);

        /// <summary>
        /// Get the mail in raw formats.
        /// </summary>
        /// <param name="mailBox"></param>
        /// <param name="startFrom"></param>
        /// <returns></returns>
        List<MailRaw> GetMailRaw(string mailBox, long startFrom);

        /// <summary>
        /// Send the mail in raw formats.
        /// </summary>
        /// <param name="rawMessage"></param>
        /// <returns></returns>
        bool SendMailRaw(MailRaw rawMessage);

        /// <summary>
        /// Check if user have completed the initialization of their mail box.
        /// </summary>
        /// <param name="userMailBox"></param>
        /// <returns></returns>
        bool CheckMailBoxInitialized(string userMailBox);
        ///<summary>
        ///
        bool SendMailInfo(MailMessageDto mMessage);

        bool InitMailManagerInstance(string userName);

        void FindContactor(SendContactorNameDto finder);
    }
}
