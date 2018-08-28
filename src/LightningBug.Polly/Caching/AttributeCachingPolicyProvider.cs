using System;
using System.Collections.Generic;
using System.Reflection;
using LightningBug.Polly.Providers;
using LightningBug.Polly.Providers.Attributes;
using Polly;
using Polly.Caching;

namespace LightningBug.Polly.Caching
{
    public class AttributeCachingPolicyProvider : AttributePolicyProvider<CacheAttribute>
    {
        private readonly ISyncCacheProvider _syncCacheProvider;
        private readonly IAsyncCacheProvider _asyncCacheProvider;

        public AttributeCachingPolicyProvider(ISyncCacheProvider syncCacheProvider,
            IAsyncCacheProvider asyncCacheProvider)
        {
            _syncCacheProvider = syncCacheProvider;
            _asyncCacheProvider = asyncCacheProvider;
        }

        public override ISyncPolicy GetSyncPolicy(MethodInfo methodInfo, CacheAttribute attribute)
        {
            var cacheKeyStrategy = GetCacheKeyStrategy(methodInfo, attribute);
            var ttlStrategy = GetTimeToLiveStrategy(methodInfo, attribute);

            return Policy.Cache(_syncCacheProvider, ttlStrategy, cacheKeyStrategy, OnCacheGet, OnCacheMiss, OnCachePut, OnCacheGetError, OnCachePutError);
        }

        public override IAsyncPolicy GetAsyncPolicy(MethodInfo methodInfo, CacheAttribute attribute)
        {
            var cacheKeyStrategy = GetCacheKeyStrategy(methodInfo, attribute);
            var ttlStrategy = GetTimeToLiveStrategy(methodInfo, attribute);

            return Policy.CacheAsync(_asyncCacheProvider, ttlStrategy, cacheKeyStrategy, OnCacheGet, OnCacheMiss, OnCachePut, OnCacheGetError, OnCachePutError);
        }

        protected virtual ICacheKeyStrategy GetCacheKeyStrategy(MethodInfo methodInfo, CacheAttribute attribute)
        {
            return new AttributeCacheKeyStrategy(attribute);
        }

        protected virtual ITtlStrategy GetTimeToLiveStrategy(MethodInfo methodInfo, CacheAttribute attribute)
        {
            var ttl = TimeSpan.FromMinutes(attribute.TimeToLiveInMinutes);
            return new SlidingTtl(ttl);
        }

        private void OnCachePutError(Context context, string key, Exception exception)
        {
            OnCachePutError((CallContextBase) context, key, exception);
        }

        private void OnCachePutError(CallContextBase context, string key, Exception exception)
        {
            OnCachePutError(context, context.Method, context.ParameterTypes, context.Arguments, key, exception);
        }

        protected virtual void OnCachePutError(Context context, MethodInfo method, IDictionary<string, Type> parameterTypes, IDictionary<string, object> arguments, string key, Exception exception)
        {
        }

        private void OnCacheGetError(Context context, string key, Exception exception)
        {
            OnCacheGetError((CallContextBase) context, key, exception);
        }

        private void OnCacheGetError(CallContextBase context, string key, Exception exception)
        {
            OnCacheGetError(context, context.Method, context.ParameterTypes, context.Arguments, key, exception);
        }

        protected virtual void OnCacheGetError(Context context, MethodInfo method, IDictionary<string, Type> parameterTypes, IDictionary<string, object> arguments, string key, Exception exception)
        {
        }

        private void OnCachePut(Context context, string key)
        {
            OnCachePut((CallContextBase)context, key);
        }

        private void OnCachePut(CallContextBase context, string key)
        {
            OnCachePut(context, context.Method, context.ParameterTypes, context.Arguments, key);
        }

        protected virtual void OnCachePut(Context context, MethodInfo method, IDictionary<string, Type> parameterTypes, IDictionary<string, object> arguments, string key)
        {
        }

        private void OnCacheMiss(Context context, string key)
        {
            OnCacheMiss((CallContextBase)context, key);
        }

        private void OnCacheMiss(CallContextBase context, string key)
        {
            OnCacheMiss(context, context.Method, context.ParameterTypes, context.Arguments, key);
        }

        protected virtual void OnCacheMiss(Context context, MethodInfo method, IDictionary<string, Type> parameterTypes, IDictionary<string, object> arguments, string key)
        {
        }

        private void OnCacheGet(Context context, string key)
        {
            OnCacheGet((CallContextBase)context, key);
        }

        private void OnCacheGet(CallContextBase context, string key)
        {
            OnCacheGet(context, context.Method, context.ParameterTypes, context.Arguments, key);
        }

        protected virtual void OnCacheGet(Context context, MethodInfo method, IDictionary<string, Type> parameterTypes, IDictionary<string, object> arguments, string key)
        {
        }
    }
}