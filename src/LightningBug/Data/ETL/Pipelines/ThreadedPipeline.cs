using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LightningBug.Data.ETL.Operations;

namespace LightningBug.Data.ETL.Pipelines
{
    public class ThreadedPipeline : AbstractPipeline
    {

        protected override void ExecuteOperation<TInput, TOuput>(IOperation<TInput, TOuput> operation, IEnumerable<TInput> input, Action<IEnumerable<TOuput>> nextOperationCallback)
        {
            var result = Task.Run(() =>
            {
                var output = operation.Execute(input);
                nextOperationCallback(output);
            });

            result.Wait();
        }
    }
}