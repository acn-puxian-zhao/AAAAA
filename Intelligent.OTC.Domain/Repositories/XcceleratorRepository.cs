using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common.Repository;
using System.Data.Entity;
using Intelligent.OTC.Common.DataContextStorage;
using Intelligent.OTC.Domain.DataModel;

namespace Intelligent.OTC.Domain.Repositories
{
    public class XcceleratorRepository : Repository, IRepository
    {
        private const string REPOSITORY_KEY = "xccelerator";

        public override DbContext GetDBContext()
        {
            IDataContextStorageContainer _dataContextStorageContainer = DataContextStorageFactory.CreateStorageContainer();

            XcceleratorEntities wfDbContext = _dataContextStorageContainer.GetDataContext(REPOSITORY_KEY) as XcceleratorEntities;
            if (wfDbContext == null)
            {
                wfDbContext = new XcceleratorEntities();
                wfDbContext.Configuration.LazyLoadingEnabled = false;
                _dataContextStorageContainer.Store(REPOSITORY_KEY, wfDbContext);
            }

            return wfDbContext;
        }
    }
}
