using System;
using LightningBug.Polly.Providers;
using Shouldly;
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
        public void WithoutPolicy()
        {
            var impl = new Throw();
            var provider = new NullPolicyProvider();
            var proxy = PollyWrapper<IThrow>.Decorate(impl, provider, new ContextProvider());
            Assert.Throws<ApplicationException>(() => proxy.ThrowException());
        }

        [Fact]
        public void WithPolicy()
        {
            var impl = new Throw();
            var provider = new NoOpPolicyProvider();
            var proxy = PollyWrapper<IThrow>.Decorate(impl, provider, new ContextProvider());
            Assert.Throws<ApplicationException>(() => proxy.ThrowException());
        }

        [Fact]
        public void ExecutesPolicy()
        {
            var impl = new Throw();
            var executed = false;
            var provider = new CallbackPolicyProvider((method, arguments) => executed = true);
            var proxy = PollyWrapper<IThrow>.Decorate(impl, provider, new ContextProvider());
            try
            {
                proxy.ThrowException();
            }
            catch
            {
            }
            executed.ShouldBeTrue();
        }

    }
}