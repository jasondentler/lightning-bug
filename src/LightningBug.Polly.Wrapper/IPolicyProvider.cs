namespace LightningBug.Polly
{
    public interface IPolicyProvider
    {
        global::Polly.ISyncPolicy GetSyncPolicy();
        global::Polly.IAsyncPolicy GetAsyncPolicy();
    }

}