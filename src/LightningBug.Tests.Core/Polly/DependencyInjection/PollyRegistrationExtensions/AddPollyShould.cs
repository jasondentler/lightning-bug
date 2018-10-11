using System;
using System.Collections.Generic;
using System.Linq;
using LightningBug.Polly.Providers;
using LightningBug.Polly.Providers.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Polly;
using Shouldly;
using Xunit;

namespace LightningBug.Polly.DependencyInjection.PollyRegistrationExtensions
{
    public class AddPollyShould
    {

        public interface ITestService
        {
            [TestAttribute]
            void Method();
        }

        public class TestService : ITestService
        {
            public void Method()
            {
            }
        }

        public class TestAttribute : PolicyAttribute
        {
            protected internal override int GetOrder()
            {
                return 0;
            }
        }

        public class TestProvider : AttributePolicyProviderBase<TestAttribute>
        {
            public override ISyncPolicy GetSyncPolicy(CallContextBase context, TestAttribute attribute)
            {
                return Policy.NoOp();
            }

            public override IAsyncPolicy GetAsyncPolicy(CallContextBase context, TestAttribute attribute)
            {
                return Policy.NoOpAsync();
            }
        }

        [Fact]
        public void NotThrow()
        {
            var serviceCollection = new Mock<IServiceCollection>();
            serviceCollection.Object.AddPolly(true);
        }

        private IEnumerable<ServiceDescriptor> GetResultingServiceDescriptors()
        {
            var serviceCollection = new Mock<IServiceCollection>();
            var registrations = new HashSet<ServiceDescriptor>();
            serviceCollection.Setup(sc => sc.Add(It.IsAny<ServiceDescriptor>()))
                .Callback<ServiceDescriptor>(sd => registrations.Add(sd));

            serviceCollection.Object.AddPolly(true, GetType().GetNestedTypes());

            return registrations;
        }

        [Fact]
        public void RegisterProvider()
        {
            var serviceDescriptors = GetResultingServiceDescriptors();
            serviceDescriptors
                .Where(sd => sd.ServiceType == typeof(IAttributePolicyProvider))
                .Count().ShouldBe(1);
        }

        [Fact]
        public void RegisterService()
        {
            var serviceDescriptors = GetResultingServiceDescriptors();
            serviceDescriptors
                .Where(sd => sd.ServiceType == typeof(ITestService))
                .Count().ShouldBe(1);
        }
    }
}
