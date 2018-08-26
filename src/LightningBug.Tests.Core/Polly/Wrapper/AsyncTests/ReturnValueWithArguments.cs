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
        public async Task X()
        {
            var impl = new Service();
            var provider = new TestPolicyProvider();
            var proxy = PollyWrapper<IService>.Decorate(impl, provider);
            var result = await proxy.EchoAsync(HelloWorld);
            result.ShouldBe(HelloWorld);
        }
    }
}