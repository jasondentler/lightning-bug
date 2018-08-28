using System.Reflection;

namespace LightningBug.Polly.Providers
{
    public interface IPolicyProvider
    {
        global::Polly.ISyncPolicy GetSyncPolicy(MethodInfo methodInfo);
        global::Polly.IAsyncPolicy GetAsyncPolicy(MethodInfo methodInfo);
    }
}