using Intelligent.OTC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Intelligent.OTC.Common.Exceptions
{
    public class AgingImportException : ApplicationException
    {
        public AgingImportException(string message)
            :base(message)
        {
            Helper.Log.Error(message, this);
        }

        public AgingImportException(string message, Exception exception)
            : base(message, exception)
        {
            Helper.Log.Error(message, exception);
        }

        public AgingImportException(string message, Exception exception, bool isSuc)
            : base(message, exception)
        {
            isSuccess = isSuc;
            Helper.Log.Error(message, exception);
        }

        public AgingImportException(string message, bool isSuc)
            : base(message)
        {
            isSuccess = isSuc;
            Helper.Log.Error(message, this);
        }

        public List<string> CustomerNums { get; set; }

        public bool isSuccess { get; set; }

    }
}
