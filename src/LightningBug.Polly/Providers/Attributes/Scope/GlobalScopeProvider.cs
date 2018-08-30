using System;

namespace LightningBug.Polly.Providers.Attributes.Scope
{
    internal class GlobalScopeProvider : IScopeProvider
    {

        public static readonly GlobalScopeProvider Instance = new GlobalScopeProvider();

        private GlobalScopeProvider()
        {
        }

        public object GetScope(PolicyAttribute attribute, CallContextBase context, Action onScopeComplete)
        {
            return typeof(object);
        }
    }
}