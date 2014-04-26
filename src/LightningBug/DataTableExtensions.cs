using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace LightningBug
{
    public static class DataTableExtensions
    {

        public static DataTable GetSchemaTable<T>()
        {
            var dataTable = new DataTable {TableName = typeof (T).Name};

            var columns = GetterDelegateCache<T>.Types
                .Select(kv => new DataColumn(kv.Key, kv.Value))
                .ToArray();

            dataTable.Columns.AddRange(columns);

            return dataTable;
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
    }
}
