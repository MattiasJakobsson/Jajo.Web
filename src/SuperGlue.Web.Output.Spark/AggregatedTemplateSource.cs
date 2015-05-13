using System.Collections.Generic;

namespace SuperGlue.Web.Output.Spark
{
    public class AggregatedTemplateSource : ITemplateSource
    {
        private readonly IEnumerable<ITemplateSource> _sources;

        public AggregatedTemplateSource(params ITemplateSource[] sources)
        {
            _sources = sources;
        }

        public IEnumerable<Template> FindTemplates()
        {
            var templates = new List<Template>();
            
            foreach (var source in _sources)
                templates.AddRange(source.FindTemplates());

            return templates;
        }
    }
}