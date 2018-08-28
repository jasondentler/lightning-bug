using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LightningBug.Polly
{
    internal static class SettersCache<TService>
    {
        private static readonly IDictionary<MethodInfo, Action<TService, object>> Setters;

        static SettersCache()
        {
            Setters = typeof(TService)
                .GetProperties()
                .Where(propertyInfo => propertyInfo.CanWrite)
                .Where(propertyInfo => !propertyInfo.GetIndexParameters().Any())
                .Select(propertyInfo => new { propertyInfo, propertyInfo.SetMethod })
                .ToDictionary(x => x.SetMethod, x => x.propertyInfo.BuildSetDelegate<TService>());
        }

        public static bool Handles(MethodInfo methodInfo)
        {
            return Setters.ContainsKey(methodInfo);
        }

        public static void Set(TService instance, MethodInfo methodInfo, object value)
        {
            var action = Setters[methodInfo];
            action(instance, value);
        }
    }
}