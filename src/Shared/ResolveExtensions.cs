using System;
using System.Collections.Generic;
using System.Linq;

namespace SuperGlue
{
    internal static class ResolveExtensions
    {
        public static class ContainerConstants
        {
            public const string ResolveInstance = "superglue.ResolveInstance";
            public const string ResolveAllInstances = "superglue.ResolveAllInstances";
            public const string RegisterTransient = "superglue.Container.RegisterTransient";
            public const string RegisterSingleton = "superglue.Container.RegisterSingleton";
            public const string RegisterSingletonType = "superglue.Container.RegisterSingletonType";
            public const string RegisterAllClosing = "superglue.Container.RegisterAllClosing";
            public const string RegisterAll = "superglue.Container.RegisterAll";
            public const string RegisterTransientFromFunc = "superglue.Container.RegisterTransientFromFunc";
            public const string RegisterSingletonFromFunc = "superglue.Container.RegisterSingletonFromFunc";
        }

        public static TService Resolve<TService>(this IDictionary<string, object> environment)
        {
            return (TService)Resolve(environment, typeof(TService));
        }

        public static object Resolve(this IDictionary<string, object> environment, Type serviceType)
        {
            return environment.Get<Func<Type, object>>(ContainerConstants.ResolveInstance)(serviceType);
        }

        public static IEnumerable<TService> ResolveAll<TService>(this IDictionary<string, object> environment)
        {
            return ResolveAll(environment, typeof(TService)).OfType<TService>();
        }

        public static IEnumerable<object> ResolveAll(this IDictionary<string, object> environment, Type serviceType)
        {
            return environment.Get<Func<Type, IEnumerable<object>>>(ContainerConstants.ResolveAllInstances)(serviceType);
        }

        public static void RegisterTransient(this IDictionary<string, object> environment, Type serviceType, Type implimentationType)
        {
            environment.Get<Action<Type, Type>>(ContainerConstants.RegisterTransient)(serviceType, implimentationType);
        }

        public static void RegisterTransient(this IDictionary<string, object> environment, Type serviceType, Func<IDictionary<string, object>, object> getService)
        {
            environment.Get<Action<Type, Func<IDictionary<string, object>, object>>>(ContainerConstants.RegisterTransientFromFunc)(serviceType, getService);
        }

        public static void RegisterSingleton(this IDictionary<string, object> environment, Type serviceType, object instance)
        {
            environment.Get<Action<Type, object>>(ContainerConstants.RegisterSingleton)(serviceType, instance);
        }

        public static void RegisterSingleton(this IDictionary<string, object> environment, Type serviceType, Func<IDictionary<string, object>, object> getService)
        {
            environment.Get<Action<Type, Func<IDictionary<string, object>, object>>>(ContainerConstants.RegisterSingletonFromFunc)(serviceType, getService);
        }

        public static void RegisterSingletonType(this IDictionary<string, object> environment, Type serviceType, Type implimentationType)
        {
            environment.Get<Action<Type, Type>>(ContainerConstants.RegisterSingletonType)(serviceType, implimentationType);
        }

        public static void RegisterAllClosing(this IDictionary<string, object> environment, Type openServiceType)
        {
            environment.Get<Action<Type>>(ContainerConstants.RegisterAllClosing)(openServiceType);
        }

        public static void RegisterAll(this IDictionary<string, object> environment, Type serviceType)
        {
            environment.Get<Action<Type>>(ContainerConstants.RegisterAll)(serviceType);
        }
    }
}