using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace EntityFramework.BulkInsert.Extensions
{
    public static class BulkInsertExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="entities"></param>
        /// <param name="options"></param>
        public static void BulkInsert<T>(this DbContext context, IEnumerable<T> entities, BulkInsertOptions options)
        {
            var bulkInsert = ProviderFactory.Get(context);
            bulkInsert.Options = options;
            bulkInsert.Run(entities);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="entities"></param>
        /// <param name="batchSize"></param>
        public static void BulkInsert<T>(this DbContext context, IEnumerable<T> entities, int? batchSize = null)
        {
            context.BulkInsert(entities, SqlBulkCopyOptions.Default, batchSize);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="entities"></param>
        /// <param name="sqlBulkCopyOptions"></param>
        /// <param name="batchSize"></param>
        public static void BulkInsert<T>(this DbContext context, IEnumerable<T> entities, SqlBulkCopyOptions sqlBulkCopyOptions, int? batchSize = null)
        {

            var options = new BulkInsertOptions {SqlBulkCopyOptions = sqlBulkCopyOptions};
            if (batchSize.HasValue)
            {
                options.BatchSize = batchSize.Value;
            }
            context.BulkInsert(entities, options);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="entities"></param>
        /// <param name="transaction"></param>
        /// <param name="sqlBulkCopyOptions"></param>
        /// <param name="batchSize"></param>
        public static void BulkInsert<T>(this DbContext context, IEnumerable<T> entities, IDbTransaction transaction, SqlBulkCopyOptions sqlBulkCopyOptions = SqlBulkCopyOptions.Default, int? batchSize = null)
        {
            var options = new BulkInsertOptions {SqlBulkCopyOptions = sqlBulkCopyOptions};
            if (batchSize.HasValue)
            {
                options.BatchSize = batchSize.Value;
            }
            context.BulkInsert(entities, options);
        }

    }

    public static class BulkInsertDefaults
    {
        public static int BatchSize = 5000;
        public static SqlBulkCopyOptions SqlBulkCopyOptions = SqlBulkCopyOptions.Default;
        public static int TimeOut = 30;
        public static int NotifyAfter = 1000;
    }

    public class BulkInsertOptions
    {
        /// <summary>
        /// 
        /// </summary>
        public int BatchSize { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public SqlBulkCopyOptions SqlBulkCopyOptions { get; set; }

        /// <summary>
        /// Number of the seconds for the operation to complete before it times out
        /// </summary>
        public int TimeOut { get; set; }

        /// <summary>
        /// Callback event handler. Event is fired after n (value from NotifyAfter) rows have been copied to table where.
        /// </summary>
        public SqlRowsCopiedEventHandler Callback { get; set; }

        /// <summary>
        /// Used with property Callback. Sets number of rows after callback is fired.
        /// </summary>
        public int NotifyAfter { get; set; }

#if !NET40
        /// <summary>
        /// 
        /// </summary>
        public bool EnableStreaming { get; set; }
#endif

        public BulkInsertOptions()
        {
            BatchSize = BulkInsertDefaults.BatchSize;
            SqlBulkCopyOptions = BulkInsertDefaults.SqlBulkCopyOptions;
            TimeOut = BulkInsertDefaults.TimeOut;
            NotifyAfter = BulkInsertDefaults.NotifyAfter;
        }
    }
}
