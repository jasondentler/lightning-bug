using System;

namespace LightningBug.Polly.Providers.Attributes.Scope
{
    internal class OnePerConcreteTypeScopeProvider : IScopeProvider
    {
        public static readonly OnePerConcreteTypeScopeProvider Instance = new OnePerConcreteTypeScopeProvider();

        private OnePerConcreteTypeScopeProvider()
        {
        }

        public object GetScope(PolicyAttribute attribute, CallContextBase context, Action onScopeComplete)
        {
            return context.Instance.GetType();
        }
    }

}