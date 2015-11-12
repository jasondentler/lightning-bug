using System.Collections.Generic;
using LightningBug.Data.ETL.Enumerators;

namespace LightningBug.Data.ETL.Operations
{
    public class MultiThreadedOperationWrapper<TInput, TOutput> : IOperation<TInput, TOutput>
    {
        private readonly IOperation<TInput, TOutput> operation;

        public MultiThreadedOperationWrapper(IOperation<TInput, TOutput> operation)
        {
            this.operation = operation;
        }

        public virtual IEnumerable<TOutput> Execute(IEnumerable<TInput> input)
        {
            var threadSafe = new ThreadSafeEnumerator<TInput>();
            var wrapper = new ChainedEnumeratorWrapper<TInput>(input, threadSafe);
            return operation.Execute(wrapper);
        }

        public string Name { get { return operation.Name; } }

    }
}