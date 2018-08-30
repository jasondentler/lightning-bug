using System;
using System.Collections.Concurrent;
using Polly;

namespace LightningBug.Polly.Providers.Attributes.Scope
{

    public class SimpleScopePolicyProvider<TAttribute> : ScopePolicyProvider<TAttribute> where TAttribute : PolicyAttribute
    {
        public SimpleScopePolicyProvider(IAttributePolicyProvider inner, Scopes scope) : base(inner, new ScopeProvider(scope))
        {
        }
    }

    public class ScopePolicyProvider<TAttribute> : AttributePolicyProviderBase<TAttribute> where TAttribute : PolicyAttribute
    {
        private readonly IAttributePolicyProvider _inner;
        private readonly IScopeProvider _scopeProvider;
        private readonly ConcurrentDictionary<Guid, object> _scopes = new ConcurrentDictionary<Guid, object>();
        private readonly ConcurrentDictionary<object, ISyncPolicy> _syncPolicies = new ConcurrentDictionary<object, ISyncPolicy>();
        private readonly ConcurrentDictionary<object, IAsyncPolicy> _asyncPolicies = new ConcurrentDictionary<object, IAsyncPolicy>();

        public ScopePolicyProvider(IAttributePolicyProvider inner, IScopeProvider scopeProvider)
        {
            _inner = inner;
            _scopeProvider = scopeProvider;
        }

        public override ISyncPolicy GetSyncPolicy(CallContextBase context, TAttribute attribute)
        {
            return GetPolicy(context, attribute, _syncPolicies, (ctx, attr) => _inner.GetSyncPolicy(ctx, attribute));
        }

        public override IAsyncPolicy GetAsyncPolicy(CallContextBase context, TAttribute attribute)
        {
            return GetPolicy(context, attribute, _asyncPolicies, (ctx, attr) => _inner.GetAsyncPolicy(ctx, attribute));
        }

        private TPolicy GetPolicy<TPolicy>(CallContextBase context, TAttribute attribute, ConcurrentDictionary<object, TPolicy> policies, Func<CallContextBase, TAttribute, TPolicy> getNewPolicy) where TPolicy : IsPolicy
        {
            var scopeId = Guid.NewGuid();

            while (!_scopes.TryAdd(scopeId, null)) // Reserve the id so we can't get a collision due to a race
                scopeId = Guid.NewGuid();

            var scope = _scopeProvider.GetScope(attribute, context, () => OnScopeComplete(scopeId));

            _scopes[scopeId] = scope; // Now record the scope that matches with this id

            lock (scope) // Because GetOrAdd can call the add func and still fail
            {
                var policy = policies.GetOrAdd(scope, _ => getNewPolicy(context, attribute));
                return policy;
            }
        }

        private void OnScopeComplete(Guid scopeId)
        {
            if (!_scopes.TryGetValue(scopeId, out var scope)) // Scope has already been removed
                return;

            _syncPolicies.TryRemove(scope, out _);
            _asyncPolicies.TryRemove(scope, out _);
            _scopes.TryRemove(scopeId, out _);
        }
    }
}