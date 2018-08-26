using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace LightningBug.Polly.Wrapper.SyncTests
{
    public class ReturnValueWithArguments
    {
        public const string HelloWorld = "Hello World!";

        public interface IService
        {
            string Echo(string message);
        }

        public class Service : IService
        {
            public string Echo(string message)
            {
                return message;
            }

        }

        [Fact]
        public void WithoutPolicy()
        {
            var impl = new Service();
            var provider = new NullPolicyProvider();
            var proxy = PollyWrapper<IService>.Decorate(impl, provider);
            var result = proxy.Echo(HelloWorld);
            result.ShouldBe(HelloWorld);
        }

        [Fact]
        public void WithPolicy()
        {
            var impl = new Service();
            var provider = new NoOpPolicyProvider();
            var proxy = PollyWrapper<IService>.Decorate(impl, provider);
            var result = proxy.Echo(HelloWorld);
            result.ShouldBe(HelloWorld);
        }

        [Fact]
        public void ExecutesPolicy()
        {
            var impl = new Service();
            var executed = false;
            var provider = new CallbackPolicyProvider((method, arguments) => executed = true);
            var proxy = PollyWrapper<IService>.Decorate(impl, provider);
            var result = proxy.Echo(HelloWorld);
            executed.ShouldBeTrue();
        }
    }
}