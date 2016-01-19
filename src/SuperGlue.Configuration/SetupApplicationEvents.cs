using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SuperGlue.Configuration
{
    public class SetupApplicationEvents : ISetupConfigurations
    {
        private static readonly CancellationTokenSource ApplicationEventsCancellationTokenSource = new CancellationTokenSource();

        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.ApplicationEventsSetup", environment =>
            {
                environment.RegisterSingleton(typeof(IApplicationEvents), new DefaultApplicationEvents(ApplicationEventsCancellationTokenSource));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup", environment =>
            {
                ApplicationEventsCancellationTokenSource.Cancel();

                return Task.CompletedTask;
            });
        }
    }
}