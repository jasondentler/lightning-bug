using System;
using System.Collections.Generic;
using System.Reflection;
using LightningBug.Polly.Providers;
using Polly;
using Polly.Caching;

namespace LightningBug.Polly.Caching
{
    public class AttributeCacheKeyStrategy : ICacheKeyStrategy
    {
        private readonly CacheAttribute _attribute;

        public AttributeCacheKeyStrategy(CacheAttribute attribute)
        {
            _attribute = attribute;
        }

        public string GetCacheKey(Context context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var callContext = context as CallContextBase;

            if (callContext == null)
                throw new ArgumentException(
                    $"Expected an implementation of CallContextBase, got {context.GetType().FullName}",
                    nameof(context));
            
            return GetCacheKey(callContext);
        }

        public string GetCacheKey(CallContextBase context)
        {
            return GetCacheKey(context.Method, context.ParameterTypes, context.Arguments);
        }

        public string GetCacheKey(MethodInfo method, IDictionary<string, Type> parameterTypes, IDictionary<string, object> arguments)
        {
            return _attribute.GetCacheKey(method, parameterTypes, arguments);
        }
    }
}