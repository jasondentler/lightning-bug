using System;
using Polly.Timeout;

namespace LightningBug.Polly.Timeout
{
    public class TimeoutAttribute : TimeoutAttributeBase
    {
        public virtual TimeSpan Timeout { get; set; }

        public virtual double TimeoutInSections
        {
            get => Timeout.TotalSeconds;
            set => Timeout = TimeSpan.FromSeconds(value);
        }

        public virtual TimeoutStrategy TimeoutStrategy { get; set; }


        protected internal override TimeSpan GetTimeout()
        {
            return Timeout;
        }

        protected internal override TimeoutStrategy GetTimeoutStrategy()
        {
            return TimeoutStrategy;
        }
    }
}