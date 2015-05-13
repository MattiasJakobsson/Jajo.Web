using System.Collections.Generic;

namespace SuperGlue.Web.Output.Spark
{
    public interface ITemplateSource
    {
        IEnumerable<Template> FindTemplates();
    }
}