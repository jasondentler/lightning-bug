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

        private readonly MethodInfo _testMethod = typeof(CompositePolicyProviderShould).GetMethod(nameof(ComposesMultipleSyncProviders));

        [Fact]
        public void ComposesMultipleSyncProviders()
        {
            var providerMocks = 1.To(100).Select(_ => new Mock<IPolicyProvider>()).ToArray();

            foreach (var providerMock in providerMocks)
            {
                providerMock
                    .Setup(p => p.GetSyncPolicy(_testMethod))
                    .Returns<MethodInfo>(mi => Policy.NoOp());
            }

            var providers = providerMocks.Select(p => p.Object).ToArray();

            var sut = new CompositePolicyProvider(providers);
            var result = sut.GetSyncPolicy(_testMethod);
            result.ShouldNotBeNull();

            foreach (var providerMock in providerMocks)
            {
                providerMock.Verify(p => p.GetSyncPolicy(_testMethod), Times.Once);
            }
        }
        [Fact]
        public void ComposesMultipleAsyncProviders()
        {
            var providerMocks = 1.To(100).Select(_ => new Mock<IPolicyProvider>()).ToArray();

            foreach (var providerMock in providerMocks)
            {
                providerMock
                    .Setup(p => p.GetAsyncPolicy(_testMethod))
                    .Returns<MethodInfo>(mi => Policy.NoOpAsync());
            }

            var providers = providerMocks.Select(p => p.Object).ToArray();

            var sut = new CompositePolicyProvider(providers);
            var result = sut.GetAsyncPolicy(_testMethod);
            result.ShouldNotBeNull();

            foreach (var providerMock in providerMocks)
            {
                providerMock.Verify(p => p.GetAsyncPolicy(_testMethod), Times.Once);
            }
        }
    }
}
