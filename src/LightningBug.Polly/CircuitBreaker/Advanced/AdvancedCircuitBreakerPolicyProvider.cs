using System;
using System.Collections.Generic;
using System.Reflection;
using LightningBug.Polly.Providers;
using LightningBug.Polly.Providers.Attributes;
using Polly;
using Polly.CircuitBreaker;

namespace LightningBug.Polly.CircuitBreaker.Advanced
{

    public class AdvancedCircuitBreakerPolicyProvider : AdvancedCircuitBreakerPolicyProvider<Exception>
    {
    }

    public class AdvancedCircuitBreakerPolicyProvider<TException> : AdvancedCircuitBreakerPolicyProvider<TException, AdvancedCircuitBreakerAttribute> where TException : Exception
    {
    }

    public class AdvancedCircuitBreakerPolicyProvider<TException, TAttribute> : AttributePolicyProviderBase<TAttribute> 
        where TException : Exception
        where TAttribute : PolicyAttribute, IAdvancedCircuitBreakerAttribute
    {
        public override ISyncPolicy GetSyncPolicy(CallContextBase context, TAttribute attribute)
        {
            var failureThreshold = attribute.GetFailureThreshold();
            var samplingDuration = attribute.GetSamplingDuration();
            var minimumThroughput = attribute.GetMinimumThroughput();
            var durationOfBreak = attribute.GetDurationOfBreak();
            return Policy
                .Handle<TException>(HandlesException)
                .OrInner<TException>(HandlesInnerException)
                .AdvancedCircuitBreaker(failureThreshold, samplingDuration, minimumThroughput, durationOfBreak, OnBreak, OnReset, OnHalfOpen);
        }

        public override IAsyncPolicy GetAsyncPolicy(CallContextBase context, TAttribute attribute)
        {
            var failureThreshold = attribute.GetFailureThreshold();
            var samplingDuration = attribute.GetSamplingDuration();
            var minimumThroughput = attribute.GetMinimumThroughput();
            var durationOfBreak = attribute.GetDurationOfBreak();
            return Policy
                .Handle<TException>(HandlesException)
                .OrInner<TException>(HandlesInnerException)
                .AdvancedCircuitBreakerAsync(failureThreshold, samplingDuration, minimumThroughput, durationOfBreak, OnBreak, OnReset, OnHalfOpen);
        }

        protected virtual void OnHalfOpen()
        {
        }

        private void OnReset(Context context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var callContext = context as CallContextBase;
            if (callContext == null)
                throw new ArgumentException(
                    $"{nameof(context)} should be a Call Context Base but was {context.GetType().FullName}",
                    nameof(context));
            OnReset((CallContextBase)context);
        }

        private void OnReset(CallContextBase context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            OnReset(context, context.Method, context.ParameterTypes, context.Arguments);
        }

        protected virtual void OnReset(Context context, MethodInfo methodInfo,
            IDictionary<string, Type> parameterTypes, IDictionary<string, object> arguments)
        {
        }

        private void OnBreak(Exception exception, CircuitState circuitState, TimeSpan durationOfBreak, Context context)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));
            if (context == null) throw new ArgumentNullException(nameof(context));
            var callContext = context as CallContextBase;
            if (callContext == null)
                throw new ArgumentException(
                    $"{nameof(context)} should be a Call Context Base but was {context.GetType().FullName}",
                    nameof(context));
            OnBreak(exception, circuitState, durationOfBreak, callContext);
        }

        private void OnBreak(Exception exception, CircuitState circuitState, TimeSpan durationOfBreak, CallContextBase context)
        {
            OnBreak(exception, circuitState, durationOfBreak, context, context.Method, context.ParameterTypes, context.Arguments);
        }

        protected virtual void OnBreak(Exception exception, CircuitState circuitState, TimeSpan durationOfBreak, Context context, MethodInfo methodInfo,
            IDictionary<string, Type> parameterTypes, IDictionary<string, object> arguments)
        {
        }

        public virtual bool HandlesException(TException exception)
        {
            return true;
        }

        public virtual bool HandlesInnerException(TException exception)
        {
            return true;
        }
    }
}