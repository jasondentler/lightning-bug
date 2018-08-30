using System;
using System.Collections.Generic;
using System.Reflection;
using Polly;

namespace LightningBug.Polly.Providers
{
    public abstract class CallContextBase : Context
    {
        internal const string CallContextKey = "__CALL__CONTEXT__";

        public abstract Type ServiceType { get; }
        public abstract object Instance { get; }
        public abstract MethodInfo Method { get; }
        public abstract IDictionary<string, object> Arguments { get; }
        public abstract IDictionary<string, Type> ParameterTypes { get; }

        protected CallContextBase()
        {
            base[CallContextKey] = this;
        }

    }
}