using System.Linq;
using System.Reflection;
using Moq;
using Polly;
using Shouldly;
using Xunit;

namespace LightningBug.Polly.Providers
{
    public class CompositePolicyProviderShould
    {
        public interface IService
        {
            void SayHello();
        }

        private readonly MethodInfo _testMethod = typeof(IService).GetMethod(nameof(IService.SayHello));

        private CallContextBase GetTestContext()
        {
            var serviceInstance = new Mock<IService>();
            var context = new CallContext(typeof(IService), serviceInstance.Object, _testMethod, new object[0]);
            return context;
        }

        [Fact]
        public void ComposesMultipleSyncProviders()
        {
            var context = GetTestContext();
            var providerMocks = 1.To(100).Select(_ => new Mock<IPolicyProvider>()).ToArray();

            foreach (var providerMock in providerMocks)
            {
                providerMock
                    .Setup(p => p.GetSyncPolicy(context))
                    .Returns<CallContextBase>(mi => Policy.NoOp());
            }

            var providers = providerMocks.Select(p => p.Object).ToArray();

            var sut = new CompositePolicyProvider(providers);
            var result = sut.GetSyncPolicy(context);
            result.ShouldNotBeNull();

            foreach (var providerMock in providerMocks)
            {
                providerMock.Verify(p => p.GetSyncPolicy(context), Times.Once);
            }
        }
        [Fact]
        public void ComposesMultipleAsyncProviders()
        {
            var context = GetTestContext();
            var providerMocks = 1.To(100).Select(_ => new Mock<IPolicyProvider>()).ToArray();

            foreach (var providerMock in providerMocks)
            {
                providerMock
                    .Setup(p => p.GetAsyncPolicy(context))
                    .Returns<CallContextBase>(mi => Policy.NoOpAsync());
            }

            var providers = providerMocks.Select(p => p.Object).ToArray();

            var sut = new CompositePolicyProvider(providers);
            var result = sut.GetAsyncPolicy(context);
            result.ShouldNotBeNull();

            foreach (var providerMock in providerMocks)
            {
                providerMock.Verify(p => p.GetAsyncPolicy(context), Times.Once);
            }
        }
    }
}
