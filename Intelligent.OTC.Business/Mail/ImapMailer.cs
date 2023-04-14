using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Domain.DataModel;
using Intelligent.OTC.Domain.Dtos;
using System;
using System.Collections.Generic;

namespace Intelligent.OTC.Business
{
    public class ImapMailer: IMailer
    {
        public void MarkAsRead(UserMailInput usermail)
        {
            throw new NotImplementedException();
        }

        public List<MailRaw> GetMailRaw(string mailBox, long startFrom)
        {
            Exception ex = new NotImplementedException();
            Helper.Log.Error(ex.Message, ex);
            throw ex;
        }

        public bool SendMailRaw(MailRaw rawMessage)
        {
            Exception ex = new NotImplementedException();
            Helper.Log.Error(ex.Message, ex);
            throw ex;
        }
        public bool SendMailInfo(MailMessageDto mMessage)
        {
            Exception ex = new NotImplementedException();
            Helper.Log.Error(ex.Message, ex);
            throw ex;
        }

        public bool CheckMailBoxInitialized(string userMailBox)
        {
            Exception ex = new NotImplementedException();
            Helper.Log.Error(ex.Message, ex);
            throw ex;
        }

        public bool InitMailManagerInstance(string userName)
        {
            Exception ex = new NotImplementedException();
            Helper.Log.Error(ex.Message, ex);
            throw ex;
        }

        public void FindContactor(SendContactorNameDto finder)
        {
            throw new NotImplementedException();
        }
    }
}
