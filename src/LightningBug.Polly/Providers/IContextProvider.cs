using System;
using System.Reflection;

namespace LightningBug.Polly.Providers
{
    public interface IContextProvider
    {
        CallContextBase GetContext(Type serviceType, object instance, MethodInfo methodInfo, object[] args);
    }
}