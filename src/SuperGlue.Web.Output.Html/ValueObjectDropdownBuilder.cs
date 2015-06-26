using System.Linq;
using HtmlTags;
using HtmlTags.Conventions;
using HtmlTags.Conventions.Elements;
using HtmlTags.Reflection;

namespace SuperGlue.Web.Output.Html
{
    public class ValueObjectDropdownBuilder : IElementBuilder
    {
        public HtmlTag Build(ElementRequest request)
        {
            var attribute = request.Accessor.GetAttribute<ValueOfAttribute>();
            var list = request.Get<IFindListOf>().Find(attribute.ItemType).ToList();

            var defaultValue = request.Value<string>();

            if (string.IsNullOrEmpty(defaultValue))
                defaultValue = list.Select(x => x.Key).FirstOrDefault();

            return new SelectTag(tag =>
            {
                foreach (var selectListItem in list)
                    tag.Option(selectListItem.Text, selectListItem.Key);

                if (defaultValue != null)
                    tag.SelectByValue(defaultValue);

                if (!string.IsNullOrEmpty(attribute.FilteredBy))
                    tag.Data("filtered-by", attribute.FilteredBy);
            });
        }
    }
}