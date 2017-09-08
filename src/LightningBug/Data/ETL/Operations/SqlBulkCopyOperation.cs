using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using LightningBug.Reflection;

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
            using (var bcp = new SqlBulkCopy(""))
                Timeout = TimeSpan.FromSeconds(bcp.BulkCopyTimeout);
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
                    bcp.BulkCopyTimeout = (int) Math.Ceiling(Timeout.TotalSeconds);
                    SetupMappings(bcp);
                    bcp.Insert(reader);
                }
            }
            return Enumerable.Empty<object>();
        }

        public TimeSpan Timeout { get; set; }

        public string Name { get { return "Database Destination:" + _destinationTable; } }

        private void SetupMappings(SqlBulkCopy bcp)
        {
            var properties = GetterDelegateCache<TInput>.ReadablePropertyNames.ToArray();

            var conn = bcp.GetConnection();

            var destination = bcp.DestinationTableName;

            var destinationParts = new List<string>(SqlIdentifier.Parse(destination));

            while (destinationParts.Count < 3)
                destinationParts.Insert(0, null);

            var schemaTable = conn.GetSchema("Columns", destinationParts.ToArray());

            var columns = schemaTable.Rows.Cast<DataRow>()
                .Select(r => (string) r[3])
                .ToArray();

            foreach (var map in properties.Intersect(columns, StringComparer.InvariantCultureIgnoreCase))
                bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping(map, map));

        }
    }
}