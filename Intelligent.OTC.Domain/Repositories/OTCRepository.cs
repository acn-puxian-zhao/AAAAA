using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common;
using System.Web;
using Intelligent.OTC.Domain.DataModel;
using System.Data.Entity;
using Intelligent.OTC.Common.DataContextStorage;
using Intelligent.OTC.Common.Repository;
using System.Data.Entity.Infrastructure;

namespace Intelligent.OTC.Domain.Repositories
{
    public class OTCRepository : Repository, IRepository
    {
        private const string REPOSITORY_KEY = "OTC";

        private OTCEntities otcDbContext;

        public override DbContext GetDBContext()
        {
            IDataContextStorageContainer _dataContextStorageContainer = DataContextStorageFactory.CreateStorageContainer();

            otcDbContext = _dataContextStorageContainer.GetDataContext(REPOSITORY_KEY) as OTCEntities;
            if (otcDbContext == null)
            {
                otcDbContext = new OTCEntities();
                otcDbContext.Configuration.LazyLoadingEnabled = false;
                (otcDbContext as IObjectContextAdapter).ObjectContext.CommandTimeout = 1800;

                _dataContextStorageContainer.Store(REPOSITORY_KEY, otcDbContext);
            }

            return otcDbContext;            
        }

        public void RecreateContext(bool autoDetectChanges = true)
        {
            IDataContextStorageContainer _dataContextStorageContainer = DataContextStorageFactory.CreateStorageContainer();

            otcDbContext.Dispose();
            otcDbContext = new OTCEntities();
            otcDbContext.Configuration.LazyLoadingEnabled = false;
            otcDbContext.Configuration.AutoDetectChangesEnabled = autoDetectChanges;
            _dataContextStorageContainer.Store(REPOSITORY_KEY, otcDbContext);
        }
    }
}
