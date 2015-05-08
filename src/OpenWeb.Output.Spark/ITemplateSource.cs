using System.Collections.Generic;

namespace OpenWeb.Output.Spark
{
    public interface ITemplateSource
    {
        IEnumerable<Template> FindTemplates();
    }
}