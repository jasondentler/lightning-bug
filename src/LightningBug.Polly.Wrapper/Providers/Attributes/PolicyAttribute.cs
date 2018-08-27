using System;

namespace LightningBug.Polly.Providers.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public abstract class PolicyAttribute : Attribute
    {
        protected internal abstract int GetOrder();
    }
}