using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace LightningBug.Polly
{
    internal static class AsyncMethodCache<TService>
    {
        private static readonly ConcurrentDictionary<MethodInfo, Func<TService, object[], Task<object>>> Methods;
        private static readonly ISet<MethodInfo> GenericMethods;

        static AsyncMethodCache()
        {
            var methods = typeof(TService)
                .GetMethods()
                .Where(mi => !GettersCache<TService>.Handles(mi))
                .Where(mi => !SettersCache<TService>.Handles(mi))
                .Where(mi => !IndexedGettersCache<TService>.Handles(mi))
                .Where(mi => !IndexedSettersCache<TService>.Handles(mi))
                .Where(mi => mi.IsAsync())
                .ToArray();

            var delegateMap = methods
                .Where(mi => !mi.ContainsGenericParameters)
                .ToDictionary(mi => mi, mi => mi.BuildAsyncDelegate<TService>());

            Methods = new ConcurrentDictionary<MethodInfo, Func<TService, object[], Task<object>>>(delegateMap);
            GenericMethods = new HashSet<MethodInfo>(methods.Except(Methods.Keys));
        }

        public static bool Handles(MethodInfo methodInfo)
        {
            if (Methods.ContainsKey(methodInfo))
                return true;

            if (!methodInfo.IsGenericMethod)
                return false;

            var genericMethodDefinition = methodInfo.GetGenericMethodDefinition();

            return GenericMethods.Contains(genericMethodDefinition);
        }

        public static Task<object> Call(TService instance, MethodInfo methodInfo, object[] args)
        {
            var func = Methods.GetOrAdd(methodInfo, mi => mi.BuildAsyncDelegate<TService>());
            return func(instance, args);
        }

    }
}