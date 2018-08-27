using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace LightningBug.Polly
{
    internal static class MethodInfoExtensions
    {

        private static readonly MethodInfo addReturnValueToAsyncVoid;

        static MethodInfoExtensions()
        {
            addReturnValueToAsyncVoid = typeof(MethodInfoExtensions).GetMethod(nameof(AddReturnValueToAsyncVoid), BindingFlags.NonPublic | BindingFlags.Static);
            if (addReturnValueToAsyncVoid == null)
                throw new Exception($"Unable to get {nameof(AddReturnValueToAsyncVoid)} method in {nameof(MethodInfoExtensions)}");
        }

        public static Func<TInstanceType, object[], Task<object>> BuildAsyncDelegate<TInstanceType>(this MethodInfo mi)
        {
            var returnType = mi.IsVoidAsync() ? typeof(void) : mi.GetAsyncReturnType();

            var parameters = mi.GetParameters();

            var instanceParam = Expression.Parameter(typeof(TInstanceType), "instance");

            var argumentsArray = Expression.Parameter(typeof(object[]), "args");
            var arguments = GetCallArguments(parameters, instanceParam, argumentsArray);

            var call = Expression.Call(instanceParam, mi, arguments);

            Expression body;

            if (returnType == typeof(void))
            {
                body = Expression.Call(null, addReturnValueToAsyncVoid, call);
            }
            else
            {
                var convert = TaskConversionCache.MakeGenericExpression(returnType);
                body = Expression.Invoke(convert, call);
            }

            var lambda = Expression.Lambda<Func<TInstanceType, object[], Task<object>>>(body, instanceParam, argumentsArray);

            try
            {
                var func = lambda.Compile();
                return func;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Problem compiling {mi.Name} defined in {mi.DeclaringType.FullName}", ex);
            }
        }

        private static async Task<object> AddReturnValueToAsyncVoid(Task task)
        {
            await task;
            return null;
        }

        public static Func<TInstanceType, object[], object> BuildDelegate<TInstanceType>(this MethodInfo mi)
        {
            var parameters = mi.GetParameters();

            var instanceParam = Expression.Parameter(typeof(TInstanceType), "instance");

            var argumentsArray = Expression.Parameter(typeof(object[]), "args");
            var arguments = GetCallArguments(parameters, instanceParam, argumentsArray);

            var call = Expression.Call(instanceParam, mi, arguments);

            var body = mi.ReturnType != typeof(void)
                ? (Expression) call
                : Expression.Block(call, Expression.Constant(null, typeof(object)));

            var lambda = Expression.Lambda<Func<TInstanceType, object[], object>>(body, instanceParam, argumentsArray);

            try
            {
                var func = lambda.Compile();
                return func;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Problem compiling {mi.Name} defined in {mi.DeclaringType.FullName}", ex);
            }
        }

        private static IEnumerable<Expression> GetCallArguments(ParameterInfo[] parameters, Expression instanceParam, Expression argumentsArray)
        {
            for (var i = 0; i < parameters.Length; i++)
            {
                var argument = Expression.ArrayIndex(argumentsArray, Expression.Constant(i, typeof(int)));
                var cast = Expression.Convert(argument, parameters[i].ParameterType);
                yield return cast;
            }
        }

    }
}