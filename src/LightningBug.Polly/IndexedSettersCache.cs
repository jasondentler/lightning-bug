using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LightningBug.Polly
{
    internal static class IndexedSettersCache<TService>
    {
        private static readonly IDictionary<MethodInfo, Action<TService, object[], object>> Setters;

        static IndexedSettersCache()
        {
            Setters = typeof(TService)
                .GetProperties()
                .Where(propertyInfo => propertyInfo.CanWrite)
                .Where(propertyInfo => propertyInfo.GetIndexParameters().Any())
                .Select(propertyInfo => new { propertyInfo, propertyInfo.SetMethod })
                .ToDictionary(x => x.SetMethod, x => x.propertyInfo.BuildIndexedSetDelegate<TService>());
        }

        public static bool Handles(MethodInfo methodInfo)
        {
            return Setters.ContainsKey(methodInfo);
        }

        public static void Set(TService instance, MethodInfo methodInfo, object[] indexParameters, object value)
        {
            Setters[methodInfo](instance, indexParameters, value);
        }
    }
}