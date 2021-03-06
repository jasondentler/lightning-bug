﻿using System;
using System.Reflection;
using LightningBug.Polly.Providers;

namespace LightningBug.Polly
{
    public static class PollyWrapper<TService> where TService : class 
    {

        static PollyWrapper()
        {
            if (typeof(TService).IsClass)
                throw new NotSupportedException($"Wrappers of classes are not supported. {nameof(TService)} should be an interface.");
        }

        public static TService Decorate<TPolicyProvider>(TService implementation, TPolicyProvider policyProvider, IContextProvider contextProvider)
            where TPolicyProvider : class, IPolicyProvider
        {
            if (implementation == null) throw new ArgumentNullException(nameof(implementation));
            if (policyProvider == null) throw new ArgumentNullException(nameof(policyProvider));
            if (contextProvider == null) throw new ArgumentNullException(nameof(contextProvider));
            var proxyAsInterface = DispatchProxy.Create<TService, Proxy<TService, TPolicyProvider>>();
            var proxyAsWrapper = proxyAsInterface as Proxy<TService, TPolicyProvider> ;
            ProxyHelper<TService, TPolicyProvider>.InitializeProxy(proxyAsWrapper, implementation, policyProvider, contextProvider);
            return proxyAsInterface;
        }

    }
}
