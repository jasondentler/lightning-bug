using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LightningBug.Polly.Providers;
using Polly;

namespace LightningBug.Polly
{
    internal static class ProxyHelper<TService, TPolicyProvider>
        where TService : class
        where TPolicyProvider : class, IPolicyProvider
    {
        public static void InitializeProxy(
            Proxy<TService, TPolicyProvider> proxy,
            TService service,
            TPolicyProvider policyProvider,
            IContextProvider contextProvider)
        {
            proxy.Service = service;
            proxy.PolicyProvider = policyProvider;
            proxy.ContextProvider = contextProvider;
        }

        public static object Invoke(
            TService service,
            TPolicyProvider policyProvider,
            IContextProvider contextProvider,
            MethodInfo methodInfo,
            object[] args)
        {
            try
            {
                if (GettersCache<TService>.Handles(methodInfo))
                    return Get(service, policyProvider, methodInfo, contextProvider);

                if (IndexedGettersCache<TService>.Handles(methodInfo))
                    return Get(service, policyProvider, methodInfo, args, contextProvider);

                if (SettersCache<TService>.Handles(methodInfo))
                {
                    Set(service, policyProvider, methodInfo, args.Single(), contextProvider);
                    return null;
                }

                if (IndexedSettersCache<TService>.Handles(methodInfo))
                {
                    Set(service, policyProvider, methodInfo, args.Take(args.Length - 1).ToArray(), args.Last(), contextProvider);
                    return null;
                }

                if (SyncMethodCache<TService>.Handles(methodInfo))
                    return Call(service, policyProvider, methodInfo, args, contextProvider);

                if (AsyncMethodCache<TService>.Handles(methodInfo))
                    return CallAsync(service, policyProvider, methodInfo, args, contextProvider);

                throw new NotSupportedException($"The method {methodInfo} is not supported.");
            }
            catch (TargetInvocationException ex)
            {
                throw ex.GetBaseException() ?? ex.InnerException ?? ex;
            }
        }

        private static object Get(TService service, TPolicyProvider policyProvider, MethodInfo methodInfo, IContextProvider contextProvider)
        {
            return Execute(() => GettersCache<TService>.Get(service, methodInfo), service, methodInfo, new object[0],  policyProvider, contextProvider);
        }

        private static object Get(TService service, TPolicyProvider policyProvider, MethodInfo methodInfo, object[] indexParameters, IContextProvider contextProvider)
        {
            return Execute(() => IndexedGettersCache<TService>.Get(service, methodInfo, indexParameters), service, methodInfo, indexParameters, policyProvider, contextProvider);
        }

        private static void Set(TService service, TPolicyProvider policyProvider, MethodInfo methodInfo, object value, IContextProvider contextProvider)
        {
            Execute(() => SettersCache<TService>.Set(service, methodInfo, value), service, methodInfo, new []{value}, policyProvider, contextProvider);
        }

        private static void Set(TService service, TPolicyProvider policyProvider, MethodInfo methodInfo, object[] indexParameters, object value, IContextProvider contextProvider)
        {
            Execute(() => IndexedSettersCache<TService>.Set(service, methodInfo, indexParameters, value), service, methodInfo, indexParameters.Concat(new []{value}).ToArray(), policyProvider, contextProvider);
        }

        private static object Call(
            TService service, 
            TPolicyProvider policyProvider, 
            MethodInfo methodInfo, 
            object[] args,
            IContextProvider contextProvider)
        {
            var cb = new Func<object>(() => SyncMethodCache<TService>.Call(service, methodInfo, args));
            return Execute(cb, service, methodInfo, args, policyProvider, contextProvider);
        }

        private static async Task<object> CallAsync(
            TService service,
            TPolicyProvider policyProvider,
            MethodInfo methodInfo,
            object[] args,
            IContextProvider contextProvider)
        {
            var cb = new Func<Task<object>>(() => AsyncMethodCache<TService>.Call(service, methodInfo, args));
            return await ExecuteAsync(cb, service, methodInfo, args, policyProvider, contextProvider);
        }

        private static void Execute(Action cb, TService service, MethodInfo methodInfo, object[] args, TPolicyProvider provider, IContextProvider contextProvider)
        {
            var context = contextProvider.GetContext(typeof(TService), service, methodInfo, args);
            var policy = provider.GetSyncPolicy(context);

            if (policy == null)
            {
                cb();
                return;
            }

            var result = policy.ExecuteAndCapture(ctx => cb(), context);

            if (result.Outcome != OutcomeType.Successful)
                throw new TargetInvocationException(result.FinalException);
        }

        private static object Execute(Func<object> cb, TService service, MethodInfo methodInfo, object[] args, TPolicyProvider provider, IContextProvider contextProvider)
        {
            var context = contextProvider.GetContext(typeof(TService), service, methodInfo, args);
            var policy = provider.GetSyncPolicy(context);

            if (policy == null)
            {
                return cb();
            }

            var result = policy.ExecuteAndCapture(ctx => cb(), context);

            if (result.Outcome != OutcomeType.Successful)
                throw new TargetInvocationException(result.FinalException);

            return result.Result;
        }

        private static async Task<object> ExecuteAsync(Func<Task<object>> cb, TService service, MethodInfo methodInfo, object[] args, TPolicyProvider provider, IContextProvider contextProvider)
        {
            var context = contextProvider.GetContext(typeof(TService), service, methodInfo, args);
            var policy = provider.GetAsyncPolicy(context);

            if (policy == null)
            {
                return await cb();
            }

            var result = await policy.ExecuteAndCaptureAsync(ctx => cb(), context);

            if (result.Outcome != OutcomeType.Successful)
                throw new TargetInvocationException(result.FinalException);

            return result.Result;
        }
    }
}