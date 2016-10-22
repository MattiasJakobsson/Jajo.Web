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

            var requestedPath = _options.ChangePath(environment.GetRequest().Path);

            var path = environment.ResolvePath($"~/{requestedPath}");

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

                var output = new StaticFileOutput(path, _options.GetCacheControl(path));

                environment.SetRouteDestination(output, new List<Type>(), new Dictionary<string, object>());
                environment.SetOutput(output);
            }

            await _next(environment).ConfigureAwait(false);
        }
    }

    public class RouteStaticFilesOptions
    {
        public RouteStaticFilesOptions(IEnumerable<string> defaultFiles, Func<string, string> changePath = null, Func<string, string> getCacheControl = null)
        {
            DefaultFiles = defaultFiles;
            ChangePath = changePath ?? (x => x);
            GetCacheControl = getCacheControl ?? (x => null);
        }

        public IEnumerable<string> DefaultFiles { get; }
        public Func<string, string> ChangePath { get; }
        public Func<string, string> GetCacheControl { get; }
    }
}