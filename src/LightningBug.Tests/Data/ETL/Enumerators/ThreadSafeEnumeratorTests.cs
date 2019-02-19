using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Shouldly;
using Xunit;

namespace LightningBug.Data.ETL.Enumerators
{
    public class ThreadSafeEnumeratorTests
    {

        [Fact]
        public void RunsUntilOutOfItems()
        {
            var expected = new object[] {1, 2, 3, 4, 5};

            var threadSafeEnumerator = new ThreadSafeEnumerator<object>();

            foreach (var item in expected)
                threadSafeEnumerator.AddItem(item);
            threadSafeEnumerator.MarkAsFinished();

            var result = threadSafeEnumerator.ToArray();

            result.ShouldBe(expected);
        }

        [Fact]
        public void WaitsForItems()
        {
            var expected = new int[] { 1, 2, 3, 4, 5 };
            var threadSafeEnumerator = new ThreadSafeEnumerator<int>();
            var results =  new ConcurrentQueue<int>();

            var consumer = new Thread(() =>
            {
                foreach (var result in threadSafeEnumerator)
                    results.Enqueue(result);
            });

            results.ShouldBeEmpty();

            consumer.Start();

            foreach (var item in expected)
                threadSafeEnumerator.AddItem(item);
            threadSafeEnumerator.MarkAsFinished();

            consumer.Join(TimeSpan.FromSeconds(3)); // Wait for the consumer to finish

            results.ToArray().ShouldBe(expected);
        }
    }
}
