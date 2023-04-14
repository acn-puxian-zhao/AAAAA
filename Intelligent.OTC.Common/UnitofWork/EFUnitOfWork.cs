using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.UnitOfWork;

namespace Intelligent.OTC.Common.UnitOfWork
{
    public class EFUnitOfWork : IUnitOfWork 
    {
        public void Commit(IUnitOfWorkRepository unitofWorkRepository)
        {
            unitofWorkRepository.PersistCommit();
        }

        public void RegisterAmended<T>(T entity, IUnitOfWorkRepository unitofWorkRepository) where T : class, IAggregateRoot
        {
            unitofWorkRepository.PersistUpdateOf(entity); 
        }

        public void RegisterNew<T>(T entity, IUnitOfWorkRepository unitofWorkRepository) where T: class, IAggregateRoot
        {
            unitofWorkRepository.PersistCreationOf<T>(entity); 
        }

        public void RegisterNewRange<T>(IEnumerable<T> entities, IUnitOfWorkRepository unitofWorkRepository) where T : class, IAggregateRoot
        {
            unitofWorkRepository.PersistRangeCreationOf(entities);
        }

        public void RegisterRemoved<T>(T entity, IUnitOfWorkRepository unitofWorkRepository) where T: class, IAggregateRoot
        {
            unitofWorkRepository.PersistDeletionOf(entity); 
        }

        public void RegisterRemoveRange<T>(IEnumerable<T> enities, IUnitOfWorkRepository unitofWorkRepository) where T : class, IAggregateRoot
        {
            unitofWorkRepository.PersistRangeDeletionOf(enities);
        }
    }
}
