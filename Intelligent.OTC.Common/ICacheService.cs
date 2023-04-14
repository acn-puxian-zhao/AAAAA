using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Intelligent.OTC.Common
{
    public interface ICacheService
    {
        T GetOrSet<T>(string cacheKey, Func<T> getItemCallBack) where T : class;
    }
}
