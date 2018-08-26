using Shouldly;
using Xunit;

namespace LightningBug.Polly.Wrapper.SyncTests
{
    public class SimpleSetter
    {
        public const string HelloWorld = "Hello World!";

        public interface IService
        {
            string Greeting { get; set; }
        }

        public class Service : IService
        {
            public string Greeting { get; set; }
        }

        [Fact]
        public void X()
        {
            var impl = new Service();
            var provider = new TestPolicyProvider();
            var proxy = PollyWrapper<IService>.Decorate(impl, provider);
            proxy.Greeting = HelloWorld;
            var result = proxy.Greeting;
            result.ShouldBe(HelloWorld);
        }
    }
}