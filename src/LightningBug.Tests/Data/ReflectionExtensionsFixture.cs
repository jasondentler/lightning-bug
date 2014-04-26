using Should;
using Xunit;

namespace LightningBug.Data
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
            getter(x).ShouldEqual(2);
        }

        [Fact]
        public void CanWriteProperty()
        {
            var pi = typeof(TestClass).GetProperty("Value");
            var setter = pi.BuildSetDelegate<TestClass>();

            var x = new TestClass() {Value = 2};
            setter(x, 3);
            x.Value.ShouldEqual(3);
        }

    }
}
