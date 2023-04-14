using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Data.Entity;

namespace Intelligent.OTC.Common.DataContextStorage
{
    public class HttpDataContextStorageContainer : IDataContextStorageContainer
    {
        private string _dataContextKey = "DataContext";

        public object GetDataContext(string key)
        {
            object objectContext = null;
            if (HttpContext.Current.Items.Contains(_dataContextKey + key))
                objectContext = (object)HttpContext.Current.Items[_dataContextKey + key];

            return objectContext;
        }

        public void Store(string key, object libraryDataContext)
        {
            if (HttpContext.Current.Items.Contains(_dataContextKey + key))
                HttpContext.Current.Items[_dataContextKey + key] = libraryDataContext;
            else
                HttpContext.Current.Items.Add(_dataContextKey + key, libraryDataContext);  
        }

    }
}
