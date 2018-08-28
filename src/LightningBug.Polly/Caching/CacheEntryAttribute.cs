using System;
using System.Collections.Generic;
using System.Reflection;
using Polly.Caching;

namespace LightningBug.Polly.Caching
{
    public class CacheEntryAttribute : CacheAttribute
    {

        public static ICacheItemSerializer<object, string> KeySerializer = new JsonKeySerializer();

        private readonly string _keyPrefix;
        private readonly string _argumentName;

        public CacheEntryAttribute(string keyPrefix, string argumentName, double timeToLiveInMinutes = 24 * 60) : base(timeToLiveInMinutes)
        {
            if (string.IsNullOrWhiteSpace(argumentName))
            {
                throw new ArgumentException($"{nameof(argumentName)} should not be null, empty, or whitespace", nameof(argumentName));
            }

            _keyPrefix = keyPrefix;
            _argumentName = argumentName;
        }

        protected internal override string GetCacheKey(MethodInfo methodInfo, IDictionary<string, Type> parameterTypes, IDictionary<string, object> arguments)
        {
            var argumentValue = GetArgument(_argumentName, methodInfo, parameterTypes, arguments);
            var serializedArgument = SerializeArgument(_argumentName, argumentValue, KeySerializer, methodInfo, parameterTypes, arguments);

            if (string.IsNullOrEmpty(_keyPrefix))
                return serializedArgument;

            return $"{_keyPrefix}:{serializedArgument}";
        }

        protected virtual object GetArgument(string argumentName, MethodInfo methodInfo, IDictionary<string, Type> parameterTypes, IDictionary<string, object> arguments)
        {
            if (!arguments.ContainsKey(argumentName))
                throw new MissingArgumentException(_argumentName);

            return arguments[argumentName];
        }

        protected virtual string SerializeArgument(
            string argumentName, 
            object argumentValue,
            ICacheItemSerializer<object, string> serializer,
            MethodInfo methodInfo, 
            IDictionary<string, Type> parameterTypes, 
            IDictionary<string, object> arguments)
        {
            return serializer.Serialize(argumentValue);
        }
    }
}