using System;
using System.Linq;

namespace LightningBug.Polly.Retry
{
    public class WaitAndRetryAttribute : RetryAttributeBase
    {
        public TimeSpan[] Retries { get; }
        public bool UseJitter { get; set; }

        public WaitAndRetryAttribute(params double[] retiesInSeconds) : this(retiesInSeconds.Select(TimeSpan.FromSeconds).ToArray())
        {
            UseJitter = true;
        }

        public WaitAndRetryAttribute(params TimeSpan[] retries)
        {
            Retries = retries;
        }
    }

}