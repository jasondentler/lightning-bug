using System.Linq;
using Should;
using Xunit;

namespace LightningBug.Reflection
{
    public class SetterDelegateCacheFixture
    {
        public class TestClass
        {
            public int MyInt { get; set; }
            private string MyPrivateString { get; set; }
            protected string MyProtectedString { get; set; }

            public int MyReadOnlyInt
            {
                get { return MyInt; }
            }

            public int MyWriteOnlyInt
            {
                set { MyInt = value; }
            }

            public int Method()
            {
                return 0;
            }
        }

        [Fact]
        public void HasCorrectPropertyNames()
        {
            SetterDelegateCache<TestClass>.WritablePropertyNames.Count().ShouldEqual(2);
            SetterDelegateCache<TestClass>.WritablePropertyNames.ShouldContain("MyInt");
            SetterDelegateCache<TestClass>.WritablePropertyNames.ShouldContain("MyWriteOnlyInt");
        }

        [Fact]
        public void HasCorrectPropertyCount()
        {
            SetterDelegateCache<TestClass>.WritablePropertyCount.ShouldEqual(2);
        }

        [Fact]
        public void TypesHasCorrectPropertyNames()
        {
            SetterDelegateCache<TestClass>.Types.Count.ShouldEqual(2);
            SetterDelegateCache<TestClass>.Types.Keys.ShouldContain("MyInt");
            SetterDelegateCache<TestClass>.Types.Keys.ShouldContain("MyWriteOnlyInt");
        }

        [Fact]
        public void OrdinalsAreSequential()
        {
            var values = SetterDelegateCache<TestClass>.OrdinalLookup.Values;
            for (var i = 0; i < values.Count; i++)
                values.ShouldContain(i);
        }

        [Fact]
        public void OrdinalsAreRepeatable()
        {
            SetterDelegateCache<TestClass>.OrdinalLookup
                .ToList()
                .ForEach(kv => SetterDelegateCache<TestClass>.OrdinalLookup[kv.Key].ShouldEqual(kv.Value));
        }

        [Fact]
        public void OrdinalsMatchPropertyName()
        {
            SetterDelegateCache<TestClass>.OrdinalLookup.Keys
                .ToList()
                .ForEach(s => SetterDelegateCache<TestClass>.WritablePropertyNames.ShouldContain(s));
        }

        [Fact]
        public void CanWritePublicProperty()
        {
            var x = new TestClass() {MyInt = 1};
            SetterDelegateCache<TestClass>.Write("MyInt", x, 2);
            x.MyInt.ShouldEqual(2);
        }

        [Fact]
        public void CanWriteWriteOnlyProperty()
        {
            var x = new TestClass() {MyInt = 2};
            SetterDelegateCache<TestClass>.Write("MyWriteOnlyInt", x, 3);
            x.MyInt.ShouldEqual(3);
        }

        [Fact]
        public void CanWriteByOrdinal()
        {
            var x = new TestClass() {MyInt = 3};
            var ordinal = SetterDelegateCache<TestClass>.OrdinalLookup["MyInt"];
            SetterDelegateCache<TestClass>.Write(ordinal, x, 4);
            x.MyInt.ShouldEqual(4);
        }

        [Fact]
        public void CanWriteWriteOnlyPropertyByOrdinal()
        {
            var x = new TestClass() {MyInt = 4};
            var ordinal = SetterDelegateCache<TestClass>.OrdinalLookup["MyWriteOnlyInt"];
            SetterDelegateCache<TestClass>.Write(ordinal, x, 5);
            x.MyInt.ShouldEqual(5);
        }

    }

}
