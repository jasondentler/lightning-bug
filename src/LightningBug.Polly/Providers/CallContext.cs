using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LightningBug.Polly.Providers
{
    public class CallContext : CallContextBase
    {
        public override Type ServiceType { get; }
        public override object Instance { get; }
        public override MethodInfo Method { get; }
        public override IDictionary<string, object> Arguments { get; }
        public override IDictionary<string, Type> ParameterTypes { get; }

        public CallContext(Type serviceType, object instance, MethodInfo methodInfo, object[] args)
        {
            ServiceType = serviceType;
            Instance = instance;
            Method = methodInfo;

            ParameterTypes = methodInfo.GetParameters()
                .ToDictionary(pi => pi.Name, pi => pi.ParameterType);

            Arguments = methodInfo.GetParameters()
                .Select((pi, index) => new {pi, value = args[index]})
                .ToDictionary(x => x.pi.Name, x => x.value);
        }
    }
}