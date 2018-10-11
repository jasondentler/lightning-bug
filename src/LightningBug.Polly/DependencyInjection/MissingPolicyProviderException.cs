using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection.LightningBug.Polly.DependencyInjection
{
    public class MissingPolicyProviderException : ApplicationException
    {
        public Type AttributeType { get; }
        public Type[] ServiceInterfaces { get; }

        public MissingPolicyProviderException(Type attributeType, Type[] serviceInterfaces)
        {
            AttributeType = attributeType;
            ServiceInterfaces = serviceInterfaces;
            Message = BuildMessage(attributeType, serviceInterfaces);
        }

        private static string BuildMessage(Type attributeType, Type[] serviceInterfaces)
        {
            if (serviceInterfaces.Length == 1)
                return $"Policy attribute {attributeType.FullName} has no policy provider. The policy can't be applied to {serviceInterfaces.Single().FullName}.";


            return $"Policy attribute {attributeType.FullName} has no policy provider. The policy can't be applied, but is used on {serviceInterfaces.Length} services.";
        }

        public override string Message { get; }
    }
}