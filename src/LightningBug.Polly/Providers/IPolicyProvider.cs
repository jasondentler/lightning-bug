namespace LightningBug.Polly.Providers
{
    public interface IPolicyProvider
    {
        global::Polly.ISyncPolicy GetSyncPolicy(CallContextBase context);
        global::Polly.IAsyncPolicy GetAsyncPolicy(CallContextBase context);
    }
}