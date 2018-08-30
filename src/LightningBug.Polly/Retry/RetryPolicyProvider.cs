using System;
using System.Collections.Generic;
using System.Reflection;
using LightningBug.Polly.Providers;
using LightningBug.Polly.Providers.Attributes;
using Polly;

namespace LightningBug.Polly.Retry
{

    public class RetryPolicyProvider : RetryPolicyProvider<Exception>
    {
    }

    public class RetryPolicyProvider<TException> : RetryPolicyProvider<TException, RetryAttribute> where TException : Exception
    {
    }

    public class RetryPolicyProvider<TException, TAttribute> : AttributePolicyProviderBase<TAttribute> 
        where TException : Exception
        where TAttribute : RetryAttribute
    {
        public override ISyncPolicy GetSyncPolicy(CallContextBase context, TAttribute attribute)
        {
            return Policy
                .Handle<TException>(HandlesException)
                .OrInner<TException>(HandlesInnerException)
                .Retry(attribute.MaxRetries,
                    (exception, retryNumber, ctx) => OnRetry(exception, retryNumber, ctx));
        }


        public override IAsyncPolicy GetAsyncPolicy(CallContextBase context, TAttribute attribute)
        {
            return Policy
                .Handle<TException>(HandlesException)
                .OrInner<TException>(HandlesInnerException)
                .RetryAsync(attribute.MaxRetries,
                    (exception, retryNumber, ctx) => OnRetry(exception, retryNumber, ctx));
        }

        private void OnRetry(Exception exception, int retryNumber, Context context)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));
            if (context == null) throw new ArgumentNullException(nameof(context));
            var callContext = context as CallContextBase;
            if (callContext == null)
                throw new ArgumentException(
                    $"{nameof(context)} should be a Call Context Base but was {context.GetType().FullName}",
                    nameof(context));
            OnRetry(exception, retryNumber, (CallContextBase) context);
        }

        private void OnRetry(Exception exception, int retryNumber, CallContextBase context)
        {
            OnRetry(exception, retryNumber, context, context.Method, context.ParameterTypes, context.Arguments);
        }

        protected virtual void OnRetry(Exception exception, int retryNumber, Context context, MethodInfo methodInfo,
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