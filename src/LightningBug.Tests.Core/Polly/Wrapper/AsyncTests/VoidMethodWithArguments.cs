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
        public async Task WithoutPolicy()
        {
            var output = new StringWriter();
            var impl = new Service(output);
            var provider = new NullPolicyProvider();
            var proxy = PollyWrapper<IService>.Decorate(impl, provider);
            await proxy.SayAsync(HelloWorld);
            output.ToString().ShouldBe(HelloWorld);
        }

        [Fact]
        public async Task WithPolicy()
        {
            var output = new StringWriter();
            var impl = new Service(output);
            var provider = new NoOpPolicyProvider();
            var proxy = PollyWrapper<IService>.Decorate(impl, provider);
            await proxy.SayAsync(HelloWorld);
            output.ToString().ShouldBe(HelloWorld);
        }

        [Fact]
        public async Task ExecutesPolicy()
        {
            var output = new StringWriter();
            var impl = new Service(output);
            var executed = false;
            var provider = new CallbackPolicyProvider((method, arguments) => executed = true);
            var proxy = PollyWrapper<IService>.Decorate(impl, provider);
            await proxy.SayAsync(HelloWorld);
            executed.ShouldBeTrue();
        }
    }
}