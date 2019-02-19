using System.Reflection;
using LightningBug.Polly.Providers;
using Polly;

namespace LightningBug.Polly.Wrapper
{
    public class NoOpPolicyProvider : IPolicyProvider
    {
        public ISyncPolicy GetSyncPolicy(CallContextBase context)
        {
            return Policy.NoOp();
        }

        public IAsyncPolicy GetAsyncPolicy(CallContextBase context)
        {
            return Policy.NoOpAsync();
        }
    }
}