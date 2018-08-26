using System.IO;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace LightningBug.Polly.Wrapper.SyncTests
{
    public class VoidMethodWithArguments
    {
        public const string HelloWorld = "Hello World!";

        public interface IService
        {
            void Say(string message);
        }

        public class Service : IService
        {
            private readonly TextWriter _output;

            public Service(TextWriter output)
            {
                _output = output;
            }

            public void Say(string message)
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
            var proxy = PollyWrapper<IService>.Decorate(impl, provider);
            proxy.Say(HelloWorld);
            output.ToString().ShouldBe(HelloWorld);
        }

        [Fact]
        public void WithPolicy()
        {
            var output = new StringWriter();
            var impl = new Service(output);
            var provider = new NoOpPolicyProvider();
            var proxy = PollyWrapper<IService>.Decorate(impl, provider);
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
            var proxy = PollyWrapper<IService>.Decorate(impl, provider);
            proxy.Say(HelloWorld);
            executed.ShouldBeTrue();
        }
    }
}