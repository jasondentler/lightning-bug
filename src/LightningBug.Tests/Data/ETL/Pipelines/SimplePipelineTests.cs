namespace LightningBug.Data.ETL.Pipelines
{
    public class SimplePipelineTests : PipelineTests<SimplePipeline>
    {
        protected override SimplePipeline CreateNewPipeline()
        {
            return new SimplePipeline();
        }
    }
}