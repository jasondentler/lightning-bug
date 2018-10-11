using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LightningBug.Polly.Providers;
using LightningBug.Polly.Providers.Attributes;
using Microsoft.Extensions.DependencyInjection.LightningBug.Polly.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class PollyRegistrationExtensions
    {

        public static void AddPolly(this IServiceCollection serviceCollection, bool throwOnMissingProvider = false)
        {
            var assemblies = Assembly.GetCallingAssembly().FindAllAssemblies().ToArray();
            serviceCollection.AddPolly(throwOnMissingProvider, assemblies);
        }

        public static void AddPolly(this IServiceCollection serviceCollection, bool throwOnMissingProvider = false, params Assembly[] assemblies)
        {
            var types = assemblies.SelectMany(asm => asm.GetTypes()).ToArray();
            serviceCollection.AddPolly(throwOnMissingProvider, types);
        }

        public static void AddPolly(this IServiceCollection serviceCollection, bool throwOnMissingProvider = false,
            params Type[] types)
        {
            serviceCollection.AddTransient<IPolicyProvider, AttributePolicyProvider>();
            serviceCollection.AddTransient<IAttributePolicyProvider[]>(sp => sp.GetServices<IAttributePolicyProvider>().ToArray());
            serviceCollection.AddTransient<IContextProvider, ContextProvider>();

            var providers = types.Where(t => t.IsPolicyProvider()).ToArray();
            var providerAttributes = providers
                .SelectMany(t => t.GetAttributeTypesOfPolicyProvider(),
                    (provider, attribute) => new {provider, attribute})
                .ToArray();

            var services = new HashSet<Type>(types.Where(t => t.IsInterfaceWithPolicyAttribute()));

            if (throwOnMissingProvider)
                CheckForMissingProviders(providerAttributes.Select(x => x.attribute).Distinct().ToArray(), services);

            var implementations = types
                .SelectMany(t => t.GetInterfaces(), (implementation, service) => new {implementation, service})
                .Where(x => x.service.IsInterfaceWithPolicyAttribute())
                .ToArray();

            var implementationMap = implementations
                .GroupBy(x => x.service)
                .ToDictionary(x => x.Key, x => new HashSet<Type>(x.Select(y => y.implementation).ToArray()));

            CheckForMissingImplementations(services, implementationMap);

            foreach (var providerAttribute in providerAttributes)
                serviceCollection.AddPollyProvider(providerAttribute.attribute, providerAttribute.provider, PolicyProviderRegistrationExtensions.DefaultScope);

            foreach (var x in implementations)
                serviceCollection.AddResilientScoped(x.service, x.implementation);
        }

        private static void CheckForMissingImplementations(IEnumerable<Type> services, Dictionary<Type, HashSet<Type>> implementations)
        {
            var serviceSet = new HashSet<Type>(services);
            serviceSet.ExceptWith(implementations.Keys);

            if (serviceSet.Any())
                throw new ApplicationException();
        }


        private static void CheckForMissingProviders(Type[] providerAttributes, ISet<Type> services)
        {
            var serviceAttributes = services
                .SelectMany(t => t.GetAttributeTypesOfServiceInterface(),
                    (service, policyAttribute) => new { service, policyAttribute = policyAttribute.GetType() })
                .GroupBy(x => x.policyAttribute)
                .ToDictionary(g => g.Key, g => g.Select(x => x.service).ToArray());

            var usedAttributeMissingProvider = serviceAttributes.Keys.Except(providerAttributes)
                .Where(missing => missing.GetTypeHeirarchy().Any(parent => providerAttributes.Contains(parent)))
                .FirstOrDefault();

            if (usedAttributeMissingProvider == default(Type)) return;

            throw new MissingPolicyProviderException(usedAttributeMissingProvider, serviceAttributes[usedAttributeMissingProvider]);
        }
    }

}
