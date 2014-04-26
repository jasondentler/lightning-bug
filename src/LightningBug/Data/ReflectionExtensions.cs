using System;
using System.Linq.Expressions;
using System.Reflection;

namespace LightningBug.Data
{
    public static class ReflectionExtensions
    {
        public static Func<TInstanceType, object> BuildGetDelegate<TInstanceType>(this PropertyInfo pi)
        {
            var instanceParam = Expression.Parameter(typeof(TInstanceType), "instance");

            var result = Expression.MakeMemberAccess(instanceParam, pi);

            var convert = Expression.Convert(result, typeof(object));

            var lambda = Expression.Lambda<Func<TInstanceType, object>>(convert, instanceParam);

            return lambda.Compile();
        }

        public static Action<TInstanceType, object> BuildSetDelegate<TInstanceType>(this PropertyInfo pi)
        {
            var instanceParam = Expression.Parameter(typeof(TInstanceType), "instance");
            var valueParam = Expression.Parameter(typeof(object), "value");

            var convert = Expression.Convert(valueParam, pi.PropertyType);
            var property = Expression.Property(instanceParam, pi);
            var assign = Expression.Assign(property, convert);

            var lambda = Expression.Lambda<Action<TInstanceType, object>>(assign, instanceParam, valueParam);

            return lambda.Compile();
        }
    }
}