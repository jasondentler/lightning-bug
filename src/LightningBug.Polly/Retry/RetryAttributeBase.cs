using LightningBug.Polly.Providers.Attributes;

namespace LightningBug.Polly.Retry
{
    public abstract class RetryAttributeBase : PolicyAttribute
    {
        public int Order { get; set; }
        protected internal override int GetOrder()
        {
            return Order;
        }
    }
}