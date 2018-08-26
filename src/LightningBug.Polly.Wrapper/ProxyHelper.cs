using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace LightningBug.Polly
{
    internal static class ProxyHelper<TService, TPolicyProvider>
        where TService : class
        where TPolicyProvider : class, IPolicyProvider
    {
        public static void InitializeProxy(
            Proxy<TService, TPolicyProvider> proxy,
            TService service,
            TPolicyProvider policyProvider)
        {
            proxy.Service = service;
            proxy.PolicyProvider = policyProvider;
        }

        public static object Invoke(
            TService service,
            TPolicyProvider policyProvider,
            MethodInfo methodInfo,
            object[] args)
        {
            try
            {
                if (GettersCache<TService>.Handles(methodInfo))
                    return Get(service, policyProvider, methodInfo);

                if (IndexedGettersCache<TService>.Handles(methodInfo))
                    return Get(service, policyProvider, methodInfo, args);

                if (SettersCache<TService>.Handles(methodInfo))
                    return Set(service, policyProvider, methodInfo, args.Single());

                if (IndexedSettersCache<TService>.Handles(methodInfo))
                    return Set(service, policyProvider, methodInfo, args.Take(args.Length - 1).ToArray(), args.Last());

                if (SyncMethodCache<TService>.Handles(methodInfo))
                    return Call(service, policyProvider, methodInfo, args);

                if (AsyncMethodCache<TService>.Handles(methodInfo))
                    return CallAsync(service, policyProvider, methodInfo, args);

                throw new NotSupportedException($"The method {methodInfo.ToString()} is not supported.");
            }
            catch (TargetInvocationException ex)
            {
                throw ex.GetBaseException() ?? ex.InnerException ?? ex;
            }
        }

        private static object Get(TService service, TPolicyProvider policyProvider, MethodInfo methodInfo)
        {
            return GettersCache<TService>.Get(service, methodInfo);
        }

        private static object Get(TService service, TPolicyProvider policyProvider, MethodInfo methodInfo, object[] indexParameters)
        {
            return IndexedGettersCache<TService>.Get(service, methodInfo, indexParameters);
        }

        private static object Set(TService service, TPolicyProvider policyProvider, MethodInfo methodInfo, object value)
        {
            SettersCache<TService>.Set(service, methodInfo, value);
            return null;
        }

        private static object Set(TService service, TPolicyProvider policyProvider, MethodInfo methodInfo, object[] indexParameters, object value)
        {
            IndexedSettersCache<TService>.Set(service, methodInfo, indexParameters, value);
            return null;
        }

        private static object Call(
            TService service, 
            TPolicyProvider policyProvider, 
            MethodInfo methodInfo, 
            object[] args)
        {
            return SyncMethodCache<TService>.Call(service, methodInfo, args);
        }

        private static Task<object> CallAsync(
            TService service,
            TPolicyProvider policyProvider,
            MethodInfo methodInfo,
            object[] args)
        {
            return AsyncMethodCache<TService>.Call(service, methodInfo, args);
        }
    }
}