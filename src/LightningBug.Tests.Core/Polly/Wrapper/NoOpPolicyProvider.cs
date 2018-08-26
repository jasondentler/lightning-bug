using Polly;

namespace LightningBug.Polly.Wrapper
{
    public class NoOpPolicyProvider : IPolicyProvider
    {
        public ISyncPolicy GetSyncPolicy()
        {
            return Policy.NoOp();
        }

        public IAsyncPolicy GetAsyncPolicy()
        {
            return Policy.NoOpAsync();
        }
    }
}