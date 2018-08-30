using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Polly;

namespace LightningBug.Polly.Providers.Attributes
{
    public class AttributePolicyProvider : IPolicyProvider
    {
        private readonly IAttributePolicyProvider[] _attributePolicyProviders;

        public AttributePolicyProvider(params IAttributePolicyProvider[] attributePolicyProviders)
        {
            if (attributePolicyProviders == null) throw new ArgumentNullException(nameof(attributePolicyProviders));

            if (!attributePolicyProviders.Any())
                throw new ArgumentException($"At least one {nameof(IAttributePolicyProvider)} is required.");

            _attributePolicyProviders = attributePolicyProviders;
        }

        public ISyncPolicy GetSyncPolicy(CallContextBase context)
        {
            var methodInfo = context.Method;
            var attributes = methodInfo.GetCustomAttributes<PolicyAttribute>();
            var policies = attributes
                .OrderBy(attribute => attribute.GetOrder())
                .Select(attribute => GetSyncPolicy(context, attribute))
                .Where(policy => policy != null)
                .ToArray();

            if (!policies.Any())
                return null;

            if (policies.Length == 1)
                return policies.Single();

            return Policy.Wrap(policies);
        }

        public IAsyncPolicy GetAsyncPolicy(CallContextBase context)
        {
            var methodInfo = context.Method;
            var attributes = methodInfo.GetCustomAttributes<PolicyAttribute>();
            var policies = attributes
                .GroupBy(attribute => attribute.GetOrder())
                .OrderBy(g => g.Key) // Always use the specified order, if there is one
                .SelectMany(CustomSort) // Then sort to break the ties
                .Select(attribute => GetAsyncPolicy(context, attribute))
                .Where(policy => policy != null)
                .ToArray();

            if (!policies.Any())
                return null;

            if (policies.Length == 1)
                return policies.Single();

            return Policy.WrapAsync(policies);
        }

        protected virtual ISyncPolicy GetSyncPolicy(CallContextBase context, PolicyAttribute attribute)
        {
            var policies = _attributePolicyProviders
                .Select(provider => provider.GetSyncPolicy(context, attribute))
                .Where(policy => policy != null)
                .ToArray();

            if (!policies.Any()) return null;
            if (policies.Length == 1) return policies.Single();

            return Policy.Wrap(policies);
        }

        protected virtual IAsyncPolicy GetAsyncPolicy(CallContextBase context, PolicyAttribute attribute)
        {
            var policies = _attributePolicyProviders
                .Select(provider => provider.GetAsyncPolicy(context, attribute))
                .Where(policy => policy != null)
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