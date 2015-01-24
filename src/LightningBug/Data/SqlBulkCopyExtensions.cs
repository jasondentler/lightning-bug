using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace LightningBug.Data
{
    public static class SqlBulkCopyExtensions
    {

        private static readonly FieldInfo ConnectionFieldInfo;

        static SqlBulkCopyExtensions()
        {
            ConnectionFieldInfo = typeof(SqlBulkCopy).GetField("_connection", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        /// <summary>
        /// Inserts <paramref name="rows"/> in to SQL
        /// </summary>
        /// <param name="copy"></param>
        /// <param name="rows">Data source of rows to insert.</param>
        public static void Insert(this SqlBulkCopy copy, DataRow[] rows)
        {
            copy.WriteToServer(rows);
        }

        /// <summary>
        /// Inserts <paramref name="table"/> rows in to SQL
        /// </summary>
        /// <param name="copy"></param>
        /// <param name="table">Data source of rows to insert</param>
        public static void Insert(this SqlBulkCopy copy, DataTable table)
        {
            copy.WriteToServer(table);
        }

        /// <summary>
        /// Inserts <paramref name="table"/> rows in to SQL
        /// </summary>
        /// <param name="copy"></param>
        /// <param name="table">Data source of rows to insert</param>
        /// <param name="rowState">A value from the <see cref="DataRowState"/> enumeration. Only rows matching the row state are copied to the destination.</param>
        public static void Insert(this SqlBulkCopy copy, DataTable table, DataRowState rowState)
        {
            copy.WriteToServer(table, rowState);
        }

        /// <summary>
        /// Inserts <paramref name="reader"/> rows in to SQL
        /// </summary>
        /// <param name="copy"></param>
        /// <param name="reader">Data source of rows to insert</param>
        public static void Insert(this SqlBulkCopy copy, IDataReader reader)
        {
            copy.WriteToServer(reader);
        }

        /// <summary>
        /// Updates rows in SQL
        /// </summary>
        /// <param name="copy"></param>
        /// <param name="reader"></param>
        /// <param name="keyColumns">Column(s) to match</param>
        /// <param name="dataColumns">Column(s) to update</param>
        public static void Update(this SqlBulkCopy copy, IDataReader reader, string[] keyColumns, string[] dataColumns)
        {
            new UpdateOperation(copy, reader, keyColumns, dataColumns).Execute();
        }

        internal static SqlConnection GetConnection(this SqlBulkCopy copy)
        {
            return (SqlConnection) ConnectionFieldInfo.GetValue(copy);
        }

    }

}
