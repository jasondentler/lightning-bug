using System;
using LightningBug.Polly.Providers.Attributes;

namespace LightningBug.Polly.CircuitBreaker
{
    public class CircuitBreakerAttribute : PolicyAttribute, ICircuitBreakerAttribute
    {

        public CircuitBreakerAttribute()
        {
            ExceptionsAllowedBeforeBreaking = 5;
            DurationOfBreakInMinutes = 1;
        }

        public int Order { get; set; }

        public int ExceptionsAllowedBeforeBreaking { get; set; }

        public double DurationOfBreakInMinutes
        {
            get => DurationOfBreak.TotalMinutes;
            set => DurationOfBreak = TimeSpan.FromMinutes(value);
        }
        public TimeSpan DurationOfBreak { get; set; }

        protected internal override int GetOrder()
        {
            return Order;
        }

        int ICircuitBreakerAttribute.GetExceptionsAllowedBeforeBreaking()
        {
            return ExceptionsAllowedBeforeBreaking;
        }

        TimeSpan ICircuitBreakerAttribute.GetDurationOfBreak()
        {
            return DurationOfBreak;
        }

    }
}