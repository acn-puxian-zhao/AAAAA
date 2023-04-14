using Intelligent.OTC.Common.Utils;
using System;

namespace Intelligent.OTC.Common.Exceptions
{
    public class UserNotLoginException : ApplicationException
    {
        public UserNotLoginException()
            : base("User not logged in")
        {
            Helper.Log.Info(this.Message);
        }
    }
}
