using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace LightningBug.Polly
{
    public static class TaskConversionCache
    {
        private static readonly MethodInfo _makeSpecific;
        private static readonly MethodInfo _makeGeneric;
        private static readonly ConcurrentDictionary<Type, Func<Task<object>, Task>> _makeSpecificMap;
        private static readonly ConcurrentDictionary<Type, Func<Task, Task<object>>> _makeGenericMap;

        static TaskConversionCache()
        {
            _makeSpecific = typeof(TaskConversionCache)
                .GetMethod(nameof(MakeSpecificInternal), BindingFlags.NonPublic | BindingFlags.Static);

            if (_makeSpecific == null)
                throw new Exception();

            _makeGeneric = typeof(TaskConversionCache)
                .GetMethod(nameof(MakeGenericInternal), BindingFlags.NonPublic | BindingFlags.Static);

            if (_makeGeneric == null)
                throw new Exception();

            _makeSpecificMap = new ConcurrentDictionary<Type, Func<Task<object>, Task>>();
            _makeGenericMap = new ConcurrentDictionary<Type, Func<Task, Task<object>>>();
        }

        private static async Task<T> MakeSpecificInternal<T>(Task<object> task)
        {
            return (T)await task;
        }

        public static Func<Task<object>, Task<T>> MakeSpecific<T>()
        {
            return MakeSpecificInternal<T>;
        }

        public static Func<Task<object>, Task> MakeSpecific(Type t)
        {
            return _makeSpecificMap.GetOrAdd(t, _ =>
            {
                var mi = _makeSpecific.MakeGenericMethod(t);
                var pi = mi.GetParameters().Single();
                var param = Expression.Parameter(pi.ParameterType, pi.Name);
                var call = Expression.Call(null, mi, param);
                var cast = Expression.Convert(call, typeof(Task));
                var lambda = Expression.Lambda<Func<Task<object>, Task>>(cast, param);
                return lambda.Compile();
            });
        }

        private static async Task<object> MakeGenericInternal<T>(Task<T> task)
        {
            return await task;
        }

        public static Func<Task<T>, Task<object>> MakeGeneric<T>()
        {
            return MakeGenericInternal<T>;
        }

        public static Func<Task, Task<object>> MakeGeneric(Type t)
        {
            return _makeGenericMap.GetOrAdd(t, _ =>
            {
                var lambda = MakeGenericExpression(t);
                return lambda.Compile();
            });
        }

        public static Expression<Func<Task, Task<object>>> MakeGenericExpression(Type t)
        {
            var mi = _makeGeneric.MakeGenericMethod(t);
            var pi = mi.GetParameters().Single();
            var param = Expression.Parameter(typeof(Task), pi.Name);
            var cast = Expression.Convert(param, pi.ParameterType);
            var call = Expression.Call(null, mi, cast);
            var lambda = Expression.Lambda<Func<Task, Task<object>>>(call, param);
            return lambda;
        }

    }
}
