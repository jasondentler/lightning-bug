namespace LightningBug.Polly.Retry
{

    public class RetryAttribute : RetryAttributeBase
    {
        public int MaxRetries { get; }

        public RetryAttribute(int maxRetries)
        {
            MaxRetries = maxRetries;
        }
    }
}
