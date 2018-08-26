using Shouldly;
using Xunit;

namespace LightningBug.Polly.Wrapper.SyncTests
{
    public class IndexedGetter
    {
        public const string HelloWorld = "Hello World!";

        public interface IService
        {
            string this[string id] { get; }
        }

        public class Service : IService
        {
            public string this[string id] => id;
        }

        [Fact]
        public void X()
        {
            var impl = new Service();
            var provider = new TestPolicyProvider();
            var proxy = PollyWrapper<IService>.Decorate(impl, provider);
            var result = proxy[HelloWorld];
            result.ShouldBe(HelloWorld);
        }
    }
}