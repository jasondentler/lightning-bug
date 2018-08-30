using System;

namespace LightningBug.Polly.CircuitBreaker
{
    public interface ICircuitBreakerAttribute
    {
        int GetExceptionsAllowedBeforeBreaking();
        TimeSpan GetDurationOfBreak();

    }
}