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
        private readonly IDictionary<Type, object> _settings = new ConcurrentDictionary<Type, object>();
        private IEnumerable<IStartApplication> _appStarters;
        private IReadOnlyCollection<ISetupConfigurations> _setups;
        private IDictionary<string, object> _environment;

        public virtual async Task<IEnumerable<string>> StartApplications(IDictionary<string, object> settings, string environment, ApplicationStartersOverrides overrides = null)
        {
            _environment = settings;

            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            if (!basePath.EndsWith("\\"))
                basePath = string.Format("{0}\\", basePath);

            settings[ConfigurationsEnvironmentExtensions.ConfigurationConstants.ResolvePathFunc] = (Func<string, string>)(x => x.Replace("~", basePath));

            settings[ConfigurationsEnvironmentExtensions.ConfigurationConstants.AlterConfigSettings] = (Action<Type, Action<object>>)((settingsType, alter) => alter(GetSettings(settingsType)));

            overrides = overrides ?? ApplicationStartersOverrides.Empty();

            var subApplications = SubApplications.Init(basePath).ToList();

            settings[SubApplicationsEnvironmentExtensions.SubApplicationConstants.SubApplications] = subApplications;

            var assemblies = new List<Assembly>();

            assemblies.AddRange(subApplications.Select(x => x.Assembly));

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.StartsWith("SuperGlue")))
            {
                if (!assemblies.Contains(assembly))
                    assemblies.Add(assembly);
            }

            _setups = RunConfigurations(assemblies, environment);

            Configure(environment);

            foreach (var setup in _setups)
                await setup.Configure(new SettingsConfiguration(GetSettings, settings, environment));

            _appStarters = settings.ResolveAll<IStartApplication>();

            settings["superglue.ApplicationStarters"] = _appStarters;

            foreach (var item in _appStarters.GroupBy(x => x.Chain))
            {
                var starter = item.OrderBy(x => overrides.GetSortOrder(x)).First();

                AppFunc chain = null;

                if (_chains.ContainsKey(item.Key))
                    chain = _chains[item.Key];

                chain = chain ?? starter.GetDefaultChain(GetAppFunctionBuilder(item.Key), environment);

                if (chain != null)
                    await starter.Start(chain, settings, environment);
            }

            return subApplications.Select(x => x.Path).ToList();
        }

        public virtual async Task ShutDown()
        {
            var appStarters = _appStarters ?? new List<IStartApplication>();

            foreach (var startApplication in appStarters)
                await startApplication.ShutDown(_environment);

            var setups = _setups ?? new List<ISetupConfigurations>();

            foreach (var setup in setups)
                await setup.Shutdown(_environment);
        }

        protected abstract void Configure(string environment);

        protected virtual object GetSettings(Type settingsType)
        {
            if (_settings.ContainsKey(settingsType))
                return _settings[settingsType];

            var settings = _environment.Resolve(settingsType);

            _settings[settingsType] = settings;

            return settings;
        }

        protected void AddChain(string name, Action<IBuildAppFunction> configure)
        {
            var appFuncBuilder = GetAppFunctionBuilder(name);

            configure(appFuncBuilder);

            _chains[name] = appFuncBuilder.Build();
        }

        protected void AlterSettings<TSettings>(Action<TSettings> action)
        {
            _environment.AlterSettings(action);
        }

        protected virtual IBuildAppFunction GetAppFunctionBuilder(string chain)
        {
            return new BuildAppFunction(_environment, chain);
        }

        protected virtual IReadOnlyCollection<ISetupConfigurations> RunConfigurations(IEnumerable<Assembly> assemblies, string environment)
        {
            _environment[ConfigurationsEnvironmentExtensions.ConfigurationConstants.Assemblies] = assemblies;

            var setups = assemblies
                .SelectMany(x => x.GetTypes())
                .Where(x => typeof (ISetupConfigurations).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(Activator.CreateInstance)
                .OfType<ISetupConfigurations>()
                .ToList();

            var configurations = setups.SelectMany(x => x.Setup(environment)).Where(x => string.IsNullOrEmpty(x.Environment) || x.Environment.Equals(environment, StringComparison.OrdinalIgnoreCase)).ToList();

            var executed = ExecuteConfigurationsDependingOn(new ReadOnlyCollection<ConfigurationSetupResult>(configurations), "superglue.ApplicationSetupStarted").ToList();

            while (true)
            {
                var missing = configurations.Where(x => !executed.Contains(x.ConfigurationName)).ToList();

                if(!missing.Any())
                    break;

                var dependencyToExecute = missing.OrderBy(x => missing.Count(y => y.ConfigurationName == x.DependsOn)).Select(x => x.DependsOn).FirstOrDefault();

                var missingExecuted = ExecuteConfigurationsDependingOn(missing, dependencyToExecute).ToList();

                if (!missingExecuted.Any())
                {
                    foreach (var configuration in missing)
                        configuration.Action(_environment);

                    break;
                }

                executed.AddRange(missingExecuted);
            }

            return setups;
        }

        protected virtual IEnumerable<string> ExecuteConfigurationsDependingOn(IReadOnlyCollection<ConfigurationSetupResult> configurations, string dependsOn)
        {
            var configurationsToExecute = configurations.Where(x => x.DependsOn == dependsOn).ToList();

            var results = new List<string>();

            foreach (var configuration in configurationsToExecute)
            {
                configuration.Action(_environment);
                results.Add(configuration.ConfigurationName);
            }

            var executed = results.ToList();

            foreach (var item in results.Distinct())
                executed.AddRange(ExecuteConfigurationsDependingOn(configurations, item));

            return executed;
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