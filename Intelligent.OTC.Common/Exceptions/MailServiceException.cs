using Intelligent.OTC.Common.Utils;
using System;

namespace Intelligent.OTC.Common.Exceptions
{
    public class MailServiceException : ApplicationException
    {
        public MailServiceException()
        {

        }

        public MailServiceException(Exception innerException)
            :base("Mail service failed to execute", innerException)
        {
            Helper.Log.Error(innerException.Message, innerException);
        }
    }
}
