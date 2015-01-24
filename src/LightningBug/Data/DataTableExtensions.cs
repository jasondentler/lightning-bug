using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace LightningBug.Data
{
    public static class DataTableExtensions
    {

        public static DataTable GetSchemaTable<T>()
        {
            var dataTable = new DataTable {TableName = typeof (T).Name};

            var columns = GetterDelegateCache<T>.Types
                .Select(kv => new DataColumn(kv.Key, ConvertToDatabaseType(kv.Value)))
                .ToArray();

            dataTable.Columns.AddRange(columns);

            return dataTable;
        }

        private static Type ConvertToDatabaseType(Type t)
        {
            if (t.IsGenericType)
            {
                var generic = t.GetGenericTypeDefinition();
                if (generic == typeof (Nullable<>))
                    return t.GenericTypeArguments.Single();
            }
            return t;
        }

        public static DataTable AsDataTable<T>(this IEnumerable<T> rows)
        {
            var dataTable = GetSchemaTable<T>();

            rows.Select(row => GetterDelegateCache<T>.ReadablePropertyNames
                .Select(s => GetterDelegateCache<T>.Read(s, row))
                .ToArray())
                .ToList()
                .ForEach(values => dataTable.Rows.Add(values));

            return dataTable;
        }

        public static IEnumerable<T> Convert<T>(this DataTable table) where T : new()
        {
            return table.Convert(() => new T());
        }

        public static IEnumerable<T> Convert<T>(this DataTable table, Func<T> constructor)
        {
            return table.Rows.Cast<DataRow>().Select(r => r.ToInstance(constructor));
        }

        public static void CreateTable(this DataTable table, SqlConnection connection)
        {
            table.CreateTable(connection, table.TableName);
        }

        public static void CreateTable(this DataTable table, SqlConnection connection, string tableName)
        {
            var serverConnection = new ServerConnection(connection);
            var server = new Server(serverConnection);
            var db = server.Databases[serverConnection.DatabaseName];
            var t = new Table(db, tableName);
            foreach (var column in table.Columns.Cast<DataColumn>().OrderBy(c => c.Ordinal))
            {
                var c = new Column();
                c.Name = column.ColumnName;
                c.Parent = t;
                c.DataType = column.DataType.ToSmoDataType();
                t.Columns.Add(c);
            }
            t.Create();
        }

    }
}
