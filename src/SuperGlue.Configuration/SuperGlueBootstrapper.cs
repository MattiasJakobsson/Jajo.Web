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
        private IReadOnlyCollection<ConfigurationSetupResult> _setups;
        private IDictionary<string, object> _environment;
        private string _applicationEnvironment;

        public virtual async Task StartApplications(IDictionary<string, object> settings, string environment, ApplicationStartersOverrides overrides = null)
        {
            _environment = settings;
            _applicationEnvironment = environment;

            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            if (!basePath.EndsWith("\\"))
                basePath = string.Format("{0}\\", basePath);

            settings[ConfigurationsEnvironmentExtensions.ConfigurationConstants.ResolvePathFunc] = (Func<string, string>)(x => x.Replace("~", basePath));

            settings[ConfigurationsEnvironmentExtensions.ConfigurationConstants.GetConfigSettings] = (Func<Type, object>)GetSettings;

            settings[ConfigurationsEnvironmentExtensions.ConfigurationConstants.ApplicationName] = ApplicationName;

            overrides = overrides ?? ApplicationStartersOverrides.Empty();

            var assemblies = LoadApplicationAssemblies().ToList();

            _setups = FindConfigurationSetups(assemblies, environment);

            await RunConfigurations(_setups, environment, x => x.StartupAction(_environment));

            await Configure(environment);

            var settingsConfiguration = new SettingsConfiguration(GetSettings, settings, environment);

            await RunConfigurations(_setups, environment, x => x.ConfigureAction(settingsConfiguration));

            await settings.Publish(ConfigurationEvents.BeforeApplicationStart);

            _appStarters = settings.ResolveAll<IStartApplication>();

            settings["superglue.ApplicationStarters"] = _appStarters;

            var startTasks = new ConcurrentBag<Task>();

            Parallel.ForEach(_appStarters.GroupBy(x => x.Chain), item =>
            {
                var starter = item.OrderBy(x => overrides.GetSortOrder(x)).First();

                AppFunc chain = null;

                if (_chains.ContainsKey(item.Key))
                    chain = _chains[item.Key];

                chain = chain ?? starter.GetDefaultChain(GetAppFunctionBuilder(item.Key), environment);

                if (chain != null)
                    startTasks.Add(starter.Start(chain, settings, environment));
            });

            await Task.WhenAll(startTasks);

            await settings.Publish(ConfigurationEvents.AfterApplicationStart);
        }

        public virtual async Task ShutDown()
        {
            var appStarters = _appStarters ?? new List<IStartApplication>();

            await _environment.Publish(ConfigurationEvents.BeforeApplicationShutDown);

            var actions = new ConcurrentBag<Task>();

            Parallel.ForEach(appStarters, x =>
            {
                actions.Add(x.ShutDown(_environment));
            });

            await Task.WhenAll(actions);

            await _environment.Publish(ConfigurationEvents.AfterApplicationShutDown);

            await RunConfigurations(_setups ?? new ReadOnlyCollection<ConfigurationSetupResult>(new Collection<ConfigurationSetupResult>()), _applicationEnvironment, x => x.ShutdownAction(_environment));
        }

        protected abstract Task Configure(string environment);

        protected abstract string ApplicationName { get; }

        protected virtual object GetSettings(Type settingsType)
        {
            if (_settings.ContainsKey(settingsType))
                return _settings[settingsType];

            var settings = _environment.Resolve(settingsType);

            _settings[settingsType] = settings;

            return settings;
        }

        protected Task AddChain(string name, Action<IBuildAppFunction> configure)
        {
            var appFuncBuilder = GetAppFunctionBuilder(name);

            configure(appFuncBuilder);

            _chains[name] = appFuncBuilder.Build();

            return Task.CompletedTask;
        }

        protected void AlterSettings<TSettings>(Action<TSettings> action) where TSettings : class
        {
            _environment.AlterSettings(action);
        }

        protected virtual IBuildAppFunction GetAppFunctionBuilder(string chain)
        {
            return new BuildAppFunction(_environment, chain);
        }

        protected virtual IReadOnlyCollection<ConfigurationSetupResult> FindConfigurationSetups(IEnumerable<Assembly> assemblies, string environment)
        {
            _environment[ConfigurationsEnvironmentExtensions.ConfigurationConstants.Assemblies] = assemblies;

            var setups = assemblies
                .SelectMany(x => x.GetTypes())
                .Where(x => typeof (ISetupConfigurations).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(Activator.CreateInstance)
                .OfType<ISetupConfigurations>()
                .ToList();

            return setups.SelectMany(x => x.Setup(environment)).ToList();
        }

        protected virtual async Task RunConfigurations(IReadOnlyCollection<ConfigurationSetupResult> configurations, string environment, Func<ConfigurationSetupResult, Task> executionAction)
        {
            var executed = (await ExecuteConfigurationsDependingOn(configurations, "superglue.ApplicationSetupStarted", executionAction)).ToList();

            while (true)
            {
                var missing = configurations.Where(x => !executed.Contains(x)).ToList();

                if (!missing.Any())
                    break;

                var dependencyToExecute = missing.OrderBy(x => missing.Count(y => y.ConfigurationName == x.DependsOn)).Select(x => x.DependsOn).FirstOrDefault();

                var missingExecuted = (await ExecuteConfigurationsDependingOn(missing, dependencyToExecute, executionAction)).ToList();

                if (!missingExecuted.Any())
                {
                    var actions = new ConcurrentBag<Task>();

                    Parallel.ForEach(missing, x =>
                    {
                        actions.Add(executionAction(x));
                    });

                    await Task.WhenAll(actions);

                    break;
                }

                executed.AddRange(missingExecuted);
            }
        }

        protected virtual IEnumerable<Assembly> LoadApplicationAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(x =>
                        !x.GlobalAssemblyCache && !x.IsDynamic && !x.FullName.StartsWith("System") &&
                        !x.FullName.StartsWith("Microsoft"));
        }

        protected virtual async Task<IEnumerable<ConfigurationSetupResult>> ExecuteConfigurationsDependingOn(IReadOnlyCollection<ConfigurationSetupResult> configurations, string dependsOn, Func<ConfigurationSetupResult, Task> executionAction)
        {
            var configurationsToExecute = configurations.Where(x => x.DependsOn == dependsOn).ToList();

            var results = new ConcurrentBag<ConfigurationSetupResult>();

            var actions = new ConcurrentBag<Task>();

            Parallel.ForEach(configurationsToExecute, configuration =>
            {
                actions.Add(executionAction(configuration));
                results.Add(configuration);
            });

            await Task.WhenAll(actions);

            var executed = results.ToList();

            foreach (var item in results.Distinct())
                executed.AddRange(await ExecuteConfigurationsDependingOn(configurations, item.ConfigurationName, executionAction));

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