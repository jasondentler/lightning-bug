using System;
using System.Data;

namespace LightningBug.Data.ETL.Operations
{
    public class DatabaseSource<T> : AbstractDatabaseSource<T> where T : new ()
    {
        public DatabaseSource(Func<IDbConnection> connectionProvider, string query)
            : base(connectionProvider, query)
        {
        }

        protected override T CreateInstance()
        {
            return new T();
        }
    }
}