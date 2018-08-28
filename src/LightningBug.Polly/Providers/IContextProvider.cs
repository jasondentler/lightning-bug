using System.Reflection;

namespace LightningBug.Polly.Providers
{
    public interface IContextProvider
    {
        CallContextBase GetContext(MethodInfo methodInfo, object[] args);
    }
}