using System;
using System.Collections.Generic;
using LightningBug.Polly.Providers;
using LightningBug.Polly.Providers.Attributes;
using LightningBug.Polly.Providers.Attributes.Scope;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Polly;
using Shouldly;
using Xunit;

namespace LightningBug.Polly.DependencyInjection.PolicyProviderRegistrationExtensions
{
    public class AddPollyProvidersShould
    {
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
                throw new NotImplementedException();
            }

            public override IAsyncPolicy GetAsyncPolicy(CallContextBase context, TestAttribute attribute)
            {
                throw new NotImplementedException();
            }
        }

        public abstract class AbstractTestprovider : AttributePolicyProviderBase<TestAttribute>
        {
        }

        public class OpenGenericTestProvider<TObject> : AttributePolicyProviderBase<TestAttribute>
        {
            public override ISyncPolicy GetSyncPolicy(CallContextBase context, TestAttribute attribute)
            {
                throw new NotImplementedException();
            }

            public override IAsyncPolicy GetAsyncPolicy(CallContextBase context, TestAttribute attribute)
            {
                throw new NotImplementedException();
            }
        }

        public interface IInterfaceTestProvider : IAttributePolicyProvider<TestAttribute>
        {
        }

        public class NonAttributeProvider : IAttributePolicyProvider
        {
            public ISyncPolicy GetSyncPolicy(CallContextBase context, PolicyAttribute attribute)
            {
                throw new NotImplementedException();
            }

            public IAsyncPolicy GetAsyncPolicy(CallContextBase context, PolicyAttribute attribute)
            {
                throw new NotImplementedException();
            }
        }

        public class NotBaseClassProvider : IAttributePolicyProvider<TestAttribute>
        {
            public ISyncPolicy GetSyncPolicy(CallContextBase context, PolicyAttribute attribute)
            {
                throw new NotImplementedException();
            }

            public IAsyncPolicy GetAsyncPolicy(CallContextBase context, PolicyAttribute attribute)
            {
                throw new NotImplementedException();
            }

            public ISyncPolicy GetSyncPolicy(CallContextBase context, TestAttribute attribute)
            {
                throw new NotImplementedException();
            }

            public IAsyncPolicy GetAsyncPolicy(CallContextBase context, TestAttribute attribute)
            {
                throw new NotImplementedException();
            }
        }


        private void RegistersProviderImplementation(Func<ServiceDescriptor, bool> check)
        {
            var serviceCollection = new Mock<IServiceCollection>();

            ServiceDescriptor serviceDescriptor = null;

            serviceCollection
                .Setup(sc => sc.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(TestProvider))))
                .Callback<ServiceDescriptor>(sd => serviceDescriptor = sd);

            serviceCollection.Object.AddPollyProviders(typeof(TestProvider));
            serviceDescriptor.ShouldNotBeNull();
            check(serviceDescriptor).ShouldBeTrue();
        }

        private void RegistersProviderService(Func<ServiceDescriptor, bool> check)
        {
            var serviceCollection = new Mock<IServiceCollection>();

            ServiceDescriptor serviceDescriptor = null;

            serviceCollection
                .Setup(sc => sc.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IAttributePolicyProvider))))
                .Callback<ServiceDescriptor>(sd => serviceDescriptor = sd);

            serviceCollection.Object.AddPollyProviders(typeof(TestProvider));
            serviceDescriptor.ShouldNotBeNull();
            check(serviceDescriptor).ShouldBeTrue();
        }

        [Fact]
        public void RegistersTwoItems()
        {
            var serviceCollection = new Mock<IServiceCollection>();
            var registrations = new HashSet<ServiceDescriptor>();

            serviceCollection.Setup(sc => sc.Add(It.IsAny<ServiceDescriptor>()))
                .Callback<ServiceDescriptor>(sd => registrations.Add(sd));

            serviceCollection.Object.AddPollyProviders(typeof(TestProvider));
            serviceCollection.Verify(sc => sc.Add(It.IsAny<ServiceDescriptor>()), Times.Exactly(2));
        }

        [Fact]
        public void RegisterTheImplementationSingleton()
        {
            RegistersProviderImplementation(sd => sd.Lifetime == ServiceLifetime.Singleton);
        }

        [Fact]
        public void RegisterTheServiceSingleton()
        {
            RegistersProviderService(sd => sd.Lifetime == ServiceLifetime.Singleton);
        }


        [Fact]
        public void RegistersTheProviderImplementation()
        {
            RegistersProviderImplementation(sd => sd.ImplementationType == typeof(TestProvider));
        }

        [Fact]
        public void RegistersTheProviderServiceAsAFactory()
        {
            RegistersProviderService(sd => sd.ImplementationFactory != null);
        }

        [Fact]
        public void RegisteredFactoryUsesPollyWrapper()
        {
            var serviceCollection = new Mock<IServiceCollection>();
            Func<IServiceProvider, object> factory = null;

            serviceCollection.Setup(sc => sc.Add(It.IsAny<ServiceDescriptor>()))
                .Callback<ServiceDescriptor>(sd => factory = sd.ImplementationFactory);

            serviceCollection.Object.AddPollyProviders(typeof(TestProvider));

            var serviceProvider = new Mock<IServiceProvider>();

            serviceProvider.Setup(sp => sp.GetService(typeof(TestProvider)))
                .Returns(new TestProvider());

            var result = factory(serviceProvider.Object);

            serviceProvider.Verify(sp => sp.GetService(typeof(TestProvider)), Times.Once);

            result.ShouldBeAssignableTo<IAttributePolicyProvider>();
            result.ShouldNotBeAssignableTo<TestProvider>();
            result.ShouldBeAssignableTo<SimpleScopePolicyProvider<TestAttribute>>();
        }

        [Fact]
        public void ShouldIgnoreBadTypes()
        {
            var serviceCollection = new Mock<IServiceCollection>();

            serviceCollection.Object.AddPollyProviders(
                typeof(AbstractTestprovider),
                typeof(OpenGenericTestProvider<>),
                typeof(IInterfaceTestProvider),
                typeof(NonAttributeProvider)
            );

            serviceCollection.Verify(sc => sc.Add(It.IsAny<ServiceDescriptor>()), Times.Never);
        }

        [Fact]
        public void ShouldRegisterTypes()
        {
            var serviceCollection = new Mock<IServiceCollection>();

            serviceCollection.Object.AddPollyProviders(
                typeof(OpenGenericTestProvider<object>),
                typeof(NotBaseClassProvider)
            );

            serviceCollection.Verify(sc => sc.Add(It.IsAny<ServiceDescriptor>()), Times.Exactly(4));
        }
    }
}