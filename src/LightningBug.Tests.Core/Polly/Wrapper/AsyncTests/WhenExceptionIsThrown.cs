using System;
using System.Threading.Tasks;
using Xunit;

namespace LightningBug.Polly.Wrapper.AsyncTests
{
    public class WhenExceptionIsThrown
    {

        public interface IThrow
        {
            Task ThrowException();
        }

        public class Throw : IThrow
        {
            public Task ThrowException()
            {
                throw new ApplicationException();
            }
        }

        [Fact]
        public async Task X()
        {
            var impl = new Throw();
            var provider = new TestPolicyProvider();
            var proxy = PollyWrapper<IThrow>.Decorate(impl, provider);
            await Assert.ThrowsAsync<ApplicationException>(async () => await proxy.ThrowException());
        }

    }
}