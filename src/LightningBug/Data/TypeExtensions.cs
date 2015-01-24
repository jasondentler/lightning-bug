using System;
using Microsoft.SqlServer.Management.Smo;

namespace LightningBug.Data
{
    public static class TypeExtensions
    {
        public static DataType ToSmoDataType(this Type type)
        {
            if (type == typeof(string)) return DataType.NVarCharMax;
            if (type == typeof (long)) return DataType.BigInt;
            if (type == typeof (bool)) return DataType.Bit;
            if (type == typeof (DateTime)) return DataType.DateTime;
            if (type == typeof (byte)) return DataType.TinyInt;
            if (type == typeof (char)) return DataType.NChar(1);
            if (type == typeof (char[])) return DataType.NVarCharMax;
            if (type == typeof (decimal)) return DataType.Money;
            if (type == typeof (float)) return DataType.Float;
            if (type == typeof (int)) return DataType.Int;
            if (type == typeof (sbyte)) return DataType.TinyInt;
            if (type == typeof (short)) return DataType.SmallInt;
            if (type == typeof (Guid)) return DataType.UniqueIdentifier;
            throw new NotSupportedException(string.Format("Type {0} isn't supported", type));
        }
    }
}