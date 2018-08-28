using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LightningBug.Polly
{
    internal static class GettersCache<TService>
    {

        private static readonly IDictionary<MethodInfo, Func<TService, object>> Getters;

        static GettersCache()
        {
            Getters = typeof(TService)
                .GetProperties()
                .Where(propertyInfo => propertyInfo.CanRead)
                .Where(propertyInfo => !propertyInfo.GetIndexParameters().Any())
                .Select(propertyInfo => new {propertyInfo, propertyInfo.GetMethod})
                .ToDictionary(x => x.GetMethod, x => x.propertyInfo.BuildGetDelegate<TService>());
        }

        public static bool Handles(MethodInfo methodInfo)
        {
            return Getters.ContainsKey(methodInfo);
        }

        public static object Get(TService instance, MethodInfo methodInfo)
        {
            var func = Getters[methodInfo];
            return func(instance);
        }
    }
}