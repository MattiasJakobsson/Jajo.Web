using System.Collections.Generic;
using System.Threading.Tasks;
using HtmlTags.Conventions;
using HtmlTags.Conventions.Elements;
using SuperGlue.Configuration;

namespace SuperGlue.Web.Output.Html
{
    public class SetupHtmlConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.HtmlSetup", environment =>
            {
                environment.RegisterTransient(typeof(ITagGenerator), x => new TagGenerator(x.Resolve<ITagLibrary>(), new ActiveProfile(), x.Resolve));
                environment.RegisterTransient(typeof(ITagLibrary), typeof(TagLibrary));
                environment.RegisterTransient(typeof(IElementGenerator<>), typeof(ElementGenerator<>));
                environment.RegisterTransient(typeof(IElementNamingConvention), typeof(DefaultElementNamingConvention));
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