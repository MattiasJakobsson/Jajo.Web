using System;
using System.Collections.Generic;
using SuperGlue.Configuration.Ioc;

namespace SuperGlue.Configuration
{
    public static class IocExtensions
    {
        public static T Resolve<T>(this IDictionary<string, object> environment)
        {
            var resolver = environment.Get<IResolveServices>(SetupIocConfiguration.ServiceResolverKey);

            return resolver.Resolve<T>();
        }

        public static object Resolve(this IDictionary<string, object> environment, Type type)
        {
            var resolver = environment.Get<IResolveServices>(SetupIocConfiguration.ServiceResolverKey);

            return resolver.Resolve(type);
        }

        public static IEnumerable<T> ResolveAll<T>(this IDictionary<string, object> environment)
        {
            var resolver = environment.Get<IResolveServices>(SetupIocConfiguration.ServiceResolverKey);

            return resolver.ResolveAll<T>();
        }

        public static IEnumerable<object> ResolveAll(this IDictionary<string, object> environment, Type type)
        {
            var resolver = environment.Get<IResolveServices>(SetupIocConfiguration.ServiceResolverKey);

            return resolver.ResolveAll(type);
        }
    }
}