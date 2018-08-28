using System.Reflection;

namespace LightningBug.Polly.Providers
{
    public class ContextProvider : IContextProvider
    {
        public CallContextBase GetContext(MethodInfo methodInfo, object[] args)
        {
            return new CallContext(methodInfo, args);
        }
    }
}