using Intelligent.OTC.Common.Utils;
using System;
using System.Net;

namespace Intelligent.OTC.Common.Exceptions
{
    public class NoPermissionException : ApplicationException
    {
        public NoPermissionException()
            :base("Permission not enouth.")
        {
            StatusCode = HttpStatusCode.Unauthorized;

            Helper.Log.Error(this.Message, this);
        }

        public HttpStatusCode StatusCode { get; set; }
    }
}
