using System;
using System.IO;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace LightningBug.Polly.Wrapper.AsyncTests
{
    public class ReturnValueWithArguments
    {
        public const string HelloWorld = "Hello World!";

        public interface IService
        {
            Task<string> EchoAsync(string message);
        }

        public class Service : IService
        {
            public Task<string> EchoAsync(string message)
            {
                return Task.Run(() => message);
            }

        }

        [Fact]
        public async Task WithoutPolicy()
        {
            var impl = new Service();
            var provider = new NullPolicyProvider();
            var proxy = PollyWrapper<IService>.Decorate(impl, provider);
            var result = await proxy.EchoAsync(HelloWorld);
            result.ShouldBe(HelloWorld);
        }

        [Fact]
        public async Task WithPolicy()
        {
            var impl = new Service();
            var provider = new NoOpPolicyProvider();
            var proxy = PollyWrapper<IService>.Decorate(impl, provider);
            var result = await proxy.EchoAsync(HelloWorld);
            result.ShouldBe(HelloWorld);
        }

        [Fact]
        public async Task ExecutesPolicy()
        {
            var impl = new Service();
            var executed = false;
            var provider = new CallbackPolicyProvider((method, arguments) => executed = true);
            var proxy = PollyWrapper<IService>.Decorate(impl, provider);
            var result = await proxy.EchoAsync(HelloWorld);
            executed.ShouldBeTrue();
        }
    }
}