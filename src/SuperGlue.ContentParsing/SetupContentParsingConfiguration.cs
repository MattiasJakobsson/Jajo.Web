using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.ContentParsing
{
    public class SetupContentParsingConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.ContentParsingSetup", environment =>
            {
                environment.RegisterAll(typeof(ITextParser));
                environment.RegisterAll(typeof(IParseModelExpression));
                environment.RegisterTransient(typeof(IFindParameterValueFromModel), typeof(DefaultParameterValueFinder));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}