using System;
using System.Linq;
using Should;
using Xunit;

namespace LightningBug.Tests
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
            dt.Columns.Count.ShouldEqual(3);
            dt.Columns["Id"].DataType.ShouldEqual(typeof(Guid));
            dt.Columns["Name"].DataType.ShouldEqual(typeof(string));
            dt.Columns["EmployeeId"].DataType.ShouldEqual(typeof(int));
            dt.Rows.Count.ShouldEqual(0);
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
            dt.Columns.Count.ShouldEqual(3);
            dt.Columns["Id"].DataType.ShouldEqual(typeof(Guid));
            dt.Columns["Name"].DataType.ShouldEqual(typeof(string));
            dt.Columns["EmployeeId"].DataType.ShouldEqual(typeof(int));
            dt.Rows.Count.ShouldEqual(2);
            dt.Rows[0]["Id"].ShouldEqual(source[0].Id);
            dt.Rows[0]["Name"].ShouldEqual(source[0].Name);
            dt.Rows[0]["EmployeeId"].ShouldEqual(source[0].EmployeeId);
            dt.Rows[1]["Id"].ShouldEqual(source[1].Id);
            dt.Rows[1]["Name"].ShouldEqual(source[1].Name);
            dt.Rows[1]["EmployeeId"].ShouldEqual(source[1].EmployeeId);
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
            result.Length.ShouldEqual(2);
            result[0].Id.ShouldEqual(dt.Rows[0]["Id"]);
            result[0].Name.ShouldEqual(dt.Rows[0]["Name"]);
            result[0].EmployeeId.ShouldEqual(dt.Rows[0]["EmployeeId"]);
            result[1].Id.ShouldEqual(dt.Rows[1]["Id"]);
            result[1].Name.ShouldEqual(dt.Rows[1]["Name"]);
            result[1].EmployeeId.ShouldEqual(dt.Rows[1]["EmployeeId"]);
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
            result.Length.ShouldEqual(2);
            result[0].Id.ShouldEqual(dt.Rows[0]["Id"]);
            result[0].Name.ShouldEqual(dt.Rows[0]["Name"]);
            result[0].EmployeeId.ShouldEqual(dt.Rows[0]["EmployeeId"]);
            result[0].GetDependency().ShouldBeSameAs(dependency);
            result[1].Id.ShouldEqual(dt.Rows[1]["Id"]);
            result[1].Name.ShouldEqual(dt.Rows[1]["Name"]);
            result[1].EmployeeId.ShouldEqual(dt.Rows[1]["EmployeeId"]);
            result[1].GetDependency().ShouldBeSameAs(dependency);
        }

    }
}
