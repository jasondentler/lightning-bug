using System;
using System.Collections.Generic;
using System.Reflection;
using LightningBug.Polly.Providers.Attributes;

namespace LightningBug.Polly.Caching
{
    public abstract class CacheAttribute : PolicyAttribute
    {
        public int Order { get; set; }
        public double TimeToLiveInMinutes { get; }

        protected CacheAttribute(double timeToLiveInMinutes = 24 * 60)
        {
            if (timeToLiveInMinutes <= 0)
                throw new ArgumentOutOfRangeException(nameof(timeToLiveInMinutes));
            TimeToLiveInMinutes = timeToLiveInMinutes;
        }

        protected internal override int GetOrder()
        {
            return Order;
        }

        protected internal abstract string GetCacheKey(MethodInfo methodInfo, IDictionary<string, Type> parameterTypes, IDictionary<string, object> arguments);
    }

}
