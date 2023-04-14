using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common;
using Intelligent.OTC.Domain.DataModel;
using System.Data.Entity;
using Intelligent.OTC.Common.DataContextStorage;
using Intelligent.OTC.Common.Repository;

namespace Intelligent.OTC.Domain.Repositories
{
    public class WorkflowRepository : Repository, IRepository
    {
        private const string REPOSITORY_KEY = "workflow";

        public override DbContext GetDBContext()
        {
            IDataContextStorageContainer _dataContextStorageContainer = DataContextStorageFactory.CreateStorageContainer();

            WorkflowEntities wfDbContext = _dataContextStorageContainer.GetDataContext(REPOSITORY_KEY) as WorkflowEntities;
            if (wfDbContext == null)
            {
                wfDbContext = new WorkflowEntities();
                wfDbContext.Configuration.LazyLoadingEnabled = false;
                _dataContextStorageContainer.Store(REPOSITORY_KEY, wfDbContext);
            }

            return wfDbContext;
        }
    }
}
