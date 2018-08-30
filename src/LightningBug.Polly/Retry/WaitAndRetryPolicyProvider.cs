using System;
using System.Collections.Generic;
using System.Reflection;
using LightningBug.Polly.Providers;
using LightningBug.Polly.Providers.Attributes;
using Polly;

namespace LightningBug.Polly.Retry
{
    public class WaitAndRetryPolicyProvider : WaitAndRetryPolicyProvider<Exception>
    {
    }

    public class WaitAndRetryPolicyProvider<TException> : WaitAndRetryPolicyProvider<TException, WaitAndRetryAttribute> where TException : Exception
    {
    }

    public class WaitAndRetryPolicyProvider<TException, TAttribute> : AttributePolicyProviderBase<TAttribute>
        where TException : Exception
        where TAttribute : WaitAndRetryAttribute
    {
        public override ISyncPolicy GetSyncPolicy(CallContextBase context, TAttribute attribute)
        {
            return Policy
                .Handle<TException>(HandlesException)
                .OrInner<TException>(HandlesInnerException)
                .WaitAndRetry(attribute.Retries,
                    (exception, timeSpan, ctx) => OnRetry(exception, timeSpan, ctx));
        }


        public override IAsyncPolicy GetAsyncPolicy(CallContextBase context, TAttribute attribute)
        {
            return Policy
                .Handle<TException>(HandlesException)
                .OrInner<TException>(HandlesInnerException)
                .WaitAndRetryAsync(attribute.Retries,
                    (exception, timeSpan, ctx) => OnRetry(exception, timeSpan, ctx));
        }

        private void OnRetry(Exception exception, TimeSpan timeSpan, Context context)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));
            if (context == null) throw new ArgumentNullException(nameof(context));
            var callContext = context as CallContextBase;
            if (callContext == null)
                throw new ArgumentException(
                    $"{nameof(context)} should be a Call Context Base but was {context.GetType().FullName}",
                    nameof(context));
            OnRetry(exception, timeSpan, (CallContextBase)context);
        }

        private void OnRetry(Exception exception, TimeSpan timeSpan, CallContextBase context)
        {
            OnRetry(exception, timeSpan, context, context.Method, context.ParameterTypes, context.Arguments);
        }

        protected virtual void OnRetry(Exception exception, TimeSpan timeSpan, Context context, MethodInfo methodInfo,
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