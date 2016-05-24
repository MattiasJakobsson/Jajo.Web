using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SuperGlue.Configuration
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public abstract class SuperGlueBootstrapper
    {
        protected readonly IDictionary<string, AppFunc> Chains = new ConcurrentDictionary<string, AppFunc>();
        private readonly IDictionary<Type, object> _settings = new ConcurrentDictionary<Type, object>();

        private readonly IDictionary<string, ChainSettings> _chainSettings =
            new ConcurrentDictionary<string, ChainSettings>();

        private IEnumerable<IStartApplication> _appStarters;
        private IReadOnlyCollection<ConfigurationSetupResult> _setups;
        protected IDictionary<string, object> Environment;
        protected string ApplicationEnvironment;

        public virtual Task StartApplications(IDictionary<string, object> settings, string environment)
        {
            return Start(settings, environment);
        }

        public virtual async Task ShutDown()
        {
            await Environment.Publish(ConfigurationEvents.BeforeApplicationShutDown).ConfigureAwait(false);

            await RunShutdowns(_appStarters ?? new List<IStartApplication>()).ConfigureAwait(false);

            await Environment.Publish(ConfigurationEvents.AfterApplicationShutDown).ConfigureAwait(false);

            await
                RunConfigurations(
                    _setups ??
                    new ReadOnlyCollection<ConfigurationSetupResult>(new Collection<ConfigurationSetupResult>()),
                    ApplicationEnvironment, x => x.ShutdownAction(Environment)).ConfigureAwait(false);
        }

        public virtual string ApplicationName => GetType().Assembly.GetName().Name.Replace(".", "");

        protected abstract Task Configure(string environment);

        protected virtual async Task Start(IDictionary<string, object> settings, string environment)
        {
            var stopwatch = Stopwatch.StartNew();

            Environment = settings;
            ApplicationEnvironment = environment;

            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            if (!basePath.EndsWith("\\"))
                basePath = $"{basePath}\\";

            settings[ConfigurationsEnvironmentExtensions.ConfigurationConstants.ResolvePathFunc] =
                (Func<string, string>)(x => x.Replace("~", basePath));

            settings[ConfigurationsEnvironmentExtensions.ConfigurationConstants.GetConfigSettings] =
                (Func<Type, object>)GetSettings;

            settings[ConfigurationsEnvironmentExtensions.ConfigurationConstants.GetChainSettingsKey] =
                (Func<string, ChainSettings>)GetChainSettings;

            settings[ConfigurationsEnvironmentExtensions.ConfigurationConstants.GetChain] =
                (Func<string, Action<IBuildAppFunction>, Task<AppFunc>>)GetChain;

            settings[ConfigurationsEnvironmentExtensions.ConfigurationConstants.ApplicationName] = ApplicationName;

            settings[ConfigurationsEnvironmentExtensions.ConfigurationConstants.GetTagsKey] =
                (Func<IEnumerable<string>>)GetApplicationTags;

            var assemblies = LoadApplicationAssemblies().ToList();

            _setups = FindConfigurationSetups(assemblies, environment);

            await RunConfigurations(_setups, environment, x => x.StartupAction(Environment)).ConfigureAwait(false);

            var chainDefiners = settings.ResolveAll<IDefineChain>();

            foreach (var chainDefiner in chainDefiners)
                await AddChain(chainDefiner.Name, chainDefiner.Define, chainDefiner.AlterSettings).ConfigureAwait(false);

            await Configure(environment).ConfigureAwait(false);

            var settingsConfiguration = new SettingsConfiguration(GetSettings, settings, environment);

            await
                RunConfigurations(_setups, environment, x => x.ConfigureAction(settingsConfiguration))
                    .ConfigureAwait(false);

            _appStarters = settings
                .ResolveAll<IStartApplication>()
                .ToList();

            await BeforeApplicationStart(settings, _appStarters).ConfigureAwait(false);

            settings["superglue.ApplicationStarters"] = _appStarters;

            await RunStarters(_appStarters).ConfigureAwait(false);

            await settings.Publish(ConfigurationEvents.AfterApplicationStart).ConfigureAwait(false);

            stopwatch.Stop();

            Environment.Log("Bootstrapping done in {0} ms", LogLevel.Debug, stopwatch.Elapsed.TotalMilliseconds);

            await
                Environment.PushDiagnosticsData(DiagnosticsCategories.Setup, DiagnosticsTypes.Bootstrapping, "Init",
                    new Tuple<string, IDictionary<string, object>>("ApplicationsStarted", new Dictionary<string, object>
                    {
                        {"ExecutionTime", stopwatch.Elapsed},
                        {"Environment", environment},
                        {"ApplicationName", ApplicationName}
                    })).ConfigureAwait(false);
        }

        protected virtual Task BeforeApplicationStart(IDictionary<string, object> settings,
            IEnumerable<IStartApplication> appStarters)
        {
            return settings.Publish(ConfigurationEvents.BeforeApplicationStart);
        }

        protected virtual Task RunStarters(IEnumerable<IStartApplication> appStarters)
        {
            var startTasks = new ConcurrentBag<Task>();

            Parallel.ForEach(appStarters, starter =>
            {
                AppFunc chain = null;

                if (Chains.ContainsKey(starter.Chain))
                    chain = Chains[starter.Chain];

                chain = chain ??
                        starter.GetDefaultChain(GetAppFunctionBuilder(starter.Chain), Environment,
                            ApplicationEnvironment);

                if (chain != null)
                    startTasks.Add(starter.Start(chain, Environment, ApplicationEnvironment));
            });

            return Task.WhenAll(startTasks);
        }

        protected virtual Task RunShutdowns(IEnumerable<IStartApplication> appStarters)
        {
            var actions = new ConcurrentBag<Task>();

            Parallel.ForEach(appStarters, x => { actions.Add(x.ShutDown(Environment)); });

            return Task.WhenAll(actions);
        }

        protected virtual object GetSettings(Type settingsType)
        {
            if (_settings.ContainsKey(settingsType))
                return _settings[settingsType];

            var settings = Activator.CreateInstance(settingsType);

            _settings[settingsType] = settings;

            return settings;
        }

        protected virtual ChainSettings GetChainSettings(string chain)
        {
            if (_chainSettings.ContainsKey(chain))
                return _chainSettings[chain];

            var settings = new ChainSettings();

            _chainSettings[chain] = settings;

            return settings;
        }

        protected virtual IEnumerable<string> GetTags()
        {
            return Enumerable.Empty<string>();
        }

        protected virtual IEnumerable<string> GetApplicationTags()
        {
            if (_setups == null)
                return GetTags();

            return _setups.SelectMany(x => x.ApplicationTags).Union(GetTags()).Distinct();
        }

        protected virtual async Task<AppFunc> GetChain(string name, Action<IBuildAppFunction> buildDefault)
        {
            if (!Chains.ContainsKey(name))
                await AddChain(name, buildDefault).ConfigureAwait(false);

            return Chains[name];
        }

        protected Task AddChain(string name, Action<IBuildAppFunction> configure,
            Action<ChainSettings> alterSettings = null)
        {
            var appFuncBuilder = GetAppFunctionBuilder(name);

            configure(appFuncBuilder);

            Chains[name] = appFuncBuilder.Build();

            alterSettings?.Invoke(GetChainSettings(name));

            return Task.CompletedTask;
        }

        protected void AlterSettings<TSettings>(Action<TSettings> action) where TSettings : class
        {
            Environment.AlterSettings(action);
        }

        protected virtual IBuildAppFunction GetAppFunctionBuilder(string chain)
        {
            return new BuildAppFunction(Environment, chain);
        }

        protected virtual IReadOnlyCollection<ConfigurationSetupResult> FindConfigurationSetups(
            IEnumerable<Assembly> assemblies, string environment)
        {
            Environment[ConfigurationsEnvironmentExtensions.ConfigurationConstants.Assemblies] = assemblies;

            var setups = assemblies
                .SelectMany(x => x.GetTypes())
                .Where(x => typeof(ISetupConfigurations).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(Activator.CreateInstance)
                .OfType<ISetupConfigurations>()
                .ToList();

            return setups.SelectMany(x => x.Setup(environment)).ToList();
        }

        protected virtual async Task RunConfigurations(IReadOnlyCollection<ConfigurationSetupResult> configurations,
            string environment, Func<ConfigurationSetupResult, Task> executionAction)
        {
            var executed =
                (await
                    ExecuteConfigurationsDependingOn(configurations, "superglue.ApplicationSetupStarted",
                        executionAction).ConfigureAwait(false)).ToList();

            while (true)
            {
                var missing = configurations.Where(x => !executed.Contains(x)).ToList();

                if (!missing.Any())
                    break;

                var dependencyToExecute =
                    missing.OrderBy(x => missing.Count(y => y.ConfigurationName == x.DependsOn))
                        .Select(x => x.DependsOn)
                        .FirstOrDefault();

                var missingExecuted =
                    (await
                        ExecuteConfigurationsDependingOn(missing, dependencyToExecute, executionAction)
                            .ConfigureAwait(false)).ToList();

                if (!missingExecuted.Any())
                {
                    var actions = new ConcurrentBag<Task>();

                    Parallel.ForEach(missing, x => { actions.Add(executionAction(x)); });

                    await Task.WhenAll(actions).ConfigureAwait(false);

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

        protected virtual async Task<IEnumerable<ConfigurationSetupResult>> ExecuteConfigurationsDependingOn(
            IReadOnlyCollection<ConfigurationSetupResult> configurations, string dependsOn,
            Func<ConfigurationSetupResult, Task> executionAction)
        {
            var configurationsToExecute = configurations.Where(x => x.DependsOn == dependsOn).ToList();

            var results = new ConcurrentBag<ConfigurationSetupResult>();

            var actions = new ConcurrentBag<Task>();

            Parallel.ForEach(configurationsToExecute, configuration =>
            {
                actions.Add(executionAction(configuration));
                results.Add(configuration);
            });

            await Task.WhenAll(actions).ConfigureAwait(false);

            var executed = results.ToList();

            foreach (var item in results.Distinct())
                executed.AddRange(
                    await
                        ExecuteConfigurationsDependingOn(configurations, item.ConfigurationName, executionAction)
                            .ConfigureAwait(false));

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

            paths.AddRange(
                (AppDomain.CurrentDomain.RelativeSearchPath ?? "").Split(';').Where(x => !string.IsNullOrEmpty(x)));

            var referencedPaths = paths
                .SelectMany(x => Directory.GetFiles(x, "*.dll"))
                .ToList();

            var toLoad =
                referencedPaths.Where(r => !loadedPaths.Contains(r, StringComparer.InvariantCultureIgnoreCase)).ToList();
            toLoad.ForEach(
                path => loadedAssemblies.Add(AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(path))));
        }
    }
}