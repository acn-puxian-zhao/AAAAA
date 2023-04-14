using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common.Repository;
using Intelligent.OTC.Domain.DataModel;

namespace Intelligent.OTC.Domain.Repositories
{
    public class OTCTimerRepository : Repository, IRepository
    {
        private OTCEntities otcDbContext;

        public override DbContext GetDBContext()
        {
            if (otcDbContext == null)
            {
                otcDbContext = new OTCEntities();
                otcDbContext.Configuration.LazyLoadingEnabled = false;
                (otcDbContext as IObjectContextAdapter).ObjectContext.CommandTimeout = 1800;

            }

            return otcDbContext;
        }

    }
}
