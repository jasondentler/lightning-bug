using System;
using System.Collections.Generic;
using System.Reflection;

namespace LightningBug.Polly.Caching
{
    public class SingleCacheEntryAttribute : CacheAttribute
    {
        public string CacheKey { get; }

        public SingleCacheEntryAttribute(string cacheKey, double timeToLiveInMinutes = 24 * 60) : base(timeToLiveInMinutes)
        {
            CacheKey = cacheKey;
        }

        protected internal override string GetCacheKey(MethodInfo methodInfo, IDictionary<string, Type> parameterTypes, IDictionary<string, object> arguments)
        {
            return CacheKey;
        }
    }
}