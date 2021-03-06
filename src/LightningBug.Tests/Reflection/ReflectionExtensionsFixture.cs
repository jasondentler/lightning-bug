﻿using LightningBug.Data;
using Shouldly;
using Xunit;

namespace LightningBug.Reflection
{
    public class ReflectionExtensionsFixture
    {

        public class TestClass { public int Value { get; set; } }

        [Fact]
        public void CanReadProperty()
        {
            var pi = typeof (TestClass).GetProperty("Value");
            var getter = pi.BuildGetDelegate<TestClass>();

            var x = new TestClass() {Value = 2};
            getter(x).ShouldBe(2);
        }

        [Fact]
        public void CanWriteProperty()
        {
            var pi = typeof(TestClass).GetProperty("Value");
            var setter = pi.BuildSetDelegate<TestClass>();

            var x = new TestClass() {Value = 2};
            setter(x, 3);
            x.Value.ShouldBe(3);
        }

    }
}
