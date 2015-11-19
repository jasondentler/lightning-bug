using System;
using System.Data.SqlClient;

namespace LightningBug.Data.ETL.Operations
{
    public abstract class SqlCommandOperation<T> : CommandOperation<T>
    {
        private readonly Func<SqlConnection> _connectionProvider;

        protected SqlCommandOperation(Func<SqlConnection> connectionProvider)
        {
            _connectionProvider = connectionProvider;
        }

        public override void OnInputReady()
        {
            base.OnInputReady();
            using (var conn = _connectionProvider())
            {
                conn.Open();
                ExecuteSql(conn);
            }
        }

        protected abstract void ExecuteSql(SqlConnection connection);

    }
}