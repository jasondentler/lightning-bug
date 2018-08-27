using System.Reflection;
using LightningBug.Polly.Providers;
using Polly;

namespace LightningBug.Polly.Wrapper
{
    public class NullPolicyProvider : IPolicyProvider
    {
        public ISyncPolicy GetSyncPolicy(MethodInfo methodInfo)
        {
            return null;
        }

        public IAsyncPolicy GetAsyncPolicy(MethodInfo methodInfo)
        {
            return null;
        }
    }
}