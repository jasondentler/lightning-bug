using System;
using System.Reflection;

namespace LightningBug.Polly.Providers
{
    public class ContextProvider : IContextProvider
    {
        public CallContextBase GetContext(Type serviceType, object instance, MethodInfo methodInfo, object[] args)
        {
            return new CallContext(serviceType, instance, methodInfo, args);
        }
    }
}