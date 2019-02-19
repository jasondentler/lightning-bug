using System.Reflection;
using LightningBug.Polly.Providers;
using Polly;

namespace LightningBug.Polly.Wrapper
{
    public class NullPolicyProvider : IPolicyProvider
    {
        public ISyncPolicy GetSyncPolicy(CallContextBase context)
        {
            return null;
        }

        public IAsyncPolicy GetAsyncPolicy(CallContextBase context)
        {
            return null;
        }
    }
}