using System;
using System.Linq.Expressions;
using System.Reflection;

namespace LightningBug.Reflection
{
    public static class ReflectionExtensions
    {

        /// <summary>
        /// Creates a delegate for the getter of the property
        /// </summary>
        /// <typeparam name="TInstanceType">Type of instance to read</typeparam>
        /// <param name="pi">Property with getter</param>
        /// <returns>A delegate for the getter of the property</returns>
        public static Func<TInstanceType, object> BuildGetDelegate<TInstanceType>(this PropertyInfo pi)
        {
            var instanceParam = Expression.Parameter(typeof(TInstanceType), "instance");

            var result = Expression.MakeMemberAccess(instanceParam, pi);

            var convert = Expression.Convert(result, typeof(object));

            var lambda = Expression.Lambda<Func<TInstanceType, object>>(convert, instanceParam);

            return lambda.Compile();
        }

        /// <summary>
        /// Creates a delegate for the setter of the property
        /// </summary>
        /// <typeparam name="TInstanceType">Type of instance to write</typeparam>
        /// <param name="pi">Property with setter</param>
        /// <returns>A delegate for the setter of the property</returns>
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