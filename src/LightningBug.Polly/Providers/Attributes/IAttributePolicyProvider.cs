using System.Reflection;
using Polly;

namespace LightningBug.Polly.Providers.Attributes
{
    public interface IAttributePolicyProvider
    {
        ISyncPolicy GetSyncPolicy(MethodInfo methodInfo, PolicyAttribute attribute);
        IAsyncPolicy GetAsyncPolicy(MethodInfo methodInfo, PolicyAttribute attribute);
    }
}