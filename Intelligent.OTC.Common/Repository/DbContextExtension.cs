using System.Data;
using System.Data.Common;
using System.Data.Entity;

namespace Intelligent.OTC.Common.Repository
{
    public static class DbContextExtension
    {
        public static DataTable ExecuteDataTable(this Database database, string sql, params object[] parameters)
        {
            return ExecuteDataTable(database, CommandType.StoredProcedure, sql, parameters);
        }

        public static DataTable ExecuteDataTable(this Database database, CommandType cmdType, string sql, params object[] parameters)
        {
            DataTable ds = new DataTable();
            DbProviderFactory provider = DbProviderFactories.GetFactory(database.Connection);
            DbConnection conn = provider.CreateConnection();
            conn.ConnectionString = database.Connection.ConnectionString;
            DbCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = cmdType;
            if (parameters != null)
            {
                foreach (var para in parameters)
                {
                    cmd.Parameters.Add(para);
                }
            }
            DbDataAdapter adapter = provider.CreateDataAdapter();
            adapter.SelectCommand = cmd;
            adapter.Fill(ds);
            return ds;
        }

        public static DataSet ExecuteDataSet(this Database database, string sql, params object[] parameters)
        {
            return ExecuteDataSet(database, CommandType.StoredProcedure, sql, parameters);
        }

        public static DataSet ExecuteDataSet(this Database database, CommandType cmdType, string sql, params object[] parameters)
        {
            DataSet ds = new DataSet();
            DbProviderFactory provider = DbProviderFactories.GetFactory(database.Connection);
            DbConnection conn = provider.CreateConnection();
            conn.ConnectionString = database.Connection.ConnectionString;
            DbCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = cmdType;
            if (parameters != null)
            {
                foreach (var para in parameters)
                {
                    cmd.Parameters.Add(para);
                }
            }
            DbDataAdapter adapter = provider.CreateDataAdapter();
            adapter.SelectCommand = cmd;
            adapter.Fill(ds);
            return ds;
        }

        public static void ExecuteSP(this Database database,string sql, params object[] parameters)
        {
            DbProviderFactory provider = DbProviderFactories.GetFactory(database.Connection);
            using (DbConnection conn = provider.CreateConnection())
            {
                conn.ConnectionString = database.Connection.ConnectionString;
                conn.Open();
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.StoredProcedure;
                if (parameters != null)
                {
                    foreach (var para in parameters)
                    {
                        cmd.Parameters.Add(para);
                    }
                }
                cmd.ExecuteNonQuery();
            }
        }
    }
}
