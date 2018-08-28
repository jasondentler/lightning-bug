using System.Reflection;
using System.Threading.Tasks;
using LightningBug.Polly.Providers;

namespace LightningBug.Polly
{
    public class Proxy<TService, TPolicyProvider> : DispatchProxy
        where TService : class
        where TPolicyProvider : class, IPolicyProvider
    {

        internal TService Service { get; set; }
        internal TPolicyProvider PolicyProvider { get; set; }
        public IContextProvider ContextProvider { get; set; }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            var result = ProxyHelper<TService, TPolicyProvider>.Invoke(Service, PolicyProvider, ContextProvider, targetMethod, args);

            return !targetMethod.IsAsyncWithReturnParameter() 
                ? result 
                : CastBackToCorrectTaskType(targetMethod, result);
        }

        private object CastBackToCorrectTaskType(MethodInfo targetMethod, object result)
        {
            var returnType = targetMethod.GetAsyncReturnType();
            var conversionDelegate = TaskConversionCache.MakeSpecific(returnType);
            return conversionDelegate((Task<object>)result);
        }
    }
}