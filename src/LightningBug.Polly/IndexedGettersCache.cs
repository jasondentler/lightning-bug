using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LightningBug.Polly
{
    internal static class IndexedGettersCache<TService>
    {

        private static readonly IDictionary<MethodInfo, Func<TService, object[], object>> Getters;

        static IndexedGettersCache()
        {
            Getters = typeof(TService)
                .GetProperties()
                .Where(propertyInfo => propertyInfo.CanRead)
                .Where(propertyInfo => propertyInfo.GetIndexParameters().Any())
                .Select(propertyInfo => new { propertyInfo, propertyInfo.GetMethod })
                .ToDictionary(x => x.GetMethod, x => x.propertyInfo.BuildIndexedGetDelegate<TService>());
        }

        public static bool Handles(MethodInfo methodInfo)
        {
            return Getters.ContainsKey(methodInfo);
        }

        public static object Get(TService instance, MethodInfo methodInfo, object[] indexParameters)
        {
            return Getters[methodInfo](instance, indexParameters);
        }
    }
}