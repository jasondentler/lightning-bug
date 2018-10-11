using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using LightningBug.Polly;
using LightningBug.Polly.Providers;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceRegistrationExtensions
    {
        public static void AddResilientTransient<TService, TImplementation>(this IServiceCollection serviceCollection)
            where TService : class
            where TImplementation : class, TService
        {
            if (serviceCollection == null) throw new ArgumentNullException(nameof(serviceCollection));
            serviceCollection.AddTransient<TImplementation, TImplementation>();
            serviceCollection.AddTransient(provider => provider.GetWrapped<TService, TImplementation>());
        }

        public static void AddResilientScoped<TService, TImplementation>(this IServiceCollection serviceCollection)
            where TService : class
            where TImplementation : class, TService
        {
            if (serviceCollection == null) throw new ArgumentNullException(nameof(serviceCollection));
            serviceCollection.AddScoped<TImplementation, TImplementation>();
            serviceCollection.AddScoped(provider =>
            {
                Debug.WriteLine($"Getting wrapped {typeof(TService).FullName} implementation {typeof(TImplementation).FullName}");
                return provider.GetWrapped<TService, TImplementation>();
            });
        }

        internal static void AddResilientScoped(this IServiceCollection serviceCollection, Type service, Type implementation)
        {
            if (serviceCollection == null) throw new ArgumentNullException(nameof(serviceCollection));
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (implementation == null) throw new ArgumentNullException(nameof(implementation));

            var genericMethod = typeof(ServiceRegistrationExtensions).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(mi => mi.ContainsGenericParameters)
                .Where(mi => mi.Name.StartsWith(nameof(AddResilientScoped)))
                .Single();

            var concreteMethod = genericMethod.MakeGenericMethod(service, implementation);
            concreteMethod.Invoke(null, new object[] {serviceCollection});
        }

        public static void AddResilientSingleton<TService, TImplementation>(this IServiceCollection serviceCollection)
            where TService : class
            where TImplementation : class, TService
        {
            if (serviceCollection == null) throw new ArgumentNullException(nameof(serviceCollection));
            Debug.WriteLine($"Registering polly wrapper factory for {typeof(TService).Name} implemented by {typeof(TImplementation).Name}");
            serviceCollection.AddSingleton<TImplementation, TImplementation>();
            serviceCollection.AddSingleton(provider => provider.GetWrapped<TService, TImplementation>());
        }

        public static void AddResilientSingleton<TService, TImplementation>(this IServiceCollection serviceCollection, TImplementation instance)
            where TService : class
            where TImplementation : class, TService
        {
            if (serviceCollection == null) throw new ArgumentNullException(nameof(serviceCollection));
            serviceCollection.AddSingleton(instance);
            serviceCollection.AddSingleton(serviceProvider =>
            {
                if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
                var policyProvider = serviceProvider.GetRequiredService<IPolicyProvider>();
                var contextProvider = serviceProvider.GetRequiredService<IContextProvider>();
                var pollyWrapperFactory = new PollyWrapperFactory<TService, TImplementation>(instance, policyProvider, contextProvider);
                var wrappedInstance = pollyWrapperFactory.Wrap();
                return wrappedInstance;
            });
        }

        private static TService GetWrapped<TService, TImplementation>(this IServiceProvider serviceProvider)
            where TService : class
            where TImplementation : class, TService
        {
            if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
            var wrapperFactory = serviceProvider.GetPollyWrapperFactory<TService, TImplementation>();
            return wrapperFactory.Wrap();
        }

        private static PollyWrapperFactory<TService, TImplementation> GetPollyWrapperFactory<TService, TImplementation>(this IServiceProvider serviceProvider) 
            where TService : class 
            where TImplementation : class, TService
        {
            if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
            Debug.WriteLine($"Getting polly wrapper factory for {typeof(TService).Name} implemented by {typeof(TImplementation).Name}");

            var impl = serviceProvider.GetRequiredService<TImplementation>();
            var policyProvider = serviceProvider.GetRequiredService<IPolicyProvider>();
            var contextProvider = serviceProvider.GetRequiredService<IContextProvider>();
            return new PollyWrapperFactory<TService, TImplementation>(impl, policyProvider, contextProvider);
        }

    }
}
