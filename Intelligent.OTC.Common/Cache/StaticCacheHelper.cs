using System;
using System.Collections.Concurrent;

namespace Intelligent.OTC.Common
{
    /// <summary>
    /// 自定义静态缓存, 存储耗时的小数据
    /// </summary>
    public static class StaticCacheHelper
    {
        private static ConcurrentDictionary<string, CahceItem> Data = new ConcurrentDictionary<string, CahceItem>();

        public static void Add(string user, string key, int duration, object value)
        {
            string dicKey = string.Format("{0}_{1}", user, key);
            CahceItem item;
            if (Data.ContainsKey(dicKey))
            {
                item = Data[dicKey];
            }
            else
            {
                item = new CahceItem();
            }

            item.Expire = DateTime.Now.AddSeconds(duration);
            item.Value = value;
            Data.TryAdd(dicKey, item);
        }

        public static T Get<T>(string user, string key)
        {
            string dicKey = string.Format("{0}_{1}", user, key);
            CahceItem item;

            if (Data.TryGetValue(dicKey, out item))
            {
                if (DateTime.Now > item.Expire)
                {
                    //the cache expire
                    return default(T);
                }
                else
                {
                    return (T)item.Value;
                }
            }
            else
            {
                //the cache is not exist
                return default(T);
            }
        }
    }

    public class CahceItem
    {
        //the cache cannot be used after the expire time
        public DateTime Expire { get; set; }

        //the cache value
        public object Value { get; set; }
    }
}
