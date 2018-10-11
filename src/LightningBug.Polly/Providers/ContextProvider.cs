using System;
using System.Reflection;

namespace LightningBug.Polly.Providers
{
    public class ContextProvider : IContextProvider
    {
        public CallContextBase GetContext(Type serviceType, object instance, MethodInfo methodInfo, object[] args)
        {
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));
            if (args == null) throw new ArgumentNullException(nameof(args));
            return new CallContext(serviceType, instance, methodInfo, args);
        }
    }
}