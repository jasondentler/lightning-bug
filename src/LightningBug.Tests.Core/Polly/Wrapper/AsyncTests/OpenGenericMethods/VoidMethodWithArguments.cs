using System.IO;
using System.Threading.Tasks;
using LightningBug.Polly.Providers;
using Shouldly;
using Xunit;

namespace LightningBug.Polly.Wrapper.AsyncTests.OpenGenericMethods
{
    public class VoidMethodWithArguments
    {
        public const string HelloWorld = "Hello World!";

        public interface IService
        {
            Task SayAsync<T>(T message);
        }

        public class Service : IService
        {
            private readonly TextWriter _output;

            public Service(TextWriter output)
            {
                _output = output;
            }

            public Task SayAsync<T>(T message)
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
            var proxy = PollyWrapper<IService>.Decorate(impl, provider, new ContextProvider());
            await proxy.SayAsync(HelloWorld);
            output.ToString().ShouldBe(HelloWorld);
        }

        [Fact]
        public async Task WithPolicy()
        {
            var output = new StringWriter();
            var impl = new Service(output);
            var provider = new NoOpPolicyProvider();
            var proxy = PollyWrapper<IService>.Decorate(impl, provider, new ContextProvider());
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
            var proxy = PollyWrapper<IService>.Decorate(impl, provider, new ContextProvider());
            await proxy.SayAsync(HelloWorld);
            executed.ShouldBeTrue();
        }
    }
}