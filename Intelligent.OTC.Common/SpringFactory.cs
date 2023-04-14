using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.Utils;
using Spring.Context;
using Spring.Context.Support;
using System;

namespace Intelligent.OTC.Common
{
    public class SpringFactory
    {
        private static IApplicationContext _applicationContext;

        static SpringFactory()
        {
            try
            {
                _applicationContext = ContextRegistry.GetContext();

            }
            catch (Exception ex)
            {
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        public static IApplicationContext ApplicationContext
        {
            get { return _applicationContext; }
        }



        public static T GetObjectImpl<T>(string key)
        {
            if (_applicationContext == null)
            {
                Exception ex = new OTCInitiationException("The spring context is under the process of creation and cannot be called from within the process." + Environment.NewLine
                    + "you may see this exception if you try to access GetObjectImpl method in any field of the classes managed by spring container.");
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }

            object impl = _applicationContext[key];
            return (T)impl;
        }
    }
}
