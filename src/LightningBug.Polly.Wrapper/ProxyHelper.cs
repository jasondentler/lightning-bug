﻿using System;
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
                {
                    Set(service, policyProvider, methodInfo, args.Single());
                    return null;
                }

                if (IndexedSettersCache<TService>.Handles(methodInfo))
                {
                    Set(service, policyProvider, methodInfo, args.Take(args.Length - 1).ToArray(), args.Last());
                    return null;
                }

                if (SyncMethodCache<TService>.Handles(methodInfo))
                    return Call(service, policyProvider, methodInfo, args);

                if (AsyncMethodCache<TService>.Handles(methodInfo))
                    return CallAsync(service, policyProvider, methodInfo, args);

                throw new NotSupportedException($"The method {methodInfo} is not supported.");
            }
            catch (TargetInvocationException ex)
            {
                throw ex.GetBaseException() ?? ex.InnerException ?? ex;
            }
        }

        private static object Get(TService service, TPolicyProvider policyProvider, MethodInfo methodInfo)
        {
            return Execute(() => GettersCache<TService>.Get(service, methodInfo), methodInfo, policyProvider);
        }

        private static object Get(TService service, TPolicyProvider policyProvider, MethodInfo methodInfo, object[] indexParameters)
        {
            return Execute(() => IndexedGettersCache<TService>.Get(service, methodInfo, indexParameters), methodInfo, policyProvider);
        }

        private static void Set(TService service, TPolicyProvider policyProvider, MethodInfo methodInfo, object value)
        {
            Execute(() => SettersCache<TService>.Set(service, methodInfo, value), methodInfo, policyProvider);
        }

        private static void Set(TService service, TPolicyProvider policyProvider, MethodInfo methodInfo, object[] indexParameters, object value)
        {
            Execute(() => IndexedSettersCache<TService>.Set(service, methodInfo, indexParameters, value), methodInfo, policyProvider);
        }

        private static object Call(
            TService service, 
            TPolicyProvider policyProvider, 
            MethodInfo methodInfo, 
            object[] args)
        {
            var cb = new Func<object>(() => SyncMethodCache<TService>.Call(service, methodInfo, args));
            return Execute(cb, methodInfo, policyProvider);
        }

        private static async Task<object> CallAsync(
            TService service,
            TPolicyProvider policyProvider,
            MethodInfo methodInfo,
            object[] args)
        {
            var cb = new Func<Task<object>>(() => AsyncMethodCache<TService>.Call(service, methodInfo, args));
            return await ExecuteAsync(cb, methodInfo, policyProvider);
        }

        private static void Execute(Action cb, MethodInfo methodInfo, TPolicyProvider provider)
        {
            var policy = provider.GetSyncPolicy(methodInfo);

            if (policy == null)
            {
                cb();
                return;
            }

            var result = policy.ExecuteAndCapture(cb);

            if (result.Outcome != OutcomeType.Successful)
                throw new TargetInvocationException(result.FinalException);
        }

        private static object Execute(Func<object> cb, MethodInfo methodInfo, TPolicyProvider provider)
        {
            var policy = provider.GetSyncPolicy(methodInfo);

            if (policy == null)
            {
                return cb();
            }

            var result = policy.ExecuteAndCapture(cb);

            if (result.Outcome != OutcomeType.Successful)
                throw new TargetInvocationException(result.FinalException);

            return result.Result;
        }

        private static async Task<object> ExecuteAsync(Func<Task<object>> cb, MethodInfo methodInfo, TPolicyProvider provider)
        {
            var policy = provider.GetAsyncPolicy(methodInfo);

            if (policy == null)
            {
                return await cb();
            }

            var result = await policy.ExecuteAndCaptureAsync(cb);

            if (result.Outcome != OutcomeType.Successful)
                throw new TargetInvocationException(result.FinalException);

            return result.Result;
        }
    }
}