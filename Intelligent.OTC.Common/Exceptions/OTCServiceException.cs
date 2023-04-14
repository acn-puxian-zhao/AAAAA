using Intelligent.OTC.Common.Utils;
using System;
using System.Net;

namespace Intelligent.OTC.Common.Exceptions
{
    public class OTCServiceException : ApplicationException
    {
        public OTCServiceException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
            :base(message)
        {
            StatusCode = statusCode;
            Helper.Log.Info(message);
        }

        public HttpStatusCode StatusCode { get; set; }
    }
}
