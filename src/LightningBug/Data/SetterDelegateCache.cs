using System;
using System.Collections.Generic;
using System.Linq;

namespace LightningBug.Data
{

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

        public static IEnumerable<string> WritablePropertyNames
        {
            get { return Setters.Keys; }
        }

        public static int WritablePropertyCount
        {
            get { return Setters.Count; }
        }

        public static IDictionary<string, Type> Types
        {
            get { return new Dictionary<string, Type>(TypeMap); }
        }

        public static IDictionary<string, int> OrdinalLookup
        {
            get { return new Dictionary<string, int>(Ordinals); }
        }

        public static void Write(string propertyName, TInstanceType instance, object value)
        {
            if (Types[propertyName] == typeof (DateTime))
                value = DateTime.Parse(value.ToString());
            Setters[propertyName](instance, value);
        }

        public static void Write(int ordinal, TInstanceType instance, object value)
        {
            OrdinalSetters[ordinal](instance, value);
        }
    }
}