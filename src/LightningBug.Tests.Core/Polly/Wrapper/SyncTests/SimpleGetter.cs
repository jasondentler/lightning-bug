using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace LightningBug.Polly.Wrapper.SyncTests
{
    public class SimpleGetter
    {
        public const string HelloWorld = "Hello World!";

        public interface IService
        {
            string Greeting { get; }
        }

        public class Service : IService
        {
            public string Greeting => HelloWorld;
        }

        [Fact]
        public void X()
        {
            var impl = new Service();
            var provider = new TestPolicyProvider();
            var proxy = PollyWrapper<IService>.Decorate(impl, provider);
            var result = proxy.Greeting;
            result.ShouldBe(HelloWorld);
        }
    }
}