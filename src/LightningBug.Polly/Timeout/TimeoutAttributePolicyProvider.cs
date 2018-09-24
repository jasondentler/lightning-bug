using System;
using System.Threading.Tasks;
using LightningBug.Polly.Providers;
using LightningBug.Polly.Providers.Attributes;
using Polly;

namespace LightningBug.Polly.Timeout
{
    public class TimeoutAttributePolicyProvider : TimeoutAttributePolicyProvider<TimeoutAttribute>
    {
    }

    public class TimeoutAttributePolicyProvider<TTimeoutAttribute> : AttributePolicyProviderBase<TTimeoutAttribute> where TTimeoutAttribute : TimeoutAttributeBase
    {
        public override ISyncPolicy GetSyncPolicy(CallContextBase context, TTimeoutAttribute attribute)
        {
            return Policy.Timeout(attribute.GetTimeout(), attribute.GetTimeoutStrategy(), OnTimeout);
        }

        public override IAsyncPolicy GetAsyncPolicy(CallContextBase context, TTimeoutAttribute attribute)
        {
            return Policy.TimeoutAsync(attribute.GetTimeout(), attribute.GetTimeoutStrategy(), OnTimeoutAsync);
        }

        private async Task OnTimeoutAsync(Context context, TimeSpan timeout, Task timedOutTask, Exception exception)
        {
            await Task.FromResult<object>(null);
        }

        protected virtual void OnTimeout(Context context, TimeSpan timeout, Task timedOutTask, Exception exception)
        {
        }


    }
}
