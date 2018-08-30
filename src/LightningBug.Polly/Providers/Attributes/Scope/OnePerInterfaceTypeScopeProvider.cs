using System;

namespace LightningBug.Polly.Providers.Attributes.Scope
{
    internal class OnePerInterfaceTypeScopeProvider : IScopeProvider
    {
        public static readonly OnePerInterfaceTypeScopeProvider Instance = new OnePerInterfaceTypeScopeProvider();

        private OnePerInterfaceTypeScopeProvider()
        {
        }

        public object GetScope(PolicyAttribute attribute, CallContextBase context, Action onScopeComplete)
        {
            return context.ServiceType;
        }
    }
}