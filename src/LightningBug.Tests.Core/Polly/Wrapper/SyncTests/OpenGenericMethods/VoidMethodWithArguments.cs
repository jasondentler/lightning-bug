using System.IO;
using LightningBug.Polly.Providers;
using Shouldly;
using Xunit;

namespace LightningBug.Polly.Wrapper.SyncTests.OpenGenericMethods
{
    public class VoidMethodWithArguments
    {
        public const string HelloWorld = "Hello World!";

        public interface IService
        {
            void Say<T>(T message);
        }

        public class Service : IService
        {
            private readonly TextWriter _output;

            public Service(TextWriter output)
            {
                _output = output;
            }

            public void Say<T>(T message)
            {
                _output.Write(message);
            }
        }

        [Fact]
        public void WithoutPolicy()
        {
            var output = new StringWriter();
            var impl = new Service(output);
            var provider = new NullPolicyProvider();
            var proxy = PollyWrapper<IService>.Decorate(impl, provider, new ContextProvider());
            proxy.Say(HelloWorld);
            output.ToString().ShouldBe(HelloWorld);
        }

        [Fact]
        public void WithPolicy()
        {
            var output = new StringWriter();
            var impl = new Service(output);
            var provider = new NoOpPolicyProvider();
            var proxy = PollyWrapper<IService>.Decorate(impl, provider, new ContextProvider());
            proxy.Say(HelloWorld);
            output.ToString().ShouldBe(HelloWorld);
        }

        [Fact]
        public void ExecutesPolicy()
        {
            var output = new StringWriter();
            var impl = new Service(output);
            var executed = false;
            var provider = new CallbackPolicyProvider((method, arguments) => executed = true);
            var proxy = PollyWrapper<IService>.Decorate(impl, provider, new ContextProvider());
            proxy.Say(HelloWorld);
            executed.ShouldBeTrue();
        }
    }
}