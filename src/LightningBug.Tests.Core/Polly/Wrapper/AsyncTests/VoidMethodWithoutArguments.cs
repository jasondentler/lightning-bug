using System;
using System.IO;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace LightningBug.Polly.Wrapper.AsyncTests
{
    public class VoidMethodWithoutArguments
    {
        public const string HelloWorld = "Hello World!";

        public interface IService
        {
            Task SayHelloWorldAsync();
        }

        public class Service : IService
        {
            private readonly TextWriter _output;

            public Service(TextWriter output)
            {
                _output = output;
            }

            public Task SayHelloWorldAsync()
            {
                return Task.Run(() => _output.Write(HelloWorld));
            }
        }

        [Fact]
        public async Task X()
        {
            var output = new StringWriter();
            var impl = new Service(output);
            var provider = new TestPolicyProvider();
            var proxy = PollyWrapper<IService>.Decorate(impl, provider);
            await proxy.SayHelloWorldAsync();
            output.ToString().ShouldBe(HelloWorld);
        }
    }
}