using System;
using System.Collections.Generic;
using System.Linq;
using LightningBug.Data;

namespace LightningBug.Reflection
{
    /// <summary>
    /// Caches delegates for reading properties
    /// </summary>
    /// <typeparam name="TInstanceType">Type to read from</typeparam>
    public static class GetterDelegateCache<TInstanceType>
    {
        // ReSharper disable StaticFieldInGenericType
        private static readonly Dictionary<string, Type> TypeMap;
        private static readonly Dictionary<string, Func<TInstanceType, object>> Getters;
        private static readonly Dictionary<int, Func<TInstanceType, object>> OrdinalGetters;
        private static readonly Dictionary<string, int> Ordinals;
        // ReSharper restore StaticFieldInGenericType

        static GetterDelegateCache()
        {
            var properties = typeof (TInstanceType)
                .GetProperties()
                .Where(p => p.CanRead)
                .ToArray();

            TypeMap = properties
                .ToDictionary(pi => pi.Name, pi => pi.PropertyType);

            Getters = properties
                .ToDictionary(pi => pi.Name, pi => pi.BuildGetDelegate<TInstanceType>());

            Ordinals = Getters.Keys.Select((s, i) => new {s, i})
                .ToDictionary(x => x.s, x => x.i);

            OrdinalGetters = Getters
                .ToDictionary(kv => Ordinals[kv.Key], kv => kv.Value);
        }

        /// <summary>
        /// Names of properties with getters
        /// </summary>
        public static IEnumerable<string> ReadablePropertyNames
        {
            get { return Getters.Keys; }
        }

        /// <summary>
        /// Count of properties with getters
        /// </summary>
        public static int ReadablePropertyCount
        {
            get { return Getters.Count; }
        }

        /// <summary>
        /// Types of properties with getters
        /// </summary>
        public static IDictionary<string, Type> Types
        {
            get { return new Dictionary<string, Type>(TypeMap); }
        }

        /// <summary>
        /// Ordinals of properties with getters
        /// </summary>
        public static IDictionary<string, int> OrdinalLookup
        {
            get { return new Dictionary<string, int>(Ordinals); }
        }

        /// <summary>
        /// Gets value of <paramref name="propertyName"/> property on <paramref name="instance"/>.
        /// </summary>
        /// <param name="propertyName">Name of property to read</param>
        /// <param name="instance">Instance to read value from</param>
        /// <returns>Value of <paramref name="propertyName"/> property on <paramref name="instance"/>.</returns>
        public static object Read(string propertyName, TInstanceType instance)
        {
            return Getters[propertyName](instance);
        }

        /// <summary>
        /// Gets value of <paramref name="ordinal"/> property on <paramref name="instance"/>.
        /// </summary>
        /// <param name="ordinal">Ordinal value of the property to read</param>
        /// <param name="instance">Instance to read value from</param>
        /// <returns>Value of <paramref name="ordinal"/> property on <paramref name="instance"/>.</returns>
        public static object Read(int ordinal, TInstanceType instance)
        {
            return OrdinalGetters[ordinal](instance);
        }
    }
}