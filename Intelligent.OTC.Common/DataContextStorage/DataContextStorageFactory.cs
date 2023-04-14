using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Data.Entity;

namespace Intelligent.OTC.Common.DataContextStorage
{
    public class DataContextStorageFactory
    {
        public static IDataContextStorageContainer threadDataContextStorageContainer;
        public static IDataContextStorageContainer httpDataContextStorageContainer;

        public static IDataContextStorageContainer CreateStorageContainer()
        {
            if(HttpContext.Current == null)
            {
                //当前请求是从后台任务调用
                if(threadDataContextStorageContainer == null)
                {
                    threadDataContextStorageContainer = new ThreadDataContextStorageContainer();
                }
                return threadDataContextStorageContainer as IDataContextStorageContainer;
            }
            else
            {
                //当前请求是从Web请求调用
                if (httpDataContextStorageContainer == null)
                {
                    httpDataContextStorageContainer = new HttpDataContextStorageContainer();
                }
                return httpDataContextStorageContainer as IDataContextStorageContainer;
            }
        }
    }
}
