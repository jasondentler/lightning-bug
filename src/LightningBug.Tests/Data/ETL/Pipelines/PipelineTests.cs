using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using LightningBug.Data.ETL.Operations;
using Moq;
using Shouldly;
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

            hasExecuted.ShouldBeTrue();
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

            op1HasExecuted.ShouldBeTrue();
            op2HasExecuted.ShouldBeTrue();
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

            op1HasExecuted.ShouldBeTrue();
            op2HasExecuted.ShouldBeTrue();
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

        [Fact(Skip = "DB")]
        public void ReadDatabaseRecords()
        {
            var connectionString = "Server=.;Database=ETLTest;Integrated Security=SSPI;";
            var connectionProvider = new Func<IDbConnection>(() => new SqlConnection(connectionString));
            var source = new DatabaseSource<Record>(connectionProvider, "SELECT TOP 1000 * FROM TestTableSize");
            var pipeline = CreateNewPipeline();
            pipeline.AddOperation(source);
            pipeline.Execute();
        }

        [Fact()]
        public void Full()
        {
            var stopwatch = new Stopwatch();
            var connectionString = "Server=.;Database=ETLTest;Integrated Security=SSPI;";
            var connectionProvider = new Func<SqlConnection>(() => new SqlConnection(connectionString));
            var read = new DatabaseSource<Record>(connectionProvider, "SELECT * FROM TestTableSize");
            var convert = new ConversionOperation<Record, ResultingRecord>(input => input);
            var write = new SqlBulkCopyOperation<ResultingRecord>(connectionProvider, "ResultingRecord");
            var pipeline = CreateNewPipeline();
            pipeline.AddOperation(read);
            pipeline.AddOperation(convert);
            pipeline.AddOperation(write);
            stopwatch.Start();
            pipeline.Execute();
            stopwatch.Stop();
            Debug.WriteLine(string.Format("Processed in {0}", stopwatch.Elapsed));
        }

        public class Record
        {
            public string MyKeyField { get; set; }
            public DateTime MyDate1 { get; set; }
            public DateTime MyDate2 { get; set; }
            public DateTime MyDate3 { get; set; }
            public DateTime MyDate4 { get; set; }
            public DateTime MyDate5 { get; set; }

            public override string ToString()
            {
                return string.Format("Record " + MyKeyField);
            }


            public static implicit operator ResultingRecord(Record record)
            {
                return new ResultingRecord()
                {
                    Id = record.MyKeyField,
                    MinimumDate = new[]
                    {
                        record.MyDate1,
                        record.MyDate2,
                        record.MyDate3,
                        record.MyDate4,
                        record.MyDate5
                    }.Min()
                };
            }
        }

        public class ResultingRecord
        {
            public string Id { get; set; }
            public DateTime MinimumDate { get; set; }


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
