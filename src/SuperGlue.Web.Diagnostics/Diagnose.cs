using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SuperGlue.Web.Diagnostics
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class Diagnose
    {
        private readonly AppFunc _next;
        private readonly DiagnoseOptions _options;

        public Diagnose(AppFunc next, DiagnoseOptions options)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            if (options == null)
                throw new ArgumentNullException("options");

            _next = next;
            _options = options;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            await _next(environment);

            stopwatch.Stop();

            var routedTo = environment.GetRouteInformation().RoutedTo;

            if (routedTo != null)
                _options.ManageDiagnosticsInformation.RouteExecuted(routedTo, stopwatch.Elapsed);
        
            _options.ManageDiagnosticsInformation.UrlVisited(environment.GetUri(), stopwatch.Elapsed);
        }
    }

    public class DiagnoseOptions
    {
        public DiagnoseOptions(IManageDiagnosticsInformation manageDiagnosticsInformation)
        {
            ManageDiagnosticsInformation = manageDiagnosticsInformation;
        }

        public IManageDiagnosticsInformation ManageDiagnosticsInformation { get; private set; }
    }
}