using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace LightningBug.Polly
{
    internal static class AsyncMethodInfoExtensions
    {
        public static bool IsAsync(this MethodInfo mi)
        {
            return mi.IsVoidAsync() || mi.IsAsyncWithReturnParameter();
        }

        public static bool IsVoidAsync(this MethodInfo mi)
        {
            return mi.ReturnType == typeof(Task);
        }

        public static bool IsAsyncWithReturnParameter(this MethodInfo mi)
        {
            var returnType = mi.ReturnType;
            if (!returnType.IsConstructedGenericType) return false;
            return returnType.GetGenericTypeDefinition() == typeof(Task<>);
        }

        public static Type GetAsyncReturnType(this MethodInfo mi)
        {
            var returnType = mi.ReturnType;
            return returnType.GetGenericArguments().Single();
        }
    }
}