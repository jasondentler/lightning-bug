using System.Collections.Generic;

namespace LightningBug.Data.ETL.Operations
{
    public interface IOperation
    {
        string Name { get; }
    }

    public interface IOperation<in TInput, out TOutput> : IOperation
    {
        IEnumerable<TOutput> Execute(IEnumerable<TInput> input);
    }
}
