using Shouldly;
using Xunit;

namespace LightningBug.Polly.Wrapper.SyncTests
{
    public class SimpleGetter
    {
        public const string HelloWorld = "Hello World!";

        public interface IService
        {
            string Greeting { get; }
        }

        public class Service : IService
        {
            public string Greeting => HelloWorld;
        }

        [Fact]
        public void WithoutPolicy()
        {
            var impl = new Service();
            var provider = new NullPolicyProvider();
            var proxy = PollyWrapper<IService>.Decorate(impl, provider);
            var result = proxy.Greeting;
            result.ShouldBe(HelloWorld);
        }

        [Fact]
        public void WithPolicy()
        {
            var impl = new Service();
            var provider = new NoOpPolicyProvider();
            var proxy = PollyWrapper<IService>.Decorate(impl, provider);
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
            var result = proxy.Greeting;
            executed.ShouldBeTrue();
        }
    }
}