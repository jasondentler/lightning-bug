using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LightningBug.Data.ETL.Operations
{
    public class FinalOperation<TInput> : IOperation<TInput, object>
    {
        public IEnumerable<object> Execute(IEnumerable<TInput> input)
        {
            long itemCount = 0;
            foreach (var item in input)
            {
                //NOOP
                itemCount++;
            }
            Debug.WriteLine(string.Format("Result has {0} rows", itemCount));
            return Enumerable.Empty<object>();
        }

        public string Name { get { return "Final Operation"; } }
    }
}
