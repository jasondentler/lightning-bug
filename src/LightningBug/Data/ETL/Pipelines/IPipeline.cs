using LightningBug.Data.ETL.Operations;

namespace LightningBug.Data.ETL.Pipelines
{
    public interface IPipeline
    {
        void AddOperation<TInput, TOutput>(IOperation<TInput, TOutput> operation);
        void Execute();
    }
}