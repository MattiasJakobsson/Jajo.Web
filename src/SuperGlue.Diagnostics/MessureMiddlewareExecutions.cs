using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.Diagnostics
{
    public class MessureMiddlewareExecutions : IWrapMiddleware<object>
    {
        public Task<IEndThings> Begin(IDictionary<string, object> environment, Type middleWareType)
        {
            var key = DiagnosticsCategories.RequestsFor(environment);
            var requestId = environment.GetCurrentChain().RequestId;

            if (string.IsNullOrEmpty(requestId) || !environment.GetSettings<DiagnosticsSettings>().IsKeyAllowed(key))
                return Task.FromResult<IEndThings>(new FakeDisposable());

            var stopwatch = Stopwatch.StartNew();

            return Task.FromResult<IEndThings>(new Disposable(middleWareType, key, stopwatch, environment, requestId));
        }

        public class Disposable : IEndThings
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

            public Task End()
            {
                _stopwatch.Stop();

                return _environment.PushDiagnosticsData(_key, DiagnosticsTypes.RequestExecution, _requestId, new Tuple<string, IDictionary<string, object>>(
                    $"MiddleWare-{_middleWareType.Name}-Executed", new Dictionary<string, object>
                {
                    {"Middleware", _middleWareType},
                    {"ExecutionTime", _stopwatch.Elapsed}
                }));
            }
        }

        public class FakeDisposable : IEndThings
        {
            public Task End()
            {
                return Task.CompletedTask;
            }
        }
    }
}