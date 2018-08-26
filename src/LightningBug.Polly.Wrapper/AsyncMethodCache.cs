using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace LightningBug.Polly
{
    internal static class AsyncMethodCache<TService>
    {
        private static readonly IDictionary<MethodInfo, Func<TService, object[], Task<object>>> Methods;

        static AsyncMethodCache()
        {
            Methods = typeof(TService)
                .GetMethods()
                .Where(mi => !GettersCache<TService>.Handles(mi))
                .Where(mi => !SettersCache<TService>.Handles(mi))
                .Where(mi => !IndexedGettersCache<TService>.Handles(mi))
                .Where(mi => !IndexedSettersCache<TService>.Handles(mi))
                .Where(mi => mi.IsAsync())
                .ToDictionary(mi => mi, mi => mi.BuildAsyncDelegate<TService>());
        }

        public static bool Handles(MethodInfo methodInfo)
        {
            return Methods.ContainsKey(methodInfo);
        }

        public static Task<object> Call(TService instance, MethodInfo methodInfo, object[] args)
        {
            var func = Methods[methodInfo];
            return func(instance, args);
        }

    }
}