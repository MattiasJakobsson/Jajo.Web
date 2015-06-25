using System.Collections.Generic;
using HtmlTags;
using HtmlTags.Conventions;
using HtmlTags.Conventions.Elements;

namespace SuperGlue.Web.Output.Html
{
    public class FormBuilder : IElementBuilder
    {
        public HtmlTag Build(ElementRequest request)
        {
            var environment = request.Get<IDictionary<string, object>>();

            return new FormTag(environment.RouteTo(request.Model));
        }
    }
}