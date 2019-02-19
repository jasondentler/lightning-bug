using System.Threading.Tasks;
using LightningBug.Polly.Providers;
using Shouldly;
using Xunit;

namespace LightningBug.Polly.Wrapper.AsyncTests.OpenGenericMethods
{
    public class WithinClosedGenericClass
    {
        public const string HelloWorld = "Hello World!";

        public interface IService<T1>
        {
            Task<T2> EchoAsync<T2>(T2 message);
        }

        public class Service : IService<string>
        {
            public Task<T> EchoAsync<T>(T message)
            {
                return Task.Run(() => message);
            }

        }

        [Fact]
        public async Task WithoutPolicy()
        {
            var impl = new Service();
            var provider = new NullPolicyProvider();
            var proxy = PollyWrapper<IService<string>>.Decorate(impl, provider, new ContextProvider());
            var result = await proxy.EchoAsync(HelloWorld);
            result.ShouldBe(HelloWorld);
        }

        [Fact]
        public async Task WithPolicy()
        {
            var impl = new Service();
            var provider = new NoOpPolicyProvider();
            var proxy = PollyWrapper<IService<string>>.Decorate(impl, provider, new ContextProvider());
            var result = await proxy.EchoAsync(HelloWorld);
            result.ShouldBe(HelloWorld);
        }

        [Fact]
        public async Task ExecutesPolicy()
        {
            var impl = new Service();
            var executed = false;
            var provider = new CallbackPolicyProvider((method, arguments) => executed = true);
            var proxy = PollyWrapper<IService<string>>.Decorate(impl, provider, new ContextProvider());
            var result = await proxy.EchoAsync(HelloWorld);
            executed.ShouldBeTrue();
        }
    }
}