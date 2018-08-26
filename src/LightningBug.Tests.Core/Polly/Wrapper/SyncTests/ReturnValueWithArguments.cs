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
        public void X()
        {
            var impl = new Service();
            var provider = new TestPolicyProvider();
            var proxy = PollyWrapper<IService>.Decorate(impl, provider);
            var result = proxy.Echo(HelloWorld);
            result.ShouldBe(HelloWorld);
        }
    }
}