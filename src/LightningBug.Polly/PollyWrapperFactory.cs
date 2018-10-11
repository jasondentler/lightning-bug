using System;
using LightningBug.Polly.Providers;

namespace LightningBug.Polly
{
    public class PollyWrapperFactory<TService, TServiceImplementation>
        where TService : class
        where TServiceImplementation : class, TService
    {
        private readonly TServiceImplementation _implementation;
        private readonly IPolicyProvider _policyProvider;
        private readonly IContextProvider _contextProvider;

        public PollyWrapperFactory(TServiceImplementation implementation, IPolicyProvider policyProvider, IContextProvider contextProvider)
        {
            _implementation = implementation ?? throw new ArgumentNullException(nameof(implementation));
            _policyProvider = policyProvider ?? throw new ArgumentNullException(nameof(policyProvider));
            _contextProvider = contextProvider ?? throw new ArgumentNullException(nameof(contextProvider));
        }

        public TService Wrap()
        {
            var wrapper = PollyWrapper<TService>.Decorate(_implementation, _policyProvider, _contextProvider);
            return wrapper;
        }
    }
}