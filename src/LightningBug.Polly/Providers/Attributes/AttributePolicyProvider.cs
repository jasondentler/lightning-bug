using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Polly;

namespace LightningBug.Polly.Providers.Attributes
{
    public abstract class AttributePolicyProvider<TAttribute> : IAttributePolicyProvider where TAttribute : PolicyAttribute
    {
        public abstract ISyncPolicy GetSyncPolicy(MethodInfo methodInfo, TAttribute attribute);
        public abstract IAsyncPolicy GetAsyncPolicy(MethodInfo methodInfo, TAttribute attribute);
        public virtual ISyncPolicy GetSyncPolicy(MethodInfo methodInfo, PolicyAttribute attribute)
        {
            if (attribute is TAttribute) return GetSyncPolicy(methodInfo, (TAttribute) attribute);
            return null;
        }

        public virtual IAsyncPolicy GetAsyncPolicy(MethodInfo methodInfo, PolicyAttribute attribute)
        {
            if (attribute is TAttribute) return GetAsyncPolicy(methodInfo, (TAttribute)attribute);
            return null;
        }
    }

    public class AttributePolicyProvider : IPolicyProvider
    {
        private readonly IAttributePolicyProvider[] _attributePolicyProviders;

        public AttributePolicyProvider(params IAttributePolicyProvider[] attributePolicyProviders)
        {
            _attributePolicyProviders = attributePolicyProviders;
        }

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

        protected virtual ISyncPolicy GetSyncPolicy(MethodInfo methodInfo, PolicyAttribute attribute)
        {
            var policies = _attributePolicyProviders
                .Select(provider => provider.GetSyncPolicy(methodInfo, attribute))
                .ToArray();

            if (!policies.Any()) return null;
            if (policies.Length == 1) return policies.Single();

            return Policy.Wrap(policies);
        }

        protected virtual IAsyncPolicy GetAsyncPolicy(MethodInfo methodInfo, PolicyAttribute attribute)
        {
            var policies = _attributePolicyProviders
                .Select(provider => provider.GetAsyncPolicy(methodInfo, attribute))
                .ToArray();

            if (!policies.Any()) return null;
            if (policies.Length == 1) return policies.Single();

            return Policy.WrapAsync(policies);
        }

        protected virtual IEnumerable<PolicyAttribute> CustomSort(IEnumerable<PolicyAttribute> attributes)
        {
            return attributes;
        }
    }
}