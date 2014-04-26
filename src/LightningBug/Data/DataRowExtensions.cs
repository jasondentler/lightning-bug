using System;
using System.Data;

namespace LightningBug.Data
{
    public static class DataRowExtensions
    {
        public static T ToInstance<T>(this DataRow row) where T : new()
        {
            return row.ToInstance(() => new T());
        }

        public static T ToInstance<T>(this DataRow row, Func<T> constructor)
        {
            var retval = constructor();

            foreach (var propertyName in SetterDelegateCache<T>.WritablePropertyNames)
            {
                var value = row.IsNull(propertyName) ? null : row[propertyName];
                SetterDelegateCache<T>.Write(propertyName, retval, value);
            }

            return retval;
        }
    }
}