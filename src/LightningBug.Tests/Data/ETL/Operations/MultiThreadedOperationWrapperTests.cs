using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using SharpTestsEx;
using Should;
using Xunit;

namespace LightningBug.Data.ETL.Operations
{
    public class MultiThreadedOperationWrapperTests
    {

        [Fact]
        public void WhenExecuting_ReturnsOutput()
        {
            var expected = new[] {1, 2, 3, 4, 5, 6};
            var operation = new Mock<IOperation<int, int>>();

            operation
                .Setup(o => o.Execute(It.IsAny<IEnumerable<int>>()))
                .Returns(new Func<IEnumerable<int>, IEnumerable<int>>(param => param.ToArray()));

            var wrapper = new MultiThreadedOperationWrapper<int, int>(operation.Object);

            var results = wrapper.Execute(expected).ToArray();

            results.Should().Have.SameSequenceAs(expected);
        }

        [Fact]
        public void Wrapper_ReturnsNameOfWrappedOperation()
        {
            var expected = Guid.NewGuid().ToString();
            var operation = new Mock<IOperation<int, int>>();
            operation.SetupGet(o => o.Name).Returns(expected);

            var wrapper = new MultiThreadedOperationWrapper<int, int>(operation.Object);
            wrapper.Name.ShouldEqual(expected);
        }

    }
}
