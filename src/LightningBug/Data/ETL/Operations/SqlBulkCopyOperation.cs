using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace LightningBug.Data.ETL.Operations
{
    public class SqlBulkCopyOperation<TInput> : IOperation<TInput, object>
    {

        private readonly Func<SqlConnection> _connectionProvider;
        private readonly string _destinationTable;

        public SqlBulkCopyOperation(Func<SqlConnection> connectionProvider, string destinationTable)
        {
            _connectionProvider = connectionProvider;
            _destinationTable = destinationTable;
        }

        public IEnumerable<object> Execute(IEnumerable<TInput> input)
        {
            var reader = new ObjectDataReader<TInput>(input);
            using (var conn = _connectionProvider())
            {
                conn.Open();
                using (var bcp = new SqlBulkCopy(conn))
                {
                    bcp.DestinationTableName = _destinationTable;
                    bcp.Insert(reader);
                }
            }
            return Enumerable.Empty<object>();
        }

        public string Name { get { return "Database Destination:" + _destinationTable; } }
    }
}