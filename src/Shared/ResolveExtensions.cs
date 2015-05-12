using System;
using System.Collections.Generic;
using System.Linq;

namespace Jajo.Web
{
    internal static class ResolveExtensions
    {
        public static TService Resolve<TService>(this IDictionary<string, object> environment)
        {
            return (TService)Resolve(environment, typeof(TService));
        }

        public static object Resolve(this IDictionary<string, object> environment, Type serviceType)
        {
            return environment.Get<Func<Type, object>>("jajo.ResolveInstance")(serviceType);
        }

        public static IEnumerable<TService> ResolveAll<TService>(this IDictionary<string, object> environment)
        {
            return ResolveAll(environment, typeof(TService)).OfType<TService>();
        }

        public static IEnumerable<object> ResolveAll(this IDictionary<string, object> environment, Type serviceType)
        {
            return environment.Get<Func<Type, IEnumerable<object>>>("jajo.ResolveAllInstances")(serviceType);
        }
    }
}