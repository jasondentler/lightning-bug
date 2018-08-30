using System;

namespace LightningBug.Polly.CircuitBreaker.Advanced
{
    public interface IAdvancedCircuitBreakerAttribute
    {
        double GetFailureThreshold();
        TimeSpan GetSamplingDuration();
        int GetMinimumThroughput();
        TimeSpan GetDurationOfBreak();
    }
}