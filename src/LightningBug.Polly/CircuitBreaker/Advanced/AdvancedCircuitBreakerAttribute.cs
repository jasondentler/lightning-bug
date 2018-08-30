using System;
using LightningBug.Polly.Providers.Attributes;

namespace LightningBug.Polly.CircuitBreaker.Advanced
{
    public class AdvancedCircuitBreakerAttribute : PolicyAttribute, IAdvancedCircuitBreakerAttribute
    {

        public AdvancedCircuitBreakerAttribute()
        {
            FailureThreshold = 1;
            SamplingDuration = TimeSpan.FromMinutes(5);
            DurationOfBreak = TimeSpan.FromMinutes(0.5);
            MinimumThroughput = 80;
        }

        public double FailureThreshold { get; set; }

        public double SamplingDurationInMinutes
        {
            get => SamplingDuration.TotalMinutes;
            set => SamplingDuration = TimeSpan.FromMinutes(value);
        }
        public TimeSpan SamplingDuration { get; set; }
        public int MinimumThroughput { get; set; }

        public double DurationOfBreakInMinutes
        {
            get => DurationOfBreak.TotalMinutes;
            set => DurationOfBreak = TimeSpan.FromMinutes(value);
        }
        public TimeSpan DurationOfBreak { get; set; }
        public int Order { get; set; }

        protected internal override int GetOrder()
        {
            return Order;
        }

        double IAdvancedCircuitBreakerAttribute.GetFailureThreshold()
        {
            return FailureThreshold;
        }

        TimeSpan IAdvancedCircuitBreakerAttribute.GetSamplingDuration()
        {
            return SamplingDuration;
        }

        int IAdvancedCircuitBreakerAttribute.GetMinimumThroughput()
        {
            return MinimumThroughput;
        }

        TimeSpan IAdvancedCircuitBreakerAttribute.GetDurationOfBreak()
        {
            return DurationOfBreak;
        }

    }
}