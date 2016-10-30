using System;
using System.Collections.Generic;
using System.Linq;
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

            var fileReaders = _options.FileReaders.ToList();

            if(!fileReaders.Any())
                fileReaders.Add(new ReadFilesFromFileSystem(fileSystem, environment, _options.DefaultFiles));

            var matchingReader = fileReaders.Select(x => x.TryRead(environment.GetRequest().Path)).FirstOrDefault(x => x.Exists);

            if (matchingReader != null)
            {
                await environment.PushDiagnosticsData(DiagnosticsCategories.RequestsFor(environment), DiagnosticsTypes.RequestExecution, environment.GetCurrentChain().RequestId,
                    new Tuple<string, IDictionary<string, object>>("RequestRouted", new Dictionary<string, object>
                    {
                        {"RoutedTo", matchingReader.Name ?? ""},
                        {"Url", environment.GetRequest().Uri},
                        {"Found", true}
                    })).ConfigureAwait(false);

                var output = new StaticFileOutput(matchingReader.Read, _options.GetCacheControl(matchingReader.Name));

                environment.SetRouteDestination(output, new List<Type>(), new Dictionary<string, object>());
                environment.SetOutput(output);
            }

            await _next(environment).ConfigureAwait(false);
        }
    }

    public class RouteStaticFilesOptions
    {
        public RouteStaticFilesOptions(IEnumerable<string> defaultFiles, IEnumerable<IReadFiles> fileReaders = null, Func<string, string> getCacheControl = null)
        {
            DefaultFiles = defaultFiles;
            FileReaders = fileReaders ?? new List<IReadFiles>();
            GetCacheControl = getCacheControl ?? (x => null);
        }

        public IEnumerable<string> DefaultFiles { get; }
        public IEnumerable<IReadFiles> FileReaders { get; }
        public Func<string, string> GetCacheControl { get; }
    }
}