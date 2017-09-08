namespace LightningBug.Data.ETL.Pipelines
{
    public class ThreadPoolPipelineTests : PipelineTests<ThreadedPipeline>
    {
        protected override ThreadedPipeline CreateNewPipeline()
        {
            return new ThreadedPipeline();
        }
    }
}
