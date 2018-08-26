using System.Collections.Generic;
using Shouldly;
using Xunit;

namespace LightningBug.Polly.Wrapper.SyncTests
{
    public class IndexedSetter
    {
        public const string HelloWorld = "Hello World!";

        public interface IService
        {
            string this[string id] { get; set; }
        }

        public class Service : IService
        {
            private readonly IDictionary<string, string> _values = new Dictionary<string, string>();

            public string this[string id]
            {
                get => _values[id];
                set => _values[id] = value;
            }
        }

        [Fact]
        public void WithoutPolicy()
        {
            var impl = new Service();
            var provider = new NullPolicyProvider();
            var proxy = PollyWrapper<IService>.Decorate(impl, provider);
            proxy[HelloWorld] = HelloWorld;
            var result = proxy[HelloWorld];
            result.ShouldBe(HelloWorld);
        }

        [Fact]
        public void WithPolicy()
        {
            var impl = new Service();
            var provider = new NoOpPolicyProvider();
            var proxy = PollyWrapper<IService>.Decorate(impl, provider);
            proxy[HelloWorld] = HelloWorld;
            var result = proxy[HelloWorld];
            result.ShouldBe(HelloWorld);
        }

        [Fact]
        public void ExecutesPolicy()
        {
            var impl = new Service();
            var executed = false;
            var provider = new CallbackPolicyProvider((method, arguments) => executed = true);
            var proxy = PollyWrapper<IService>.Decorate(impl, provider);
            proxy[HelloWorld] = HelloWorld;
            var result = proxy[HelloWorld];
            executed.ShouldBeTrue();
        }
    }
}