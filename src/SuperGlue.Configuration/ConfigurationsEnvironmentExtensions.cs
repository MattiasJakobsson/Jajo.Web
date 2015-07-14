using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SuperGlue.Configuration
{
    public static class ConfigurationsEnvironmentExtensions
    {
        public static class ConfigurationConstants
        {
            public const string Assemblies = "superglue.Assemblies";
            public const string GetConfigSettings = "superglue.Configuration.GetConfigSettings";
            public const string ResolvePathFunc = "superglue.Configuration.ResolvePath";
            public const string ChainName = "superglue.Configuration.ChainName";
            public const string EventHandlers = "superglue.EventHandlers";
            public const string ApplicationName = "superglue.ApplicationName";
        }

        public static IEnumerable<Assembly> GetAssemblies(this IDictionary<string, object> environment)
        {
            return environment.Get<ICollection<Assembly>>(ConfigurationConstants.Assemblies);
        }

        public static void AddAssembly(this IDictionary<string, object> environment, Assembly assembly)
        {
            var assemblies = environment.Get<ICollection<Assembly>>(ConfigurationConstants.Assemblies);

            if (!assemblies.Contains(assembly))
                assemblies.Add(assembly);
        }

        public static void AlterSettings<TSettings>(this IDictionary<string, object> environment, Action<TSettings> alterer) where TSettings : class
        {
            var settings = GetSettings<TSettings>(environment);

            alterer(settings);
        }

        public static TSettings GetSettings<TSettings>(this IDictionary<string, object> environment) where TSettings : class
        {
            return environment.Get<Func<Type, object>>(ConfigurationConstants.GetConfigSettings)(typeof(TSettings)) as TSettings;
        }

        public static string ResolvePath(this IDictionary<string, object> environment, string relativePath)
        {
            return environment.Get<Func<string, string>>(ConfigurationConstants.ResolvePathFunc)(relativePath);
        }

        public static string GetCurrentChain(this IDictionary<string, object> environment)
        {
            return environment.Get<string>(ConfigurationConstants.ChainName);
        }

        public static string GetApplicationName(this IDictionary<string, object> environment)
        {
            return environment.Get(ConfigurationConstants.ApplicationName, "");
        }

        public static Guid SubscribeTo(this IDictionary<string, object> environment, string eventName, Func<IDictionary<string, object>, Task> handler)
        {
            var handlers = environment.Get<IDictionary<Guid, Subscription>>(ConfigurationConstants.EventHandlers);

            if (handlers == null)
            {
                handlers = new ConcurrentDictionary<Guid, Subscription>();
                environment[ConfigurationConstants.EventHandlers] = handlers;
            }

            var id = Guid.NewGuid();

            handlers[id] = new Subscription(eventName, handler);

            return id;
        }

        public static void RemoveSubscription(this IDictionary<string, object> environment, Guid id)
        {
            var handlers = environment.Get<IDictionary<Guid, Subscription>>(ConfigurationConstants.EventHandlers, new ConcurrentDictionary<Guid, Subscription>());

            if (handlers.ContainsKey(id))
                handlers.Remove(id);
        }

        public static async Task Publish(this IDictionary<string, object> environment, string eventName)
        {
            var subsciptions = environment
                .Get<IDictionary<Guid, Subscription>>(ConfigurationConstants.EventHandlers, new ConcurrentDictionary<Guid, Subscription>())
                .Where(x => x.Value.Event == eventName)
                .Select(x => x.Value)
                .ToList();

            foreach (var subscription in subsciptions)
                await subscription.Handler(environment);
        }

        private class Subscription
        {
            public Subscription(string evnt, Func<IDictionary<string, object>, Task> handler)
            {
                Event = evnt;
                Handler = handler;
            }

            public string Event { get; private set; }
            public Func<IDictionary<string, object>, Task> Handler { get; private set; }
        }
    }
}