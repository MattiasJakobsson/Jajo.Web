using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Spark;
using SuperGlue.Configuration;

namespace SuperGlue.Web.Output.Spark
{
    public class SetupSparkConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.SparkSetup", environment =>
            {
                var assemblies = environment.GetAssemblies().ToList();

                var templateSource = new AggregatedTemplateSource(new EmbeddedTemplateSource(assemblies), new FileSystemTemplateSource(assemblies, new FileScanner()));

                environment[SparkEnvironmentExtensions.SparkConstants.TemplateSource] = templateSource;

                var templates = templateSource.FindTemplates().ToList();

                var sparkViewEngine = new SparkViewEngine(new SparkSettings())
                {
                    DefaultPageBaseType = typeof(SuperGlueSparkView).FullName,
                    ViewFolder = new SuperGlueViewFolder(templates),
                    PartialProvider = new SuperGluePartialProvider()
                };

                environment[SparkEnvironmentExtensions.SparkConstants.ViewEngine] = sparkViewEngine;

                environment.AlterSettings<OutputSettings>(x => x.When(y => y.GetRequest().Headers.Accept.Contains("text/html")).UseRenderer(new RenderOutputUsingSpark(sparkViewEngine, templates)));

                return Task.CompletedTask;
            }, "superglue.OutputSetup");
        }
    }
}