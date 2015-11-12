using System;
using System.Collections.Generic;
using LightningBug.Data.ETL.Operations;

namespace LightningBug.Data.ETL.Pipelines
{
    public class SimplePipeline : AbstractPipeline
    {
        protected override void ExecuteOperation<TInput, TOuput>(IOperation<TInput, TOuput> operation, IEnumerable<TInput> input, Action<IEnumerable<TOuput>> nextOperationCallback)
        {
            var output = operation.Execute(input);
            nextOperationCallback(output);
        }
    }
}