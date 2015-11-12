using System.Collections.Generic;
using System.Linq;

namespace LightningBug.Data.ETL.Operations
{
    public class FinalOperation<TInput> : IOperation<TInput, object>
    {
        public IEnumerable<object> Execute(IEnumerable<TInput> input)
        {
            foreach (var item in input)
            {
                //NOOP
            }
            return Enumerable.Empty<object>();
        }

        public string Name { get { return "Final Operation"; } }
    }
}
