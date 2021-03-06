﻿using System.Linq;
using Shouldly;
using Xunit;

namespace LightningBug.Reflection
{
    public class GetterDelegateCacheFixture
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
            GetterDelegateCache<TestClass>.ReadablePropertyNames.Count().ShouldBe(2);
            GetterDelegateCache<TestClass>.ReadablePropertyNames.ShouldContain("MyInt");
            GetterDelegateCache<TestClass>.ReadablePropertyNames.ShouldContain("MyReadOnlyInt");
        }

        [Fact]
        public void HasCorrectPropertyCount()
        {
            GetterDelegateCache<TestClass>.ReadablePropertyCount.ShouldBe(2);
        }

        [Fact]
        public void TypesHasCorrectPropertyNames()
        {
            GetterDelegateCache<TestClass>.Types.Count.ShouldBe(2);
            GetterDelegateCache<TestClass>.Types.Keys.ShouldContain("MyInt");
            GetterDelegateCache<TestClass>.Types.Keys.ShouldContain("MyReadOnlyInt");
        }

        [Fact]
        public void OrdinalsAreSequential()
        {
            var values = GetterDelegateCache<TestClass>.OrdinalLookup.Values;
            for (var i = 0; i < values.Count; i++)
                values.ShouldContain(i);
        }

        [Fact]
        public void OrdinalsAreRepeatable()
        {
            GetterDelegateCache<TestClass>.OrdinalLookup
                .ToList()
                .ForEach(kv => GetterDelegateCache<TestClass>.OrdinalLookup[kv.Key].ShouldBe(kv.Value));
        }

        [Fact]
        public void OrdinalsMatchPropertyName()
        {
            GetterDelegateCache<TestClass>.OrdinalLookup.Keys
                .ToList()
                .ForEach(s => GetterDelegateCache<TestClass>.ReadablePropertyNames.ShouldContain(s));
        }

        [Fact]
        public void CanReadPublicProperty()
        {
            var x = new TestClass() {MyInt = 1};
            GetterDelegateCache<TestClass>.Read("MyInt", x).ShouldBe(1);
        }

        [Fact]
        public void CanReadReadOnlyProperty()
        {
            var x = new TestClass() {MyInt = 2};
            GetterDelegateCache<TestClass>.Read("MyReadOnlyInt", x).ShouldBe(2);
        }

        [Fact]
        public void CanReadByOrdinal()
        {
            var x = new TestClass() { MyInt = 3 };
            var ordinal = GetterDelegateCache<TestClass>.OrdinalLookup["MyInt"];
            GetterDelegateCache<TestClass>.Read(ordinal, x).ShouldBe(3);
        }

        [Fact]
        public void CanReadReadOnlyPropertyByOrdinal()
        {
            var x = new TestClass() { MyInt = 4 };
            var ordinal = GetterDelegateCache<TestClass>.OrdinalLookup["MyReadOnlyInt"];
            GetterDelegateCache<TestClass>.Read(ordinal, x).ShouldBe(4);
        }

    }

}
