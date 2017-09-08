using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LightningBug.Data.ETL.Enumerators
{
    public class ThreadSafeEnumerator<T> : IChainedEnumerator<T>
    {

        private bool finished;
        private T current;
        private readonly Queue<T> cached = new Queue<T>();

        public void AddItem(T item)
        {
            lock (cached)
            {
                cached.Enqueue(item);
                Monitor.Pulse(cached);
            }
        }

        public void MarkAsFinished()
        {
            lock (cached)
            {
                finished = true;
                Monitor.Pulse(cached);
            }
        }

        public void Dispose()
        {
            cached.Clear();
        }

        public bool MoveNext()
        {
            lock (cached)
            {
                while (!cached.Any() && !finished) // Not done, just waiting for a previous step
                    Monitor.Wait(cached);

                if (finished && !cached.Any()) // All done
                    return false;

                current = cached.Dequeue();

                return true;
            }
        }

        public void Reset()
        {
            throw new NotSupportedException("Thread safe enumerator is forward-only");
        }

        public T Current { get { return current;} }

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