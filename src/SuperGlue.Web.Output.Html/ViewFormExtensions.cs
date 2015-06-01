using System.Web;
using HtmlTags;

namespace SuperGlue.Web.Output.Html
{
    public static class ViewFormExtensions
    {
        public static HtmlTag FormFor(this ISuperGlueView view)
        {
            return new FormTag().NoClosingTag();
        }

        public static HtmlTag FormFor(this ISuperGlueView view, string url)
        {
            return new FormTag(url).NoClosingTag();
        }

        public static HtmlTag FormFor<TInputModel>(this ISuperGlueView view) where TInputModel : new()
        {
            return FormFor(view, new TInputModel());
        }

        public static HtmlTag FormFor<TInputModel>(this ISuperGlueView view, TInputModel model)
        {
            return FormFor(view, (object)model);
        }

        public static HtmlTag FormFor(this ISuperGlueView view, object input)
        {
            var stringInput = input as string;
            if (stringInput != null)
                return FormFor(view, stringInput);

            var url = view.Environment.RouteTo(input);

            return FormFor(view, url);
        }

        public static IHtmlString EndForm(this ISuperGlueView view)
        {
            return new HtmlString("</form>");
        }
    }
}