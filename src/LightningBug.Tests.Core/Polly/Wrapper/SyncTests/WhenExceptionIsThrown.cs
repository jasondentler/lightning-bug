using System;
using System.Threading.Tasks;
using Xunit;

namespace LightningBug.Polly.Wrapper.SyncTests
{
    public class WhenExceptionIsThrown
    {

        public interface IThrow
        {
            void ThrowException();
        }

        public class Throw : IThrow
        {
            public void ThrowException()
            {
                throw new ApplicationException();
            }
        }

        [Fact]
        public void X()
        {
            var impl = new Throw();
            var provider = new TestPolicyProvider();
            var proxy = PollyWrapper<IThrow>.Decorate(impl, provider);
            Assert.Throws<ApplicationException>(() => proxy.ThrowException());
        }

    }
}