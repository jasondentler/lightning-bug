using System.Linq;
using System.Reflection;
using Polly;

namespace LightningBug.Polly.Providers
{
    public class CompositePolicyProvider : IPolicyProvider
    {
        private readonly IPolicyProvider[] _providers;

        public CompositePolicyProvider(params IPolicyProvider[] providers)
        {
            _providers = providers;
        }

        public ISyncPolicy GetSyncPolicy(MethodInfo methodInfo)
        {
            var policies = _providers
                .Select(policyProvider => policyProvider.GetSyncPolicy(methodInfo))
                .Where(policy => policy != null)
                .ToArray();

            return Policy.Wrap(policies);
        }

        public IAsyncPolicy GetAsyncPolicy(MethodInfo methodInfo)
        {
            var policies = _providers
                .Select(policyProvider => policyProvider.GetAsyncPolicy(methodInfo))
                .Where(policy => policy != null)
                .ToArray();

            return Policy.WrapAsync(policies);
        }
    }
}
