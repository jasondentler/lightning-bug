using Shouldly;
using Xunit;

namespace LightningBug.Polly.Wrapper.SyncTests
{
    public class SimpleSetter
    {
        public const string HelloWorld = "Hello World!";

        public interface IService
        {
            string Greeting { get; set; }
        }

        public class Service : IService
        {
            public string Greeting { get; set; }
        }

        [Fact]
        public void WithoutPolicy()
        {
            var impl = new Service();
            var provider = new NullPolicyProvider();
            var proxy = PollyWrapper<IService>.Decorate(impl, provider);
            proxy.Greeting = HelloWorld;
            var result = proxy.Greeting;
            result.ShouldBe(HelloWorld);
        }
        [Fact]
        public void WithPolicy()
        {
            var impl = new Service();
            var executed = 0;
            var provider = new CallbackPolicyProvider((method, arguments) => executed++);
            var proxy = PollyWrapper<IService>.Decorate(impl, provider);
            proxy.Greeting = HelloWorld;
            var result = proxy.Greeting;
            result.ShouldBe(HelloWorld);
        }

        [Fact]
        public void ExecutesPolicy()
        {
            var impl = new Service();
            var executed = false;
            var provider = new CallbackPolicyProvider((method, arguments) => executed = true);
            var proxy = PollyWrapper<IService>.Decorate(impl, provider);
            proxy.Greeting = HelloWorld;
            executed.ShouldBeTrue();
        }
    }
}