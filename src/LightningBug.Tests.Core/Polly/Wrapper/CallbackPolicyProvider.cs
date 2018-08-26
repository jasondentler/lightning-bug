using System;
using System.Reflection;
using Polly;

namespace LightningBug.Polly.Wrapper
{
    public class CallbackPolicyProvider : IPolicyProvider
    {

        public delegate void CallbackDelegate(MethodInfo method, object[] arguments);

        private readonly CallbackDelegate _callback;

        public CallbackPolicyProvider(CallbackDelegate callback)
        {
            _callback = callback;
        }

        public class Proxy<T> : DispatchProxy
        {
            public CallbackDelegate Callback { get; set; }
            public T Implementation { get; set; }

            protected override object Invoke(MethodInfo targetMethod, object[] args)
            {
                if (targetMethod.Name.StartsWith("Execute"))
                    Callback(targetMethod, args);

                return targetMethod.Invoke(Implementation, args);
            }
        }


        public ISyncPolicy GetSyncPolicy()
        {
            var iface = DispatchProxy.Create<ISyncPolicy, Proxy<ISyncPolicy>>();
            var proxy = iface as Proxy<ISyncPolicy>;
            proxy.Callback = _callback;
            proxy.Implementation = Policy.NoOp();
            return iface;
        }

        public IAsyncPolicy GetAsyncPolicy()
        {
            var iface = DispatchProxy.Create<IAsyncPolicy, Proxy<IAsyncPolicy>>();
            var proxy = iface as Proxy<IAsyncPolicy>;
            proxy.Callback = _callback;
            proxy.Implementation = Policy.NoOpAsync();
            return iface;
        }
    }
}