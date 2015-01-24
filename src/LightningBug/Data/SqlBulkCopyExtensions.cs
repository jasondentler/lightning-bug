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

        public static void Insert(this SqlBulkCopy copy, DataRow[] rows)
        {
            copy.WriteToServer(rows);
        }

        public static void Insert(this SqlBulkCopy copy, DataTable table)
        {
            copy.WriteToServer(table);
        }

        public static void Insert(this SqlBulkCopy copy, DataTable table, DataRowState rowState)
        {
            copy.WriteToServer(table, rowState);
        }

        public static void Insert(this SqlBulkCopy copy, IDataReader reader)
        {
            copy.WriteToServer(reader);
        }

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
