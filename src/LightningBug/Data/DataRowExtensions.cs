using System;
using System.Data;
using LightningBug.Reflection;

namespace LightningBug.Data
{
    public static class DataRowExtensions
    {

        /// <summary>
        /// Creates a new object instance from a data row
        /// </summary>
        /// <typeparam name="T">The type of object to create</typeparam>
        /// <param name="row">DataRow containing values</param>
        /// <returns>A new instance of <typeparamref name="T"/> filled with values from <paramref name="row"/>.</returns>
        /// <remarks>Uses the default constructor of <typeparamref name="T"/> to create an instance.</remarks>
        public static T ToInstance<T>(this DataRow row) where T : new()
        {
            return row.ToInstance(() => new T());
        }

        /// <summary>
        /// Creates a new object instance from a data row
        /// </summary>
        /// <typeparam name="T">The type of object to create</typeparam>
        /// <param name="row">DataRow containing values</param>
        /// <param name="constructor">Method used to create a new instance.</param>
        /// <returns>A new instance of <typeparamref name="T"/> filled with values from <paramref name="row"/>.</returns>
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