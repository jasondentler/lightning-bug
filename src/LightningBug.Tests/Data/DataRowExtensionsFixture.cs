using System;
using Shouldly;
using Xunit;

namespace LightningBug.Data
{
    public class DataRowExtensionsFixture
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
        public void CanConvertRowToInstance()
        {
            var dt = DataTableExtensions.GetSchemaTable<TestClass>();
            var row = dt.NewRow();
            var id = Guid.NewGuid();
            var name = "Lightning Bug";
            var employeeId = 123456;
            row["Id"] = id;
            row["Name"] = name;
            row["EmployeeId"] = employeeId;

            var instance = row.ToInstance<TestClass>();
            instance.ShouldNotBeNull();
            instance.Id.ShouldBe(id);
            instance.Name.ShouldBe(name);
            instance.EmployeeId.ShouldBe(employeeId);
        }

        [Fact]
        public void CanConvertRowToInstanceWithConstructorDependency()
        {
            var dt = DataTableExtensions.GetSchemaTable<TestClassWithDependency>();
            var row = dt.NewRow();
            var id = Guid.NewGuid();
            var name = "Lightning Bug";
            var employeeId = 123456;
            row["Id"] = id;
            row["Name"] = name;
            row["EmployeeId"] = employeeId;

            var dependency = new UriBuilder();
            var instance = row.ToInstance(() => new TestClassWithDependency(dependency));
            instance.ShouldNotBeNull();
            instance.Id.ShouldBe(id);
            instance.Name.ShouldBe(name);
            instance.EmployeeId.ShouldBe(employeeId);
            instance.GetDependency().ShouldBeSameAs(dependency);
        }

    }
}
