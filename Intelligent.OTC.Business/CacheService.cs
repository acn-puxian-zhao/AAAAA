using Intelligent.OTC.Common;
using System;
using System.Web;

namespace Intelligent.OTC.Business
{
    public class CacheService : ICacheService
    {
        public T GetOrSet<T>(string cacheKey, Func<T> getItemCallBack) where T : class
        {
            T item = default(T);
            if (HttpContext.Current != null)
                item = HttpContext.Current.Cache.Get(cacheKey) as T;

            if (item == null)
            {
                item = getItemCallBack();
                if (HttpContext.Current != null)
                    HttpContext.Current.Cache.Insert(cacheKey, item);
            }
            return item;
        }

    }
}
