using Polly;

namespace LightningBug.Polly.Providers.Attributes
{
    public abstract class AttributePolicyProviderBase<TAttribute> : IAttributePolicyProvider where TAttribute : PolicyAttribute
    {
        public abstract ISyncPolicy GetSyncPolicy(CallContextBase context, TAttribute attribute);
        public abstract IAsyncPolicy GetAsyncPolicy(CallContextBase context, TAttribute attribute);
        public virtual ISyncPolicy GetSyncPolicy(CallContextBase context, PolicyAttribute attribute)
        {
            if (attribute is TAttribute) return GetSyncPolicy(context, (TAttribute) attribute);
            return null;
        }

        public virtual IAsyncPolicy GetAsyncPolicy(CallContextBase context, PolicyAttribute attribute)
        {
            if (attribute is TAttribute) return GetAsyncPolicy(context, (TAttribute)attribute);
            return null;
        }
    }
}