using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.Diagnostics
{
    public class MessureMiddlewareExecutions : IWrapMiddleware<object>
    {
        public Task<IDisposable> Begin(IDictionary<string, object> environment, Type middleWareType)
        {
            var key = DiagnosticTypes.MiddleWareExecutionFor(environment);
            var requestId = environment.GetCurrentChain().RequestId;

            if (string.IsNullOrEmpty(requestId) || !environment.GetSettings<DiagnosticsSettings>().IsKeyAllowed(key))
                return Task.FromResult<IDisposable>(new FakeDisposable());

            var stopwatch = Stopwatch.StartNew();

            return Task.FromResult<IDisposable>(new Disposable(middleWareType, key, stopwatch, environment, requestId));
        }

        public class Disposable : IDisposable
        {
            private readonly string _key;
            private readonly Type _middleWareType;
            private readonly Stopwatch _stopwatch;
            private readonly IDictionary<string, object> _environment;
            private readonly string _requestId;

            public Disposable(Type middleWareType, string key, Stopwatch stopwatch, IDictionary<string, object> environment, string requestId)
            {
                _middleWareType = middleWareType;
                _key = key;
                _stopwatch = stopwatch;
                _environment = environment;
                _requestId = requestId;
            }

            public void Dispose()
            {
                _stopwatch.Stop();

                _environment.PushDiagnosticsData(_key, new Tuple<string, IDictionary<string, object>>(_requestId, new Dictionary<string, object>
                {
                    {"Middleware", _middleWareType},
                    {"ExecutionTime", _stopwatch.Elapsed}
                }));
            }
        }

        public class FakeDisposable : IDisposable
        {
            public void Dispose()
            {

            }
        }
    }
}