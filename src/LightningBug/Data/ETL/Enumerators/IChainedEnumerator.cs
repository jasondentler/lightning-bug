using System.Collections.Generic;

namespace LightningBug.Data.ETL.Enumerators
{
    public interface IChainedEnumerator<T> : IEnumerator<T>, IEnumerable<T>
    {
        void AddItem(T item);
        void MarkAsFinished();
    }
}