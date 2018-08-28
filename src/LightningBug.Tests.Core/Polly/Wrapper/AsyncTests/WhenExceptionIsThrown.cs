using System;
using System.Reflection;
using System.Threading.Tasks;
using LightningBug.Polly.Providers;
using Shouldly;
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
        public async Task WithoutPolicy()
        {
            var impl = new Throw();
            var provider = new NullPolicyProvider();
            var proxy = PollyWrapper<IThrow>.Decorate(impl, provider, new ContextProvider());
            await Assert.ThrowsAsync<ApplicationException>(async () => await proxy.ThrowException());
        }

        [Fact]
        public async Task WithPolicy()
        {
            var impl = new Throw();
            var provider = new NoOpPolicyProvider();
            var proxy = PollyWrapper<IThrow>.Decorate(impl, provider, new ContextProvider());
            try
            {
                await proxy.ThrowException();
            }
            catch (TargetInvocationException outer)
            {
                var inner = outer.InnerException;
                inner.ShouldBeOfType<ApplicationException>();
            }
        }

        [Fact]
        public async Task ExecutesPolicy()
        {
            var impl = new Throw();
            var executed = false;
            var provider = new CallbackPolicyProvider((method, arguments) => executed = true);
            var proxy = PollyWrapper<IThrow>.Decorate(impl, provider, new ContextProvider());
            try
            {
                await proxy.ThrowException();
            }
            catch
            {
            }
            executed.ShouldBeTrue();
        }
    }
}