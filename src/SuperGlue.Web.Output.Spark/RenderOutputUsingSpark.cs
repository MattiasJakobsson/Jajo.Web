using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Spark;
using Spark.Parser;
using Spark.Parser.Syntax;

namespace SuperGlue.Web.Output.Spark
{
    public class RenderOutputUsingSpark : IRenderOutput
    {
        private readonly ISparkViewEngine _engine;
        private readonly IEnumerable<Template> _availableTemplates;
        private readonly UseMasterGrammar _grammar;

        public RenderOutputUsingSpark(ISparkViewEngine engine, IEnumerable<Template> availableTemplates)
        {
            _engine = engine;
            _availableTemplates = availableTemplates;

            if (engine != null)
                _grammar = new UseMasterGrammar(engine.Settings.Prefix);
        }

        public async Task<OutputRenderingResult> Render(IDictionary<string, object> environment)
        {
            var output = environment.GetOutput();

            if (output == null)
                return null;

            var templates = _availableTemplates.ToList();

            var matchingTemplates = templates.Where(x => x.ModelType == output.GetType()).ToList();

            if (matchingTemplates.Count < 1)
                return new OutputRenderingResult("", ContentType.Html);

            if (matchingTemplates.Count > 1)
                throw new Exception(
                    string.Format(
                        "More then one template was found for endpoint.\nThe following templates were found: {0}",
                        string.Join(", ", matchingTemplates.Select(x => x.Name))));

            var template = matchingTemplates.Single();

            var descriptor = BuildDescriptor(template, true, null);

            var sparkViewEntry = _engine.CreateEntry(descriptor);

            var view = sparkViewEntry.CreateInstance() as SuperGlueSparkView;

            if (view == null)
                return new OutputRenderingResult("", ContentType.Html);

            var outputStream = new MemoryStream();
            var writer = new StreamWriter(outputStream);

            view.Render(new ViewContext(output, environment), writer);

            writer.Flush();
            outputStream.Position = 0;

            var content = await new StreamReader(outputStream).ReadToEndAsync();

            return new OutputRenderingResult(content, ContentType.Html);
        }

        private SparkViewDescriptor BuildDescriptor(Template template, bool searchForMaster, ICollection<string> searchedLocations)
        {
            var descriptor = new SparkViewDescriptor
            {
                TargetNamespace = GetNamespaceEncodedPathViewPath(GetFullPath(template))
            };

            descriptor.Templates.Add(GetFullPath(template).Replace("\\", "/"));

            if (searchForMaster && TrailingUseMasterName(descriptor) == null)
            {
                LocatePotentialTemplate(
                    PotentialDefaultMasterLocations(template),
                    descriptor.Templates,
                    null);
            }

            var trailingUseMaster = TrailingUseMasterName(descriptor);
            while (searchForMaster && !string.IsNullOrEmpty(trailingUseMaster))
            {
                if (!LocatePotentialTemplate(
                    PotentialMasterLocations(template, trailingUseMaster),
                    descriptor.Templates,
                    searchedLocations))
                {
                    return null;
                }
                trailingUseMaster = TrailingUseMasterName(descriptor);
            }

            return descriptor;
        }

        private bool LocatePotentialTemplate(
            IEnumerable<string> potentialTemplates,
            ICollection<string> descriptorTemplates,
            ICollection<string> searchedLocations)
        {
            var templatesList = potentialTemplates.ToList();

            var template = templatesList.FirstOrDefault(t => _engine.ViewFolder.HasView(t));
            if (template != null)
            {
                descriptorTemplates.Add(template);
                return true;
            }

            if (searchedLocations != null)
            {
                foreach (var potentialTemplate in templatesList)
                {
                    searchedLocations.Add(potentialTemplate);
                }
            }

            return false;
        }

        protected virtual IEnumerable<string> PotentialMasterLocations(Template template, string masterName)
        {
            return new[]
            {
                string.Format("Layouts{0}{1}.spark", template.PathSeperator, masterName),
                string.Format("Shared{0}{1}.spark", template.PathSeperator, masterName)
            };
        }

        protected virtual IEnumerable<string> PotentialDefaultMasterLocations(Template template)
        {
            return new[]
            {
                string.Format("Layouts{0}Application.spark", template.PathSeperator),
                string.Format("Shared{0}Application.spark", template.PathSeperator)
            };
        }

        public string TrailingUseMasterName(SparkViewDescriptor descriptor)
        {
            var lastTemplate = descriptor.Templates.Last();
            var sourceContext = AbstractSyntaxProvider.CreateSourceContext(lastTemplate, _engine.ViewFolder);

            if (sourceContext == null)
            {
                return null;
            }

            var result = _grammar.ParseUseMaster(new Position(sourceContext));

            return result == null ? null : result.Value;
        }


        private static string GetFullPath(Template viewTemplate)
        {
            return string.Format("{0}{1}", viewTemplate.Path, viewTemplate.Name);
        }

        private static string GetNamespaceEncodedPathViewPath(string viewPath)
        {
            return viewPath.Replace('\\', '_').Replace(':', '_');
        }

        private class UseMasterGrammar : CharGrammar
        {
            public UseMasterGrammar(string prefix)
            {
                var whiteSpace0 = Rep(Ch(char.IsWhiteSpace));
                var whiteSpace1 = Rep1(Ch(char.IsWhiteSpace));
                var startOfElement = !string.IsNullOrEmpty(prefix) ? Ch("<" + prefix + ":use") : Ch("<use");
                var startOfAttribute = Ch("master").And(whiteSpace0).And(Ch('=')).And(whiteSpace0);
                var attrValue = Ch('\'').And(Rep(ChNot('\''))).And(Ch('\''))
                    .Or(Ch('\"').And(Rep(ChNot('\"'))).And(Ch('\"')));

                var endOfElement = Ch("/>");

                var useMaster = startOfElement
                    .And(whiteSpace1)
                    .And(startOfAttribute)
                    .And(attrValue)
                    .And(whiteSpace0)
                    .And(endOfElement)
                    .Build(hit => new string(hit.Left.Left.Down.Left.Down.ToArray()));

                ParseUseMaster =
                    pos =>
                    {
                        for (var scan = pos; scan.PotentialLength() != 0; scan = scan.Advance(1))
                        {
                            var result = useMaster(scan);
                            if (result != null)
                                return result;
                        }
                        return null;
                    };
            }

            public ParseAction<string> ParseUseMaster { get; private set; }
        }
    }
}