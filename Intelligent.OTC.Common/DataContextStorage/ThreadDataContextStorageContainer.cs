using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading;
using System.Data.Entity;

namespace Intelligent.OTC.Common.DataContextStorage
{
    public class ThreadDataContextStorageContainer : IDataContextStorageContainer
    {    
        private static readonly Hashtable _libraryDataContexts = new Hashtable();

        public object GetDataContext(string key)
        {
            object libraryDataContext = null;

            if (_libraryDataContexts.Contains(GetThreadName() + key))
                libraryDataContext = (object)_libraryDataContexts[GetThreadName() + key];           

            return libraryDataContext;
        }

        public void Store(string key, object libraryDataContext)
        {
            if (_libraryDataContexts.Contains(GetThreadName() + key))
                _libraryDataContexts[GetThreadName() + key] = libraryDataContext;
            else
                _libraryDataContexts.Add(GetThreadName() + key, libraryDataContext);           
        }

        private static string GetThreadName()
        {
            return Thread.CurrentThread.Name;
        }     
    }
}
