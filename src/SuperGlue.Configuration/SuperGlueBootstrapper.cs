using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SuperGlue.Configuration
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public abstract class SuperGlueBootstrapper
    {
        private readonly IDictionary<string, AppFunc> _chains = new ConcurrentDictionary<string, AppFunc>();
        private IEnumerable<IStartApplication> _appStarters;

        public virtual void StartApplications(IDictionary<string, object> settings, ApplicationStartersOverrides overrides = null)
        {
            overrides = overrides ?? ApplicationStartersOverrides.Empty();

            var subApplications = SubApplications.Init(settings).ToList();

            var assemblies = new List<Assembly>();

            assemblies.AddRange(subApplications.Select(x => x.Assembly));

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.StartsWith("SuperGlue")))
            {
                if (!assemblies.Contains(assembly))
                    assemblies.Add(assembly);
            }

            RunConfigurations(assemblies, settings);

            Configure(settings);

            _appStarters = settings.ResolveAll<IStartApplication>();

            settings["superglue.ApplicationStarters"] = _appStarters;

            foreach (var item in _appStarters.GroupBy(x => x.Chain))
            {
                if (!_chains.ContainsKey(item.Key))
                    continue;

                var chain = _chains[item.Key];

                item.OrderBy(x => overrides.GetSortOrder(x)).First().Start(chain, settings);
            }
        }

        public virtual void ShutDown()
        {
            var appStarters = _appStarters ?? new List<IStartApplication>();

            foreach (var startApplication in appStarters)
                startApplication.ShutDown();
        }

        protected abstract void Configure(IDictionary<string, object> environment);

        protected void AddChain(string name, Action<IBuildAppFunction> configure)
        {
            var appFuncBuilder = GetAppFunctionBuilder(name);

            configure(appFuncBuilder);

            _chains[name] = appFuncBuilder.Build();
        }

        protected virtual IBuildAppFunction GetAppFunctionBuilder(string chain)
        {
            return new BuildAppFunction();
        }

        protected virtual void RunConfigurations(IEnumerable<Assembly> assemblies, IDictionary<string, object> environment)
        {
            environment[ConfigurationsEnvironmentExtensions.ConfigurationConstants.Assemblies] = assemblies;

            var configurations = assemblies
                .SelectMany(x => x.GetTypes())
                .Where(x => typeof(ISetupConfigurations).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(Activator.CreateInstance)
                .OfType<ISetupConfigurations>()
                .SelectMany(x => x.Setup())
                .ToList();

            ExecuteConfigurationsDependingOn(new ReadOnlyCollection<ConfigurationSetupResult>(configurations), "superglue.ApplicationSetupStarted", environment);
        }

        protected virtual void ExecuteConfigurationsDependingOn(IReadOnlyCollection<ConfigurationSetupResult> configurations, string dependsOn, IDictionary<string, object> environment)
        {
            var configurationsToExecute = configurations.Where(x => x.DependsOn == dependsOn).ToList();

            var results = new List<string>();

            foreach (var configuration in configurationsToExecute)
            {
                configuration.Action(environment);
                results.Add(configuration.ConfigurationName);
            }

            foreach (var item in results.Distinct())
                ExecuteConfigurationsDependingOn(configurations, item, environment);
        }

        public static SuperGlueBootstrapper Find()
        {
            LoadAssemblies();

            var bootstrapper = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => typeof(SuperGlueBootstrapper).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(Activator.CreateInstance)
                .OfType<SuperGlueBootstrapper>()
                .FirstOrDefault();

            if (bootstrapper == null)
                throw new Exception("No bootstrapper");

            return bootstrapper;
        }

        private static void LoadAssemblies()
        {
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            var loadedPaths = loadedAssemblies.Where(x => !x.IsDynamic).Select(a => a.Location).ToArray();



            var paths = new List<string>
            {
                AppDomain.CurrentDomain.BaseDirectory
            };

            paths.AddRange((AppDomain.CurrentDomain.RelativeSearchPath ?? "").Split(';').Where(x => !string.IsNullOrEmpty(x)));

            var referencedPaths = paths
                .SelectMany(x => Directory.GetFiles(x, "*.dll"))
                .ToList();

            var toLoad = referencedPaths.Where(r => !loadedPaths.Contains(r, StringComparer.InvariantCultureIgnoreCase)).ToList();
            toLoad.ForEach(path => loadedAssemblies.Add(AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(path))));
        }
    }
}