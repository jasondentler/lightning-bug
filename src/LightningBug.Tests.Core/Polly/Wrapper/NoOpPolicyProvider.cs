using System.Reflection;
using LightningBug.Polly.Providers;
using Polly;

namespace LightningBug.Polly.Wrapper
{
    public class NoOpPolicyProvider : IPolicyProvider
    {
        public ISyncPolicy GetSyncPolicy(MethodInfo methodInfo)
        {
            return Policy.NoOp();
        }

        public IAsyncPolicy GetAsyncPolicy(MethodInfo methodInfo)
        {
            return Policy.NoOpAsync();
        }
    }
}