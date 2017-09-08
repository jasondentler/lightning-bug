using System;
using System.Collections;
using System.Collections.Generic;

namespace LightningBug.Data.ETL.Enumerators
{
    public class ChainedEnumeratorWrapper<T> : IEnumerator<T>, IEnumerable<T>
    {
        private readonly IEnumerator<T> source;
        private readonly IChainedEnumerator<T> next;

        public ChainedEnumeratorWrapper(IEnumerable<T> source, IChainedEnumerator<T> next) : this(source == null ? null : source.GetEnumerator(), next) {}

        public ChainedEnumeratorWrapper(IEnumerator<T> source, IChainedEnumerator<T> next)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (next == null) throw new ArgumentNullException(nameof(next));
            this.source = source;
            this.next = next;
        }

        public void Dispose()
        {
            //NOOP
        }

        public bool MoveNext()
        {
            if (source.MoveNext())
            {
                next.AddItem(Current);
                return true;
            }
            next.MarkAsFinished();
            return false;
        }

        public void Reset()
        {
            source.Reset();
            next.Reset();
        }

        public T Current { get { return source.Current; } }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
