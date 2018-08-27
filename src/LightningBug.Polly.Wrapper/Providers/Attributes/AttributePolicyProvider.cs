using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Polly;

namespace LightningBug.Polly.Providers.Attributes
{
    public abstract class AttributePolicyProvider : IPolicyProvider
    {
        public ISyncPolicy GetSyncPolicy(MethodInfo methodInfo)
        {
            var attributes = methodInfo.GetCustomAttributes<PolicyAttribute>();
            var policies = attributes
                .OrderBy(attribute => attribute.GetOrder())
                .Select(attribute => GetSyncPolicy(methodInfo, attribute))
                .Where(policy => policy != null)
                .ToArray();

            if (!policies.Any())
                return null;

            if (policies.Length == 1)
                return policies.Single();

            return Policy.Wrap(policies);
        }

        public IAsyncPolicy GetAsyncPolicy(MethodInfo methodInfo)
        {
            var attributes = methodInfo.GetCustomAttributes<PolicyAttribute>();
            var policies = attributes
                .GroupBy(attribute => attribute.GetOrder())
                .OrderBy(g => g.Key) // Always use the specified order, if there is one
                .SelectMany(CustomSort) // Then sort to break the ties
                .Select(attribute => GetAsyncPolicy(methodInfo, attribute))
                .Where(policy => policy != null)
                .ToArray();

            if (!policies.Any())
                return null;

            if (policies.Length == 1)
                return policies.Single();

            return Policy.WrapAsync(policies);
        }

        protected abstract ISyncPolicy GetSyncPolicy(MethodInfo methodInfo, PolicyAttribute attribute);
        protected abstract IAsyncPolicy GetAsyncPolicy(MethodInfo methodInfo, PolicyAttribute attribute);

        protected virtual IEnumerable<PolicyAttribute> CustomSort(IEnumerable<PolicyAttribute> attributes)
        {
            return attributes;
        }
    }
}