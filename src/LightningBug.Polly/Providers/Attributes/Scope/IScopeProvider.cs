using System;

namespace LightningBug.Polly.Providers.Attributes.Scope
{
    public interface IScopeProvider
    {
        object GetScope(PolicyAttribute attribute, CallContextBase context, Action onScopeComplete);

    }
}