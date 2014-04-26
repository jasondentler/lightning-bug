using System;
using System.Collections.Generic;
using System.Linq;

namespace LightningBug.Data
{
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

        public static IEnumerable<string> ReadablePropertyNames
        {
            get { return Getters.Keys; }
        }

        public static int ReadablePropertyCount
        {
            get { return Getters.Count; }
        }

        public static IDictionary<string, Type> Types
        {
            get { return new Dictionary<string, Type>(TypeMap); }
        }

        public static IDictionary<string, int> OrdinalLookup
        {
            get { return new Dictionary<string, int>(Ordinals); }
        }

        public static object Read(string propertyName, TInstanceType instance)
        {
            return Getters[propertyName](instance);
        }

        public static object Read(int ordinal, TInstanceType instance)
        {
            return OrdinalGetters[ordinal](instance);
        }
    }
}