using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using LightningBug.Polly.Providers.Attributes;
using LightningBug.Polly.Providers.Attributes.Scope;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class PolicyProviderRegistrationExtensions
    {

        public const Scopes DefaultScope = Scopes.OnePerConcreteType;

        public static void AddPollyProviders(this IServiceCollection serviceCollection, IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                if (!type.IsClass) continue;
                if (type.IsAbstract) continue;
                if (type.IsGenericTypeDefinition) continue;
                var attributes = type.GetInterfaces()
                    .Where(t => t.IsConstructedGenericType)
                    .Select(t => new
                    {
                        @interface = t.GetGenericTypeDefinition(),
                        arguments = t.GetGenericArguments()
                    })
                    .Where(x => x.@interface == typeof(IAttributePolicyProvider<>))
                    .Where(x => x.arguments.Length == 1)
                    .Select(x => x.arguments.Single())
                    .ToArray();

                if (!attributes.Any()) continue;

                foreach (var attributeType in attributes)
                {
                    serviceCollection.AddPollyProvider(attributeType, type, DefaultScope);
                }
            }
        }

        public static void AddPollyProviders(this IServiceCollection serviceCollection, params Type[] types)
        {
            serviceCollection.AddPollyProviders((IEnumerable<Type>) types);
        }

        public static void AddPollyProviders(this IServiceCollection serviceCollection, IEnumerable<Assembly> assemblies)
        {
            serviceCollection.AddPollyProviders(assemblies.SelectMany(asm => asm.GetTypes()));
        }

        public static void AddPollyProviders(this IServiceCollection serviceCollection, params Assembly[] assemblies)
        {
            serviceCollection.AddPollyProviders((IEnumerable<Assembly>) assemblies);
        }

        public static void AddPollyProvider(this IServiceCollection serviceCollection, Type attributeType, Type providerType, Scopes scope)
        {
            var scopeProviderType = typeof(SimpleScopePolicyProvider<>);
            scopeProviderType = scopeProviderType.MakeGenericType(attributeType);

            var parameterTypes = new[] {typeof(IAttributePolicyProvider), typeof(Scopes)};
            var parameters = parameterTypes.Select(t => Expression.Parameter(t)).ToArray();

            var constructorInfo = scopeProviderType.GetConstructor(parameterTypes);
            var constructor = Expression.New(constructorInfo, parameters);
            var funcExpr = Expression.Lambda<Func<IAttributePolicyProvider, Scopes, IAttributePolicyProvider>>(constructor, parameters);
            var factory = funcExpr.Compile();

            serviceCollection.AddSingleton(providerType, providerType);
            serviceCollection.AddSingleton(typeof(IAttributePolicyProvider), serviceProvider =>
            {
                var attributePolicyProvider = (IAttributePolicyProvider) serviceProvider.GetService(providerType);
                var scopeProvider = factory(attributePolicyProvider, scope);
                return scopeProvider;
            });
        }

        public static void AddPollyProvider<TAttribute, TProvider>(this IServiceCollection serviceCollection)
            where TAttribute : PolicyAttribute
            where TProvider : class, IAttributePolicyProvider
        {
            serviceCollection.AddPollyProvider<TAttribute, TProvider>(DefaultScope);
        }

        public static void AddPollyProvider<TAttribute, TProvider>(this IServiceCollection serviceCollection, Scopes scope) 
            where TAttribute : PolicyAttribute
            where TProvider : class, IAttributePolicyProvider
        {
            serviceCollection.AddSingleton<TProvider, TProvider>();
            serviceCollection.AddSingleton<IAttributePolicyProvider>(serviceProvider =>
            {
                var attributePolicyProvider = serviceProvider.GetService<TProvider>();
                var scopeProvider = new SimpleScopePolicyProvider<TAttribute>(attributePolicyProvider, scope);
                return scopeProvider;
            });
        }
    }
}