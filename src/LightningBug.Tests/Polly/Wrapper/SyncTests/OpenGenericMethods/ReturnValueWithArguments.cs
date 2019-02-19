using LightningBug.Polly.Providers;
using Shouldly;
using Xunit;

namespace LightningBug.Polly.Wrapper.SyncTests.OpenGenericMethods
{
    public class ReturnValueWithArguments
    {
        public const string HelloWorld = "Hello World!";

        public interface IService
        {
            T Echo<T>(T message);
        }

        public class Service : IService
        {
            public T Echo<T>(T message)
            {
                return message;
            }
        }

        [Fact]
        public void WithoutPolicy()
        {
            var impl = new Service();
            var provider = new NullPolicyProvider();
            var proxy = PollyWrapper<IService>.Decorate(impl, provider, new ContextProvider());
            var result = proxy.Echo(HelloWorld);
            result.ShouldBe(HelloWorld);
        }

        [Fact]
        public void WithPolicy()
        {
            var impl = new Service();
            var provider = new NoOpPolicyProvider();
            var proxy = PollyWrapper<IService>.Decorate(impl, provider, new ContextProvider());
            var result = proxy.Echo(HelloWorld);
            result.ShouldBe(HelloWorld);
        }

        [Fact]
        public void ExecutesPolicy()
        {
            var impl = new Service();
            var executed = false;
            var provider = new CallbackPolicyProvider((method, arguments) => executed = true);
            var proxy = PollyWrapper<IService>.Decorate(impl, provider, new ContextProvider());
            var result = proxy.Echo(HelloWorld);
            executed.ShouldBeTrue();
        }
    }
}