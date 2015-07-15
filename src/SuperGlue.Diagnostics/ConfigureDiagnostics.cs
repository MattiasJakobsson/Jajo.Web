using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.Diagnostics
{
    public class ConfigureDiagnostics : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.Diagnostics.Configured", environment =>
            {
                environment.RegisterSingleton(typeof(IManageDiagnosticsInformation), new ManageDiagnosticsInformationInMemory());

                environment.AlterSettings<DiagnosticsSettings>(x =>
                {
                    if (applicationEnvironment == "test")
                        x.AllowAll();
                    else
                        x.DisllowAll();
                });

                environment[DiagnosticsExtensions.DiagnosticsConstants.AddData] = (Action<IDictionary<string, object>, string, Tuple<string, IDictionary<string, object>>>)((x, y, z) =>
                {
                    if (!x.GetSettings<DiagnosticsSettings>().IsKeyAllowed(y))
                        return;

                    var manager = x.Resolve<IManageDiagnosticsInformation>();

                    manager.AddMessurement(y, new DiagnosticsData(z.Item1, z.Item2.ToDictionary(a => a.Key, a => (a.Value as IDiagnosticsValue) ?? new ObjectDiagnosticsValue(a))));
                });

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}