using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.ParameterParsing
{
    public class SetupParameterParsingConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.ParameterParsingSetup", environment =>
            {
                environment.RegisterAll(typeof(IFindParameterValue));
                environment.RegisterAll(typeof(IParseExpressionPart));

                environment.RegisterTransient(typeof(IHandleParameters), typeof(DefaultParameterHandler));
                environment.RegisterTransient(typeof(IParseExpressionFor), typeof(DefaultExpressionParser));
            }, "superglue.ContainerSetup");
        }

        public Task Shutdown(IDictionary<string, object> applicationData)
        {
            return Task.Factory.StartNew(() => { });
        }

        public Task Configure(SettingsConfiguration configuration)
        {
            return Task.Factory.StartNew(() => { });
        }
    }
}