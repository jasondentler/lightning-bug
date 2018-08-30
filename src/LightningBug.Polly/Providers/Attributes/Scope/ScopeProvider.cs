using System;

namespace LightningBug.Polly.Providers.Attributes.Scope
{
    public class ScopeProvider : IScopeProvider
    {
        private readonly Scopes _scope;

        public ScopeProvider(Scopes scope)
        {
            _scope = scope;
        }

        public object GetScope(PolicyAttribute attribute, CallContextBase context, Action onScopeComplete)
        {
            switch (_scope)
            {
                case Scopes.Global:
                    return GlobalScopeProvider.Instance.GetScope(attribute, context, onScopeComplete);
                case Scopes.OnePerInterfaceType:
                    return OnePerInterfaceTypeScopeProvider.Instance.GetScope(attribute, context, onScopeComplete);
                case Scopes.OnePerConcreteType:
                    return OnePerConcreteTypeScopeProvider.Instance.GetScope(attribute, context, onScopeComplete);
                case Scopes.OnePerMethod:
                    return OnePerMethodScopeProvider.Instance.GetScope(attribute, context, onScopeComplete);
                default:
                    throw new NotSupportedException($"Unsupported {nameof(Scopes)} value {_scope}");
            }
        }
    }
}