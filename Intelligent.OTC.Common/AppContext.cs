using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Configuration;
using Intelligent.OTC.Common.Exceptions;
using Intelligent.OTC.Common.Utils;
using Intelligent.OTC.Common.DataContextStorage;

namespace Intelligent.OTC.Common
{
    public class AppContext : Dictionary<string, object>
    {
        public const string ContextKey = "AppContext";
        public const string BatchContextKey = "BatchContext";
        private static IDataContextStorageContainer _threadLocalStorageContainer;
        
        public static AppContext Current
        {
            get
            {
                if (null != HttpContext.Current && HttpContext.Current.Session != null)
                {
                    if (null == HttpContext.Current.Session[ContextKey])
                    {
                        HttpContext.Current.Session[ContextKey] = new AppContext();
                    }
                    return HttpContext.Current.Session[ContextKey] as AppContext;
                }
                else
                {
                    if (_threadLocalStorageContainer == null)
                    {
                        _threadLocalStorageContainer = new ThreadDataContextStorageContainer();
                    }

                    AppContext ctx = _threadLocalStorageContainer.GetDataContext(BatchContextKey) as AppContext;
                    if (ctx == null)
                    {
                        // the batch account time zone shift is 8. Is that right?????
                        ctx = new AppContext() { User = new SysUser() { EID = "BATCH_USER", TimeZone = 8
                            , Deal = ConfigurationManager.AppSettings["BatchDeal"]
                        } };
                        _threadLocalStorageContainer.Store(BatchContextKey, ctx);
                    }

                    return ctx;
                }
            }
        }

        private SysUser user;
        public SysUser User
        {
            get
            {
                return user;
            }
            set {
                user = value;
            }
        }
    }
}
