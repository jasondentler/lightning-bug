using System.Reflection;
using Polly;

namespace LightningBug.Polly.Providers.Attributes
{
    public interface IAttributePolicyProvider
    {
        ISyncPolicy GetSyncPolicy(CallContextBase context, PolicyAttribute attribute);
        IAsyncPolicy GetAsyncPolicy(CallContextBase context, PolicyAttribute attribute);
    }
}