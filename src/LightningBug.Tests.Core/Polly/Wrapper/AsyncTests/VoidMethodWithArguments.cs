using System;
using System.IO;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace LightningBug.Polly.Wrapper.AsyncTests
{
    public class VoidMethodWithArguments
    {
        public const string HelloWorld = "Hello World!";

        public interface IService
        {
            Task SayAsync(string message);
        }

        public class Service : IService
        {
            private readonly TextWriter _output;

            public Service(TextWriter output)
            {
                _output = output;
            }

            public Task SayAsync(string message)
            {
                return Task.Run(() => _output.Write(message));
            }
        }

        [Fact]
        public async Task X()
        {
            var output = new StringWriter();
            var impl = new Service(output);
            var provider = new TestPolicyProvider();
            var proxy = PollyWrapper<IService>.Decorate(impl, provider);
            await proxy.SayAsync(HelloWorld);
            output.ToString().ShouldBe(HelloWorld);
        }
    }
}