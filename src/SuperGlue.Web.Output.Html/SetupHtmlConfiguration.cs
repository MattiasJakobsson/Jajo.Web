using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlTags.Conventions.Elements;
using HtmlTags.Conventions.Elements.Builders;
using SuperGlue.Configuration;

namespace SuperGlue.Web.Output.Html
{
    public class SetupHtmlConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.HtmlSetup", environment =>
            {
                environment.RegisterTransient(typeof(IElementGenerator<>), (x, y) => y.GetHtmlConventionSettings().ElementGeneratorFor(x.GenericTypeArguments.First()));
                environment.RegisterTransient(typeof(IElementNamingConvention), typeof(DefaultElementNamingConvention));

                environment.AlterSettings<HtmlConventionSettings>(x =>
                {
                    x.Editors.BuilderPolicy<CheckboxBuilder>();

                    x.Editors.Always.BuildBy<TextboxBuilder>();

                    x.Editors.Modifier<AddNameModifier>();

                    x.Displays.Always.BuildBy<SpanDisplayBuilder>();

                    x.Labels.Always.BuildBy<DefaultLabelBuilder>();

                    x.Forms.Always.BuildBy<FormBuilder>();
                });
            }, "superglue.ContainerSetup");
        }

        public Task Shutdown(IDictionary<string, object> applicationData)
        {
            return Task.Factory.StartNew(() => { });
        }

        public Task Configure(SettingsConfiguration configuration)
        {
            return Task.Factory.StartNew(() =>
            {
                configuration.Settings[HtmlEnvironmentExtensions.HtmlConstants.HtmlConventionsSettings] = configuration.WithSettings<HtmlConventionSettings>();
            });
        }
    }
}