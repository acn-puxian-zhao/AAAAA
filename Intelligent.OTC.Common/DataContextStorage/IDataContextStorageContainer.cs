using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace Intelligent.OTC.Common.DataContextStorage
{
    public interface IDataContextStorageContainer
    {
        object GetDataContext(string key);
        void Store(string key, object libraryDataContext);
    }
}
