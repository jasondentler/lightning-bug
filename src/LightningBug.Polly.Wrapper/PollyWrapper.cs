using System;
using System.Reflection;

namespace LightningBug.Polly
{
    public static class PollyWrapper<TService> where TService : class 
    {

        static PollyWrapper()
        {
            if (typeof(TService).IsClass)
                throw new NotSupportedException($"Wrappers of classes are not supported. {nameof(TService)} should be an interface.");
        }

        public static TService Decorate<TPolicyProvider>(TService implementation, TPolicyProvider policyProvider)
            where TPolicyProvider : class, IPolicyProvider
        {
            var proxyAsInterface = DispatchProxy.Create<TService, Proxy<TService, TPolicyProvider>>();
            var proxyAsWrapper = proxyAsInterface as Proxy<TService, TPolicyProvider> ;
            ProxyHelper<TService, TPolicyProvider>.InitializeProxy(proxyAsWrapper, implementation, policyProvider);
            return proxyAsInterface;
        }

    }
}
