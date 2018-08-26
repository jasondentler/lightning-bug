using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace LightningBug.Polly.Wrapper.SyncTests
{
    public class ReturnValueWithoutArguments
    {
        public const string HelloWorld = "Hello World!";

        public interface IService
        {
            string EchoHelloWorld();
        }

        public class Service : IService
        {
            public string EchoHelloWorld()
            {
                return HelloWorld;
            }

        }

        [Fact]
        public void X()
        {
            var impl = new Service();
            var provider = new TestPolicyProvider();
            var proxy = PollyWrapper<IService>.Decorate(impl, provider);
            var result = proxy.EchoHelloWorld();
            result.ShouldBe(HelloWorld);
        }
    }
}