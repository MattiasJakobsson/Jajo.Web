using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.Localization
{
    public class SetupLocalizationConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.LocalizationSetup", environment =>
            {
                environment.RegisterTransient(typeof(ILocalizeText), typeof(DefaultTextLocalizer));
                environment.RegisterTransient(typeof(IFindCurrentLocalizationNamespace), typeof(DefaultLocalizationNamespaceFinder));

                environment.RegisterAll(typeof(ILocalizationVisitor));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}