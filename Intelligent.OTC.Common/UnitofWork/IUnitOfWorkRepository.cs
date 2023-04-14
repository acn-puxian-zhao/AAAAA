using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Intelligent.OTC.Common.UnitOfWork
{
    public interface IUnitOfWorkRepository
    {
        void PersistCreationOf<T>(T entity) where T : class, IAggregateRoot;
        void PersistRangeCreationOf<T>(IEnumerable<T> entities) where T : class, IAggregateRoot;
        void PersistUpdateOf<T>(T entity) where T : class, IAggregateRoot;
        void PersistDeletionOf<T>(T entity) where T : class, IAggregateRoot;
        void PersistRangeDeletionOf<T>(IEnumerable<T> entities) where T : class, IAggregateRoot;
        void PersistCommit();
    }
}
