using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace Intelligent.OTC.Common.Repository
{
    public interface IRepository
    {
        void Add<T>(T entity) where T : class, IAggregateRoot;
        void AddRange<T>(IEnumerable<T> entities) where T : class, IAggregateRoot;
        void Remove<T>(T entity) where T : class, IAggregateRoot;
        void RemoveRange<T>(IEnumerable<T> enities) where T : class, IAggregateRoot;
        void Save<T>(T entity) where T : class, IAggregateRoot;
        T FindBy<T>(int id) where T : class, IAggregateRoot;
        IEnumerable<T> FindAll<T>() where T : class, IAggregateRoot;
        DbSet<T> GetDbSet<T>() where T : class, IAggregateRoot;
        IQueryable<T> GetQueryable<T>() where T : class, IAggregateRoot;
        IEnumerable<T> FindAll<T>(int index, int count) where T : class, IAggregateRoot;
        void BulkInsert<T>(IEnumerable<T> entities) where T : class, IAggregateRoot;
        void BulkUpdate<T>(IEnumerable<T> entities) where T : class, IAggregateRoot;
        void BulkDelete<T>(IEnumerable<T> entities) where T : class, IAggregateRoot;
        void BulkMerge<T>(IEnumerable<T> entities) where T : class, IAggregateRoot;
        void BulkSaveChanges();
        void Commit();
        //Extension
        DataTable ExecuteDataTable(string sql, params object[] parameters);
        DataTable ExecuteDataTable(CommandType cmdType, string sql, params object[] parameters);
        DataSet ExecuteDataSet(string sql, params object[] parameters);
        DataSet ExecuteDataSet(CommandType cmdType, string sql, params object[] parameters);
    }
}
