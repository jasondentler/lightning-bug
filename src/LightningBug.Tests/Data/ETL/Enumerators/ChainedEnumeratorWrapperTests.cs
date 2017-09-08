using Should;
using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using SharpTestsEx;
using Xunit;

namespace LightningBug.Data.ETL.Enumerators
{
    public class ChainedEnumeratorWrapperTests
    {


        [Fact]
        public void WhenProvidedANullSource_Throws()
        {
            var next = new Mock<IChainedEnumerator<object>>();
            Assert.Throws<ArgumentNullException>(() => new ChainedEnumeratorWrapper<object>((IEnumerable<object>) null, next.Object));
        }

        [Fact]
        public void WhenProvidedANullNext_Throws()
        {
            var source = Enumerable.Empty<object>();
            Assert.Throws<ArgumentNullException>(() => new ChainedEnumeratorWrapper<object>(source, null));
        }

        [Fact]
        public void WhenProvidedSourceAndNext_DoesntThrow()
        {
            var source = Enumerable.Empty<object>();
            var next = new Mock<IChainedEnumerator<object>>();
            new ChainedEnumeratorWrapper<object>(source, next.Object);
        }

        [Fact]
        public void WhenGetEnumerator_ReturnsResults()
        {
            var source = new[] {new object(), new object(), new object()};
            var next = new Mock<IChainedEnumerator<object>>();
            var wrapper = new ChainedEnumeratorWrapper<object>(source, next.Object);
            wrapper.ToArray().Should().Have.SameSequenceAs(source);
        }

        [Fact]
        public void WhenEnumerating_CurrentMatchesSource()
        {
            var source = new object[] {1, 2, 3, 4}.AsEnumerable().GetEnumerator();
            var next = new Mock<IChainedEnumerator<object>>();
            var wrapper = new ChainedEnumeratorWrapper<object>(source, next.Object);
            wrapper.MoveNext();
            source.Current.ShouldEqual(wrapper.Current);
        }

        [Fact]
        public void WhenEnumerating_ShouldReturnTrueUntilNoMoreItems()
        {
            var source = new object[] { 1, 2, 3 };

            var next = new Mock<IChainedEnumerator<object>>();

            var wrapper = new ChainedEnumeratorWrapper<object>(source, next.Object);

            wrapper.MoveNext().Should().Be.True();
            wrapper.MoveNext().Should().Be.True();
            wrapper.MoveNext().Should().Be.True();
            wrapper.MoveNext().Should().Be.False();
        }

        [Fact]
        public void WhenEnumerating_ResetShouldResetSource()
        {
            var source = new Mock<IEnumerator<object>>();
            source.Setup(x => x.MoveNext()).Returns(true);
            source.Setup(x => x.Reset());

            var next = new Mock<IChainedEnumerator<object>>();
            next.Setup(x => x.AddItem(It.IsAny<object>()));
            next.Setup(x => x.Reset());

            var wrapper = new ChainedEnumeratorWrapper<object>(source.Object, next.Object);

            wrapper.MoveNext();
            wrapper.MoveNext();
            wrapper.MoveNext();
            wrapper.Reset();

            source.Verify(x => x.MoveNext(), Times.Exactly(3));
            source.Verify(x => x.Reset(), Times.Once);
        }

        [Fact]
        public void WhenEnumerating_ResetShouldResetNext()
        {
            var source = new Mock<IEnumerator<object>>();
            source.Setup(x => x.MoveNext()).Returns(true);
            source.Setup(x => x.Reset());

            var next = new Mock<IChainedEnumerator<object>>();
            next.Setup(x => x.AddItem(It.IsAny<object>()));
            next.Setup(x => x.Reset());

            var wrapper = new ChainedEnumeratorWrapper<object>(source.Object, next.Object);

            wrapper.MoveNext();
            wrapper.MoveNext();
            wrapper.MoveNext();
            wrapper.Reset();

            next.Verify(x => x.AddItem(It.IsAny<object>()), Times.Exactly(3));
            next.Verify(x => x.Reset(), Times.Once);
        }

        [Fact]
        public void WhenEnumerating_NoMoreItemsShouldMarkNextAsFinished()
        {
            var source = new object[] {1, 2, 3};

            var next = new Mock<IChainedEnumerator<object>>();
            next.Setup(x => x.AddItem(It.IsAny<object>()));
            next.Setup(x => x.MarkAsFinished());

            var wrapper = new ChainedEnumeratorWrapper<object>(source, next.Object);

            wrapper.MoveNext();
            wrapper.MoveNext();
            wrapper.MoveNext();
            wrapper.MoveNext();

            next.Verify(x => x.AddItem(It.IsAny<object>()), Times.Exactly(3));
            next.Verify(x => x.MarkAsFinished(), Times.Once);
        }


    }
}
