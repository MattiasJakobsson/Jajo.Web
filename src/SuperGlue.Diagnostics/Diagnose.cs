using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SuperGlue.Diagnostics
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

            var key = _options.GetKey(environment);

            if (!string.IsNullOrEmpty(key))
            {
                var diagnosticsInformationManager = environment.Resolve<IManageDiagnosticsInformation>();
                diagnosticsInformationManager.AddMessurement(_options.MessurementKey, key, stopwatch.Elapsed);
            }
        }
    }

    public class DiagnoseOptions
    {
        public DiagnoseOptions(string messurementKey, Func<IDictionary<string, object>, string> getKey)
        {
            MessurementKey = messurementKey;
            GetKey = getKey;
        }

        public string MessurementKey { get; private set; }
        public Func<IDictionary<string, object>, string> GetKey { get; private set; }
    }
}