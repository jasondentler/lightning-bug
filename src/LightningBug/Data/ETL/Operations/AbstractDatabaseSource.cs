using System;
using System.Collections.Generic;
using System.Data;
using LightningBug.Reflection;

namespace LightningBug.Data.ETL.Operations
{
    public abstract class AbstractDatabaseSource<T> : IOperation<object, T>
    {

        protected abstract T CreateInstance();

        private readonly Func<IDbConnection> _connectionProvider;
        private readonly string _query;

        protected AbstractDatabaseSource(Func<IDbConnection> connectionProvider, string query)
        {
            _connectionProvider = connectionProvider;
            _query = query;
        }

        public IEnumerable<T> Execute(IEnumerable<object> input)
        {
            using (var conn = _connectionProvider())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                using (cmd)
                {
                    cmd.CommandText = _query;
                    cmd.CommandType = CommandType.Text;
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            var instance = CreateInstance();
                            for (var i = 0; i < rdr.FieldCount; i++)
                            {
                                var value = rdr.IsDBNull(i) ? null : rdr[i];
                                SetterDelegateCache<T>.Write(i, instance, value);
                            }
                            yield return instance;
                        }
                    }
                }

            }
        }


        public string Name { get { return "Database Source:" + _query; } }
    }
}