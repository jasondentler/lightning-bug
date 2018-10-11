using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LightningBug.Polly.Providers.Attributes;

namespace Microsoft.Extensions.DependencyInjection
{

    internal static class TypeExtensions
    {
        public static IEnumerable<Type> GetTypeHeirarchy(this Type type)
        {
            while (type != typeof(object))
            {
                if (!type.CanCreateInstanceOfClass()) yield break;

                yield return type;

                type = type.BaseType;
            }
        }


        private static bool CanCreateInstanceOfClass(this Type type)
        {
            return type.IsClass
                   && !type.IsAbstract
                   && !type.IsGenericTypeDefinition;
        }

        public static bool IsPolicyAttribute(object attribute)
        {
            return attribute is PolicyAttribute;
        }

        public static bool IsPolicyAttribute(this Type type)
        {
            return type.CanCreateInstanceOfClass() 
                   && typeof(PolicyAttribute).IsAssignableFrom(type);
        }

        public static bool IsPolicyProvider(this Type type)
        {
            if (!typeof(IAttributePolicyProvider).IsAssignableFrom(type))
                return false;
            if (!type.CanCreateInstanceOfClass())
                return false;

            return type
                .GetInterfaces()
                .Where(i => i.IsConstructedGenericType)
                .Any(i => i.GetGenericTypeDefinition() == typeof(IAttributePolicyProvider<>));
        }

        public static IEnumerable<Type> GetAttributeTypesOfPolicyProvider(this Type type)
        {
            if (!type.IsPolicyProvider()) return Enumerable.Empty<Type>();

            return type
                .GetInterfaces()
                .Where(i => i.IsConstructedGenericType)
                .Where(i => i.GetGenericTypeDefinition() == typeof(IAttributePolicyProvider<>))
                .Select(i => i.GetGenericArguments().Single())
                .Where(attr => attr.IsPolicyAttribute())
                .ToArray();
        }

        public static bool IsInterfaceWithPolicyAttribute(this Type type)
        {
            return type.IsInterface
                   && !type.IsGenericTypeDefinition
                   && type.GetMembers().Any(mi => mi.IsMemberWithPolicyAttribute());
        }

        public static IEnumerable<PolicyAttribute> GetAttributeTypesOfServiceInterface(this Type type)
        {
            if (!type.IsInterface) return Enumerable.Empty<PolicyAttribute>();
            if (type.IsGenericTypeDefinition) return Enumerable.Empty<PolicyAttribute>();
            return type.GetMembers()
                .SelectMany(mi => mi.GetPolicyAttributes());
        }

        public static IEnumerable<PolicyAttribute> GetPolicyAttributes(this MemberInfo memberInfo)
        {
            return memberInfo.GetCustomAttributes(true)
                .Where(IsPolicyAttribute)
                .Cast<PolicyAttribute>();
        }

        public static bool IsMemberWithPolicyAttribute(this MemberInfo memberInfo)
        {
            return memberInfo.GetPolicyAttributes().Any();
        }

    }
}