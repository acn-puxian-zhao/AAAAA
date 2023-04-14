using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intelligent.OTC.Common;
using Intelligent.OTC.Common.UnitOfWork;
using System.Data.Entity;
using System.Data;

namespace Intelligent.OTC.Common.Repository
{
    public abstract class Repository : IUnitOfWorkRepository, IRepository
    {
        private IUnitOfWork _uow;
        public IUnitOfWork UOW
        {
            set
            {
                _uow = value;
            }
        }

        public void Add<T>(T entity) where T : class, IAggregateRoot
        {
            _uow.RegisterNew<T>(entity, this);                        
        }

        public void AddRange<T>(IEnumerable<T> entities) where T : class, IAggregateRoot
        {
            _uow.RegisterNewRange(entities, this);
        }

        public void Remove<T>(T entity) where T : class, IAggregateRoot
        {
            _uow.RegisterRemoved(entity, this);            
        }

        public void RemoveRange<T>(IEnumerable<T> enities) where T : class, IAggregateRoot
        {
            _uow.RegisterRemoveRange(enities, this);
        }

        public void Commit()
        {
            _uow.Commit(this);
        }

        public void Save<T>(T entity) where T : class, IAggregateRoot
        {
            // Do nothing as EF tracks changes
        }
        public abstract DbContext GetDBContext();

        public virtual DbSet<T> GetDbSet<T>() where T : class, IAggregateRoot
        {
            return GetDBContext().Set<T>();
        }

        public virtual T FindBy<T>(int id) where T : class, IAggregateRoot
        {
            return GetDbSet<T>().FirstOrDefault<T>(b => b.Id == id);
        }

        public virtual IEnumerable<T> FindAll<T>() where T : class, IAggregateRoot
        {
            return GetDbSet<T>().ToList<T>(); 
        }

        public virtual IQueryable<T> GetQueryable<T>() where T : class, IAggregateRoot
        {
            return GetDbSet<T>().AsQueryable<T>();
        }

        public IEnumerable<T> FindAll<T>(int index, int count) where T : class, IAggregateRoot
        {
            return GetDbSet<T>().Skip(index).Take(count).ToList<T>(); 
        }

        public void PersistCreationOf<T>(T entity) where T : class, IAggregateRoot
        {
            GetDbSet<T>().Add(entity);
        }

        public void PersistUpdateOf<T>(T entity) where T : class, IAggregateRoot
        {
            // Do nothing as EF tracks changes
        }

        public void PersistDeletionOf<T>(T entity) where T : class, IAggregateRoot
        {
            GetDbSet<T>().Remove(entity);
        }

        public void PersistRangeDeletionOf<T>(IEnumerable<T> entities) where T : class, IAggregateRoot
        {
            GetDbSet<T>().RemoveRange(entities);
        }
        public void PersistRangeCreationOf<T>(IEnumerable<T> entities) where T : class, IAggregateRoot
        {
            GetDbSet<T>().AddRange(entities);
        }
        
        public void BulkInsert<T>(IEnumerable<T> entities) where T : class, IAggregateRoot
        {
            GetDBContext().BulkInsert(entities);
        }
        public void BulkUpdate<T>(IEnumerable<T> entities) where T : class, IAggregateRoot
        {
            GetDBContext().BulkUpdate(entities);
        }
        public void BulkDelete<T>(IEnumerable<T> entities) where T : class, IAggregateRoot
        {
            GetDBContext().BulkDelete(entities);
        }
        public void BulkMerge<T>(IEnumerable<T> entities) where T : class, IAggregateRoot
        {
            GetDBContext().BulkMerge(entities);
        }

        public void BulkSaveChanges()
        {
            GetDBContext().BulkSaveChanges();
        }
        
        public void PersistCommit()
        {
            GetDBContext().SaveChanges(); 
        }
        /// <summary>
        /// DBContext扩展 返回值DataTable
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string sql, params object[] parameters)
        {
            return GetDBContext().Database.ExecuteDataTable(sql, parameters);
        }

        public IEnumerable<T> ExecuteSqlQuery<T>(string sql, params object[] parameters)
        {
            return GetDBContext().Database.SqlQuery<T>(sql, parameters);
        }

        /// <summary>
        /// DBContext扩展 返回值DataTable
        /// </summary>
        /// <param name="cmdType"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(CommandType cmdType, string sql, params object[] parameters)
        {
            return GetDBContext().Database.ExecuteDataTable(cmdType,sql, parameters);
        }
        /// <summary>
        /// DBContext扩展 返回值DataSet
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataSet ExecuteDataSet(string sql, params object[] parameters)
        {
            return GetDBContext().Database.ExecuteDataSet(sql, parameters);
        }
        /// <summary>
        /// DBContext扩展 返回值DataSet
        /// </summary>
        /// <param name="cmdType"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataSet ExecuteDataSet(CommandType cmdType, string sql, params object[] parameters)
        {
            return GetDBContext().Database.ExecuteDataSet(cmdType, sql, parameters);
        }
    }
}
