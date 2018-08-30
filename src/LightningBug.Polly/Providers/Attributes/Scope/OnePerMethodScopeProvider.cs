using System;

namespace LightningBug.Polly.Providers.Attributes.Scope
{
    internal class OnePerMethodScopeProvider : IScopeProvider
    {

        public static readonly OnePerMethodScopeProvider Instance = new OnePerMethodScopeProvider();

        private OnePerMethodScopeProvider()
        {
        }

        public object GetScope(PolicyAttribute attribute, CallContextBase context, Action onScopeComplete)
        {
            return context.Method;
        }
    }
}