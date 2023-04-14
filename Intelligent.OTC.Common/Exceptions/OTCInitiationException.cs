using Intelligent.OTC.Common.Utils;
using System;

namespace Intelligent.OTC.Common.Exceptions
{
    public class OTCInitiationException: ApplicationException
    {
        public OTCInitiationException(string msg)
            :base(msg)
        {
            Helper.Log.Error(msg, this);
        }
    }
}
