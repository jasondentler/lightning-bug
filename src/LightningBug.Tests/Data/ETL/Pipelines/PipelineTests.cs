using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LightningBug.Data.ETL.Operations;
using Moq;
using SharpTestsEx;
using Xunit;

namespace LightningBug.Data.ETL.Pipelines
{

    public abstract class PipelineTests<TPipeline> where TPipeline : IPipeline
    {

        private class TestOperation<TInput, TOuput>: IOperation<TInput, TOuput>
        {
            private readonly string name;
            private readonly Func<IEnumerable<TInput>, IEnumerable<TOuput>> execute;

            public TestOperation(string name, Func<IEnumerable<TInput>, IEnumerable<TOuput>> execute)
            {
                this.name = name;
                this.execute = execute ?? (input => Enumerable.Empty<TOuput>());
            }

            public IEnumerable<TOuput> Execute(IEnumerable<TInput> input)
            {
                return execute(input);
            }

            public string Name { get { return name; } }
        }

        protected abstract TPipeline CreateNewPipeline();

        [Fact]
        public void ExecutesNoOperations()
        {
            var pipeline = CreateNewPipeline();
            pipeline.Execute();
        }

        [Fact]
        public void ExecutesASingleOperation()
        {
            var hasExecuted = false;

            var operation = new TestOperation<int, int>("Operation", input =>
            {
                var results = SlowEnumerable(5, TimeSpan.FromSeconds(0.1));
                hasExecuted = true;
                return results;
            });

            var pipeline = CreateNewPipeline();
            pipeline.AddOperation(operation);

            pipeline.Execute();

            hasExecuted.Should().Be.True();
        }

        [Fact]
        public void ExecuteTwoOperationsOfTheSameType()
        {
            var op1HasExecuted = false;
            var op1 = new TestOperation<int, int>("Operation #1", input =>
            {
                var results = SlowEnumerable(5, TimeSpan.FromSeconds(0.1));
                op1HasExecuted = true;
                return results;
            });

            var op2HasExecuted = false;
            var op2 = new TestOperation<int, int>("Operation #2", input =>
            {
                var results = input.Select(i => i + 1);
                op2HasExecuted = true;
                return results;
            });

            var pipeline = CreateNewPipeline();
            pipeline.AddOperation(op1);
            pipeline.AddOperation(op2);

            pipeline.Execute();

            op1HasExecuted.Should().Be.True();
            op2HasExecuted.Should().Be.True();
        }

        [Fact]
        public void ExecuteTwoOperationsOfDifferentTypes()
        {
            var op1HasExecuted = false;
            var op1 = new TestOperation<object, int>("Operation #1", input =>
            {
                var results = SlowEnumerable(5, TimeSpan.FromSeconds(0.1));
                op1HasExecuted = true;
                return results;
            });

            var op2HasExecuted = false;
            var op2 = new TestOperation<int, string>("Operation #2", input =>
            {
                var results = input.Select(i => i + 1).Select(i => i.ToString());
                op2HasExecuted = true;
                return results;
            });

            var pipeline = CreateNewPipeline();
            pipeline.AddOperation(op1);
            pipeline.AddOperation(op2);

            pipeline.Execute();

            op1HasExecuted.Should().Be.True();
            op2HasExecuted.Should().Be.True();
        }

        [Fact]
        public void AddingAnOperationOfIncompatibleInput_Throws()
        {
            var op1 = new Mock<IOperation<int, int>>();
            op1.Setup(o => o.Execute(It.IsAny<IEnumerable<int>>()));
            op1.SetupGet(o => o.Name).Returns("Operation #1");

            var op2 = new Mock<IOperation<Guid, int>>();
            op2.Setup(o => o.Execute(It.IsAny<IEnumerable<Guid>>()));
            op2.SetupGet(o => o.Name).Returns("Operation #2");

            var pipeline = CreateNewPipeline();
            pipeline.AddOperation(op1.Object);

            Assert.Throws<InvalidCastException>(() => pipeline.AddOperation(op2.Object));
        }

        private IEnumerable<int> SlowEnumerable(int count, TimeSpan wait)
        {
            for (int i = 0; i < count; i++)
            {
                Console.WriteLine("Waiting...");
                Thread.Sleep(wait);
                Console.WriteLine("Returning " + i);
                yield return i;
            }
        }
    }
}
