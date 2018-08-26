using System;
using System.IO;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace LightningBug.Polly.Wrapper.AsyncTests
{
    public class ReturnValueWithoutArguments
    {
        public const string HelloWorld = "Hello World!";

        public interface IService
        {
            Task<string> EchoHelloWorldAsync();
        }

        public class Service : IService
        {
            public Task<string> EchoHelloWorldAsync()
            {
                return Task.Run(() => HelloWorld);
            }

        }

        [Fact]
        public async Task X()
        {
            var impl = new Service();
            var provider = new TestPolicyProvider();
            var proxy = PollyWrapper<IService>.Decorate(impl, provider);
            var result = await proxy.EchoHelloWorldAsync();
            result.ShouldBe(HelloWorld);
        }
    }
}