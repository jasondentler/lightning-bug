using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using LightningBug.Reflection;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace LightningBug.Data
{
    public static class DataTableExtensions
    {

        /// <summary>
        /// Creates an empty <see cref="System.Data.DataTable"/> using the schema of <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>An empty <see cref="System.Data.DataTable"/> using the schema of <typeparamref name="T"/></returns>
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

        /// <summary>
        /// Creates a <see cref="System.Data.DataTable"/> using the schema of <typeparamref name="T"/> and data from <paramref name="rows"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rows">Values for rows in the data table.</param>
        /// <returns>A <see cref="System.Data.DataTable"/> using the schema of <typeparamref name="T"/> and data from <paramref name="rows"/>.</returns>
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

        /// <summary>
        /// Converts rows in <paramref name="table"/> to instances of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <returns><see cref="IEnumerable{T}"/> of <typeparamref name="T"/> filled with data from <paramref name="table"/></returns>
        /// <remarks>Uses the default constructor of <typeparamref name="T"/> to create an instance.</remarks>
        public static IEnumerable<T> Convert<T>(this DataTable table) where T : new()
        {
            return table.Convert(() => new T());
        }

        /// <summary>
        /// Converts rows in <paramref name="table"/> to instances of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="constructor">Method used to create an instance of <typeparamref name="T"/>.</param>
        /// <returns><see cref="IEnumerable{T}"/> of <typeparamref name="T"/> filled with data from <paramref name="table"/></returns>
        public static IEnumerable<T> Convert<T>(this DataTable table, Func<T> constructor)
        {
            return table.Rows.Cast<DataRow>().Select(r => r.ToInstance(constructor));
        }

        /// <summary>
        /// Creates a SQL table matching <paramref name="table"/>.
        /// </summary>
        /// <param name="table">Schema source for new SQL table</param>
        /// <param name="connection">Connection to SQL Server database</param>
        /// <remarks>New SQL table name is <code><paramref name="table"/>.TableName</code>.</remarks>
        public static void CreateTable(this DataTable table, SqlConnection connection)
        {
            table.CreateTable(connection, table.TableName);
        }

        /// <summary>
        /// Creates a SQL table matching <paramref name="table"/> with name <paramref name="tableName"/>.
        /// </summary>
        /// <param name="table">Schema source for new SQL table</param>
        /// <param name="connection">Connection to SQL Server database</param>
        /// <param name="tableName">Name of table to create</param>
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
