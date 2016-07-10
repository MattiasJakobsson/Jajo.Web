using System.Collections.Generic;
using System.Linq;

namespace SuperGlue.Configuration.Ioc
{
    public static class ServiceResolverExtensions
    {
        public static T Resolve<T>(this IResolveServices serviceResolver)
        {
            return (T)serviceResolver.Resolve(typeof(T));
        }

        public static IEnumerable<T> ResolveAll<T>(this IResolveServices serviceResolver)
        {
            return serviceResolver.ResolveAll(typeof(T)).OfType<T>();
        }
    }
}