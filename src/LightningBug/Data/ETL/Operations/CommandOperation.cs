using System.Collections.Generic;

namespace LightningBug.Data.ETL.Operations
{

    public abstract class CommandOperation<T> : IOperation<T, T>
    {
        public IEnumerable<T> Execute(IEnumerable<T> input)
        {
            OnExecuting();
            using (var enumerator = input.GetEnumerator())
            {
                var result = enumerator.MoveNext();
                OnInputReady();
                if (result)
                {
                    yield return enumerator.Current;
                    while (enumerator.MoveNext())
                        yield return enumerator.Current;
                }
            }
            OnExecuted();
        }

        /// <summary>
        /// Executes when pipeline starts executing this operation
        /// </summary>
        public virtual void OnExecuting()
        {
        }

        /// <summary>
        /// Executes when the first item is ready, or when the previous operation has no results 
        /// </summary>
        public virtual void OnInputReady()
        {
        }

        /// <summary>
        /// Executes when all items have been processed
        /// </summary>
        public virtual void OnExecuted()
        {
        }

        public abstract string Name { get; }
    }
}
