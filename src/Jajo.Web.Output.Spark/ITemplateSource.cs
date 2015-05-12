using System.Collections.Generic;

namespace Jajo.Web.Output.Spark
{
    public interface ITemplateSource
    {
        IEnumerable<Template> FindTemplates();
    }
}