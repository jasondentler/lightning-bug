using System;
using LightningBug.Polly.Providers.Attributes;
using Polly.Timeout;

namespace LightningBug.Polly.Timeout
{
    public abstract class TimeoutAttributeBase : PolicyAttribute
    {
        public virtual int Order { get; set; }
        protected internal override int GetOrder()
        {
            return Order;
        }

        protected internal abstract TimeSpan GetTimeout();
        protected internal abstract TimeoutStrategy GetTimeoutStrategy();
    }
}