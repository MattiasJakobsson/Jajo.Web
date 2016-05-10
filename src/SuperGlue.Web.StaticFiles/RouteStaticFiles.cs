using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.FileSystem;

namespace SuperGlue.Web.StaticFiles
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class RouteStaticFiles
    {
        private readonly AppFunc _next;
        private readonly RouteStaticFilesOptions _options;

        public RouteStaticFiles(AppFunc next, RouteStaticFilesOptions options)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            _next = next;
            _options = options;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            if (environment.GetRouteInformation().RoutedTo != null)
            {
                await _next(environment).ConfigureAwait(false);

                return;
            }

            var fileSystem = environment.Resolve<IFileSystem>();

            var path = environment.ResolvePath($"~/{Path.Combine(_options.BaseDirectory, environment.GetRequest().Path)}");

            if (!fileSystem.FileExists(path))
            {
                foreach (var defaultFile in _options.DefaultFiles)
                {
                    var currentPath = Path.Combine(path, defaultFile);

                    if (!fileSystem.FileExists(currentPath))
                        continue;

                    path = currentPath;
                    break;
                }
            }

            if (fileSystem.FileExists(path))
            {
                await environment.PushDiagnosticsData(DiagnosticsCategories.RequestsFor(environment), DiagnosticsTypes.RequestExecution, environment.GetCurrentChain().RequestId,
                    new Tuple<string, IDictionary<string, object>>("RequestRouted", new Dictionary<string, object>
                    {
                        {"RoutedTo", path ?? ""},
                        {"Url", environment.GetRequest().Uri},
                        {"Found", true}
                    })).ConfigureAwait(false);

                environment.SetRouteDestination(new StaticFileOutput(path), new List<Type>(), new Dictionary<string, object>());
                environment.SetOutput(new StaticFileOutput(path));
            }

            await _next(environment).ConfigureAwait(false);
        }
    }

    public class RouteStaticFilesOptions
    {
        public RouteStaticFilesOptions(IEnumerable<string> defaultFiles, string baseDirectory = "")
        {
            DefaultFiles = defaultFiles;
            BaseDirectory = baseDirectory;
        }

        public IEnumerable<string> DefaultFiles { get; }
        public string BaseDirectory { get; }
    }
}