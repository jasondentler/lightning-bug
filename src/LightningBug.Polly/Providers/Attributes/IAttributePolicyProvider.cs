using System.Reflection;
using Polly;

namespace LightningBug.Polly.Providers.Attributes
{
    public interface IAttributePolicyProvider<in TAttribute> : IAttributePolicyProvider where TAttribute : PolicyAttribute
    {
        ISyncPolicy GetSyncPolicy(CallContextBase context, TAttribute attribute);
        IAsyncPolicy GetAsyncPolicy(CallContextBase context, TAttribute attribute);
    }

    public interface IAttributePolicyProvider
    {
        ISyncPolicy GetSyncPolicy(CallContextBase context, PolicyAttribute attribute);
        IAsyncPolicy GetAsyncPolicy(CallContextBase context, PolicyAttribute attribute);
    }
}