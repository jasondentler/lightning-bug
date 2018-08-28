using LightningBug.Polly.Providers;
using Shouldly;
using Xunit;

namespace LightningBug.Polly.Wrapper.SyncTests
{
    public class IndexedGetter
    {
        public const string HelloWorld = "Hello World!";

        public interface IService
        {
            string this[string id] { get; }
        }

        public class Service : IService
        {
            public string this[string id] => id;
        }

        [Fact]
        public void WithoutPolicy()
        {
            var impl = new Service();
            var policyProvider = new NullPolicyProvider();
            var contextProvider = new ContextProvider();
            var proxy = PollyWrapper<IService>.Decorate(impl, policyProvider, contextProvider);
            var result = proxy[HelloWorld];
            result.ShouldBe(HelloWorld);
        }
        [Fact]
        public void WithPolicy()
        {
            var impl = new Service();
            var policyProvider = new NoOpPolicyProvider();
            var contextProvider = new ContextProvider();
            var proxy = PollyWrapper<IService>.Decorate(impl, policyProvider, contextProvider);
            var result = proxy[HelloWorld];
            result.ShouldBe(HelloWorld);
        }

        [Fact]
        public void ExecutesPolicy()
        {
            var impl = new Service();
            var executed = false;
            var policyProvider = new CallbackPolicyProvider((method, arguments) => executed = true);
            var contextProvider = new ContextProvider();
            var proxy = PollyWrapper<IService>.Decorate(impl, policyProvider, contextProvider);
            var result = proxy[HelloWorld];
            result.ShouldBe(HelloWorld);
            executed.ShouldBeTrue();
        }
    }
}