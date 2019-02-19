using System;
using System.Linq;
using Shouldly;
using Xunit;

namespace LightningBug.Data
{
    public class DataTableExtensionsFixture
    {

        public class TestClass
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public int EmployeeId { get; set; }
        }

        public class TestClassWithDependency
        {
            private readonly UriBuilder _dependency;

            public TestClassWithDependency(UriBuilder dependency)
            {
                _dependency = dependency;
            }

            public Guid Id { get; set; }
            public string Name { get; set; }
            public int EmployeeId { get; set; }
            public UriBuilder GetDependency()
            {
                return _dependency;
            }
        }


        [Fact]
        public void CanGetSchemaDataTable()
        {
            var dt = DataTableExtensions.GetSchemaTable<TestClass>();
            dt.Columns.Count.ShouldBe(3);
            dt.Columns["Id"].DataType.ShouldBe(typeof(Guid));
            dt.Columns["Name"].DataType.ShouldBe(typeof(string));
            dt.Columns["EmployeeId"].DataType.ShouldBe(typeof(int));
            dt.Rows.Count.ShouldBe(0);
        }

        [Fact]
        public void CanConvertEnumerableToDataTable()
        {
            var source = new[]
            {
                new TestClass() {Id = Guid.NewGuid(), Name = "Lightning Bug", EmployeeId = 123456},
                new TestClass() {Id = Guid.NewGuid(), Name = "Firefly", EmployeeId = 234567}
            };

            var dt = source.AsDataTable();
            dt.Columns.Count.ShouldBe(3);
            dt.Columns["Id"].DataType.ShouldBe(typeof(Guid));
            dt.Columns["Name"].DataType.ShouldBe(typeof(string));
            dt.Columns["EmployeeId"].DataType.ShouldBe(typeof(int));
            dt.Rows.Count.ShouldBe(2);
            dt.Rows[0]["Id"].ShouldBe(source[0].Id);
            dt.Rows[0]["Name"].ShouldBe(source[0].Name);
            dt.Rows[0]["EmployeeId"].ShouldBe(source[0].EmployeeId);
            dt.Rows[1]["Id"].ShouldBe(source[1].Id);
            dt.Rows[1]["Name"].ShouldBe(source[1].Name);
            dt.Rows[1]["EmployeeId"].ShouldBe(source[1].EmployeeId);
        }

        [Fact]
        public void CanConvertDataTableToEnumerable()
        {
            var dt = DataTableExtensions.GetSchemaTable<TestClass>();
            dt.Rows.Add(dt.NewRow());
            dt.Rows[0]["Id"] = Guid.NewGuid();
            dt.Rows[0]["Name"] = "Lightning Bug";
            dt.Rows[0]["EmployeeId"] = 123456;
            dt.Rows.Add(dt.NewRow());
            dt.Rows[1]["Id"] = Guid.NewGuid();
            dt.Rows[1]["Name"] = "Firefly";
            dt.Rows[1]["EmployeeId"] = 234567;

            var result = dt.Convert<TestClass>().ToArray();
            result.Length.ShouldBe(2);
            result[0].Id.ShouldBe(dt.Rows[0]["Id"]);
            result[0].Name.ShouldBe(dt.Rows[0]["Name"]);
            result[0].EmployeeId.ShouldBe(dt.Rows[0]["EmployeeId"]);
            result[1].Id.ShouldBe(dt.Rows[1]["Id"]);
            result[1].Name.ShouldBe(dt.Rows[1]["Name"]);
            result[1].EmployeeId.ShouldBe(dt.Rows[1]["EmployeeId"]);
        }

        [Fact]
        public void CanConvertDataTableToEnumerableWithDependency()
        {
            var dt = DataTableExtensions.GetSchemaTable<TestClass>();
            dt.Rows.Add(dt.NewRow());
            dt.Rows[0]["Id"] = Guid.NewGuid();
            dt.Rows[0]["Name"] = "Lightning Bug";
            dt.Rows[0]["EmployeeId"] = 123456;
            dt.Rows.Add(dt.NewRow());
            dt.Rows[1]["Id"] = Guid.NewGuid();
            dt.Rows[1]["Name"] = "Firefly";
            dt.Rows[1]["EmployeeId"] = 234567;

            var dependency = new UriBuilder();
            var result = dt.Convert(() => new TestClassWithDependency(dependency)).ToArray();
            result.Length.ShouldBe(2);
            result[0].Id.ShouldBe(dt.Rows[0]["Id"]);
            result[0].Name.ShouldBe(dt.Rows[0]["Name"]);
            result[0].EmployeeId.ShouldBe(dt.Rows[0]["EmployeeId"]);
            result[0].GetDependency().ShouldBeSameAs(dependency);
            result[1].Id.ShouldBe(dt.Rows[1]["Id"]);
            result[1].Name.ShouldBe(dt.Rows[1]["Name"]);
            result[1].EmployeeId.ShouldBe(dt.Rows[1]["EmployeeId"]);
            result[1].GetDependency().ShouldBeSameAs(dependency);
        }

    }
}
