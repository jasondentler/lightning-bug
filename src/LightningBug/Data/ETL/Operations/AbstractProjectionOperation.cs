using System.Collections.Generic;
using System.Linq;

namespace LightningBug.Data.ETL.Operations
{
    public abstract class AbstractProjectionOperation<TInput, TOutput> : IOperation<TInput, TOutput>
    {
        private readonly string _name;

        protected AbstractProjectionOperation(string name)
        {
            _name = name;
        }

        public IEnumerable<TOutput> Execute(IEnumerable<TInput> input)
        {
            return input.Select(Execute);
        }

        protected abstract TOutput Execute(TInput input);

        public string Name
        {
            get { return _name; }
        }
    }
}