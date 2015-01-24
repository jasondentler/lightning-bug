using System;
using System.Collections.Generic;
using System.Linq;

namespace LightningBug.Data
{
    /// <summary>
    /// Caches delegates for setting properties
    /// </summary>
    /// <typeparam name="TInstanceType">Type to set values on</typeparam>
    public static class SetterDelegateCache<TInstanceType>
    {
        // ReSharper disable StaticFieldInGenericType
        private static readonly Dictionary<string, Type> TypeMap;
        private static readonly Dictionary<string, Action<TInstanceType, object>> Setters;
        private static readonly Dictionary<int, Action<TInstanceType, object>> OrdinalSetters;
        private static readonly Dictionary<string, int> Ordinals;
        // ReSharper restore StaticFieldInGenericType

        static SetterDelegateCache()
        {
            var properties = typeof (TInstanceType)
                .GetProperties()
                .Where(p => p.CanWrite)
                .ToArray();

            TypeMap = properties
                .ToDictionary(pi => pi.Name, pi => pi.PropertyType);

            Setters = properties
                .ToDictionary(pi => pi.Name, pi => pi.BuildSetDelegate<TInstanceType>());

            Ordinals = Setters.Keys.Select((s, i) => new {s, i})
                .ToDictionary(x => x.s, x => x.i);

            OrdinalSetters = Setters.ToDictionary(kv => Ordinals[kv.Key], kv => kv.Value);
        }

        /// <summary>
        /// Names of properties with setters
        /// </summary>
        public static IEnumerable<string> WritablePropertyNames
        {
            get { return Setters.Keys; }
        }

        /// <summary>
        /// Count of properties with setters
        /// </summary>
        public static int WritablePropertyCount
        {
            get { return Setters.Count; }
        }

        /// <summary>
        /// Types of properties with setters
        /// </summary>
        public static IDictionary<string, Type> Types
        {
            get { return new Dictionary<string, Type>(TypeMap); }
        }

        /// <summary>
        /// Ordinal values of properties with setters
        /// </summary>
        public static IDictionary<string, int> OrdinalLookup
        {
            get { return new Dictionary<string, int>(Ordinals); }
        }

        /// <summary>
        /// Sets value of <paramref name="propertyName"/> property on <paramref name="instance"/>.
        /// </summary>
        /// <param name="propertyName">Name of property to set</param>
        /// <param name="instance">Instance to write <paramref name="value"/> to</param>
        /// <param name="value">Value to write to <paramref name="propertyName"/></param>
        public static void Write(string propertyName, TInstanceType instance, object value)
        {
            if (Types[propertyName] == typeof (DateTime))
                value = DateTime.Parse(value.ToString());
            Setters[propertyName](instance, value);
        }

        /// <summary>
        /// Sets value of <paramref name="ordinal"/> property on <paramref name="instance"/>.
        /// </summary>
        /// <param name="ordinal">Name of property to set</param>
        /// <param name="instance">Instance to write <paramref name="value"/> to</param>
        /// <param name="value">Value to write to <paramref name="ordinal"/> property</param>
        public static void Write(int ordinal, TInstanceType instance, object value)
        {
            OrdinalSetters[ordinal](instance, value);
        }
    }
}