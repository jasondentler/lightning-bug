using System;
using System.Linq.Expressions;
using LightningBug.Polly.DependencyInjection.PolicyProviderRegistrationExtensions;
using LightningBug.Polly.Providers;
using LightningBug.Polly.Providers.Attributes;
using LightningBug.Polly.Wrapper;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using Xunit;

namespace LightningBug.Polly.DependencyInjection.ServiceRegistrationExtensions
{
    public class AddResilientSingletonShould
    {

        public interface IService
        {
        }

        public class Service : IService
        {
        }

        private void Registers(Expression<Func<ServiceDescriptor, bool>> check)
        {
            var serviceCollection = new Mock<IServiceCollection>();
            serviceCollection.Object.AddResilientSingleton<IService, Service>();
            serviceCollection.Verify(sc => sc.Add(It.Is<ServiceDescriptor>(check)));
        }

        [Fact]
        public void RegisterTwoItems()
        {
            var serviceCollection = new Mock<IServiceCollection>();
            serviceCollection.Object.AddResilientTransient<IService, Service>();
            serviceCollection.Verify(sc => sc.Add(It.IsAny<ServiceDescriptor>()), Times.Exactly(2));
        }

        [Fact]
        public void RegisterItSingleton()
        {
            Registers(sd => sd.Lifetime == ServiceLifetime.Singleton);
        }

        [Fact]
        public void RegistersIService()
        {
            Registers(sd => sd.ServiceType == typeof(IService));
        }

        [Fact]
        public void RegistersAFactory()
        {
            Registers(sd => sd.ImplementationFactory != null);
        }

        [Fact]
        public void RegisteredFactoryUsesPollyWrapper()
        {
            var serviceCollection = new Mock<IServiceCollection>();
            Func<IServiceProvider, object> factory = null;

            serviceCollection.Setup(sc => sc.Add(It.IsAny<ServiceDescriptor>()))
                .Callback<ServiceDescriptor>(sd => factory = sd.ImplementationFactory);

            serviceCollection.Object.AddResilientSingleton<IService, Service>();

            var serviceProvider = new Mock<IServiceProvider>();

            var instance = new Service();

            serviceProvider.Setup(sp => sp.GetService(typeof(Service)))
                .Returns(instance);
            serviceProvider.Setup(sp => sp.GetService(typeof(IPolicyProvider)))
                .Returns(new AttributePolicyProvider(new AddPollyProvidersShould.NotBaseClassProvider()));
            serviceProvider.Setup(sp => sp.GetService(typeof(IContextProvider)))
                .Returns(new ContextProvider());

            serviceProvider.Setup(sp => sp.GetService(typeof(PollyWrapperFactory<IService, Service>)))
                .Returns(new PollyWrapperFactory<IService, Service>(instance, new NoOpPolicyProvider(), new ContextProvider()));

            var result = factory(serviceProvider.Object);

            result.ShouldBeAssignableTo<IService>();
            result.ShouldNotBeAssignableTo<Service>();
        }

    }
}