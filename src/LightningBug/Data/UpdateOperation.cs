using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace LightningBug.Data
{
    internal class UpdateOperation
    {

        private const DataRowState CurrentRows = DataRowState.Added | DataRowState.Modified | DataRowState.Unchanged;
        private readonly SqlConnection _connection;
        private readonly SqlBulkCopy _bulkCopy;
        private readonly IDataReader _dataReader;
        private readonly DataRow[] _rows;
        private readonly DataTable _table;
        private readonly DataRowState _rowState;
        private readonly string _tempTableName;
        private readonly string _destinationTableName;
        private readonly string[] _keyColumnNames;
        private readonly string[] _dataColumnNames;

        private IEnumerable<DataColumn> _dataColumns;
        private IEnumerable<DataColumn> _keyColumns;

        public UpdateOperation(SqlBulkCopy bulkCopy, IDataReader dataReader, string[] keyColumns, string[] dataColumns) : this(bulkCopy, keyColumns, dataColumns)
        {
            _dataReader = dataReader;
        }

        public UpdateOperation(SqlBulkCopy bulkCopy, DataRow[] rows, string[] keyColumns, string[] dataColumns)
            : this(bulkCopy, keyColumns, dataColumns)
        {
            _rows = rows;
        }

        public UpdateOperation(SqlBulkCopy bulkCopy, DataTable table, string[] keyColumns, string[] dataColumns)
            : this(bulkCopy, table, CurrentRows, keyColumns, dataColumns)
        {
        }

        public UpdateOperation(SqlBulkCopy bulkCopy, DataTable table, DataRowState rowState, string[] keyColumns, string[] dataColumns)
            : this(bulkCopy, keyColumns, dataColumns)
        {
            _table = table;
            _rowState = rowState;
        }

        private UpdateOperation(SqlBulkCopy bulkCopy, string[] keyColumns, string[] dataColumns)
        {
            _connection = bulkCopy.GetConnection();
            _bulkCopy = bulkCopy;
            _destinationTableName = bulkCopy.DestinationTableName;
            _tempTableName = "tmpUpdate" + Guid.NewGuid().ToString().Replace("-", string.Empty);
            _keyColumnNames = keyColumns;
            _dataColumnNames = dataColumns;
            if (!_keyColumnNames.Any())
                throw new ApplicationException("No key columns provided.");
        }

        public void Execute()
        {
            var mappedSchemaTable = GetMappedSchemaTable();
            _keyColumns = mappedSchemaTable.Columns.Cast<DataColumn>()
                .Where(c => _keyColumnNames.Contains(c.ColumnName))
                .ToArray();

            var missingKeyColumns = _keyColumnNames.Except(_keyColumns.Select(c => c.ColumnName)).ToArray();
            if (missingKeyColumns.Any())
                throw new ApplicationException("Missing key column(s): " + string.Join(", ", missingKeyColumns));

            _dataColumns = mappedSchemaTable.Columns.Cast<DataColumn>()
                .Where(c => _dataColumnNames.Contains(c.ColumnName))
                .ToArray();

            var missingDataColumns = _dataColumnNames.Except(_dataColumns.Select(c => c.ColumnName)).ToArray();
            if (missingDataColumns.Any())
                throw new ApplicationException("Missing data columns(s): " + string.Join(", ", missingDataColumns));

            OpenConnection();
            CreateTable(mappedSchemaTable);
            WriteToServer();
            Update();
            DropTable();
        }

        private void OpenConnection()
        {
            if (_connection.State != ConnectionState.Open)
                _connection.Open();
        }

        private void CreateTable(DataTable mappedSchemaTable)
        {
            mappedSchemaTable.CreateTable(_connection, _tempTableName);
        }

        private DataTable GetMappedSchemaTable()
        {
            var result = GetSchemaTable();
            if (_bulkCopy.ColumnMappings.Cast<SqlBulkCopyColumnMapping>().Any())
            {
                using (var writer = new StringWriter())
                {
                    result.WriteXmlSchema(writer);
                    result = new DataTable(result.TableName);
                    using (var reader = new StringReader(writer.ToString()))
                        result.ReadXmlSchema(reader);
                }
                
                var sourceColumns = result.Columns.Cast<DataColumn>()
                    .OrderBy(c => c.Ordinal)
                    .ToArray();
                
                var mappedColumns = _bulkCopy.ColumnMappings.Cast<SqlBulkCopyColumnMapping>()
                    .OrderBy(m => m.DestinationOrdinal)
                    .Select(m => m.DestinationColumn)
                    .ToArray();

                var toRemove = sourceColumns.Where(c => !mappedColumns.Contains(c.ColumnName));

                foreach (var column in toRemove)
                    result.Columns.Remove(column);
            }
            return result;
        }

        private DataTable GetSchemaTable()
        {
            if (_table != null)
                return _table;
            if (_dataReader != null)
                return _dataReader.GetSchemaTable();
            var dt = new DataTable("#", Guid.NewGuid().ToString());
            foreach (var dataRow in _rows)
                dt.Rows.Add(dataRow);
            dt.AcceptChanges();
            return dt;
        }

        private void WriteToServer()
        {
            _bulkCopy.DestinationTableName = _tempTableName;
            if (_table != null)
            {
                _bulkCopy.WriteToServer(_table, _rowState);
            } 
            else if (_dataReader != null)
            {
                _bulkCopy.WriteToServer(_dataReader);
            }
            else if (_rows != null)
            {
                _bulkCopy.WriteToServer(_rows);
            }
            _bulkCopy.DestinationTableName = _destinationTableName;
        }

        private void Update()
        {
            var sb = new StringBuilder();
            sb.AppendLine("UPDATE [" + _destinationTableName + "] SET ");
            foreach (var c in _dataColumns.OrderBy(c => c.Ordinal))
                sb.AppendLine("\t[" + c.ColumnName + "] = source.[" + c.ColumnName + "]");
            sb.AppendLine("FROM [" + _destinationTableName + "] dest");
            sb.AppendLine("\tINNER JOIN [" + _tempTableName + "] source");
            sb.AppendLine("\t\tON ");
            foreach (var c in _keyColumns.OrderBy(c => c.Ordinal))
            {
                sb.AppendLine("\t\tsource.[" + c.ColumnName + "] = dest.[" + c.ColumnName + "]");
            }

            var cmd = new SqlCommand(sb.ToString(), _connection)
            {
                CommandTimeout = (int) TimeSpan.FromMinutes(15).TotalSeconds
            };
            cmd.ExecuteNonQuery();
        }

        private void DropTable()
        {
            var serverConnection = new ServerConnection(_connection);
            var server = new Server(serverConnection);
            var db = server.Databases[serverConnection.DatabaseName];
            var t = db.Tables[_tempTableName];
            t.Drop();
        }

    }
}