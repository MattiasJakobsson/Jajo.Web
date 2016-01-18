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
                environment.RegisterTransient(typeof(IManageDiagnosticsInformation), typeof(ManageDiagnosticsInformationInMemory));

                environment.AlterSettings<DiagnosticsSettings>(x =>
                {
                    if (applicationEnvironment == "test" || applicationEnvironment == "local")
                        x.AllowAll();
                    else
                        x.DisllowAll();
                });

                environment[DiagnosticsExtensions.DiagnosticsConstants.AddData] = (Func<IDictionary<string, object>, string, string, string, Tuple<string, IDictionary<string, object>>, Task>)((x, y, z, a, b) =>
                {
                    if (!x.GetSettings<DiagnosticsSettings>().IsKeyAllowed(y))
                        return Task.CompletedTask;

                    var manager = environment.Resolve<IManageDiagnosticsInformation>();

                    return manager.AddDiagnostics(y, z, a, new DiagnosticsData(b.Item1, b.Item2.ToDictionary(c => c.Key, c => (c.Value as IDiagnosticsValue) ?? new ObjectDiagnosticsValue(c.Value))));
                });

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}