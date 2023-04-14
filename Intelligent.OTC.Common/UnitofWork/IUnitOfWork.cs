using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Intelligent.OTC.Common.UnitOfWork
{
    public interface IUnitOfWork
    {
        void RegisterAmended<T>(T entity, IUnitOfWorkRepository unitofWorkRepository) where T : class, IAggregateRoot;
        void RegisterNew<T>(T entity, IUnitOfWorkRepository unitofWorkRepository) where T : class, IAggregateRoot;
        void RegisterNewRange<T>(IEnumerable<T> entities, IUnitOfWorkRepository unitofWorkRepository) where T : class, IAggregateRoot;
        void RegisterRemoved<T>(T entity, IUnitOfWorkRepository unitofWorkRepository) where T : class, IAggregateRoot;
        void RegisterRemoveRange<T>(IEnumerable<T> entities, IUnitOfWorkRepository unitofWorkRepository) where T : class, IAggregateRoot;
        void Commit(IUnitOfWorkRepository unitofWorkRepository);
    }

}
