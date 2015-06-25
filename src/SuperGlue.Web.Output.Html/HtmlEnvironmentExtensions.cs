using System.Collections.Generic;

namespace SuperGlue.Web.Output.Html
{
    public static class HtmlEnvironmentExtensions
    {
        public static class HtmlConstants
        {
            public const string HtmlConventionsSettings = "superglue.HtmlConventionSettings";
        }

        public static HtmlConventionSettings GetHtmlConventionSettings(this IDictionary<string, object> environment)
        {
            return environment.Get(HtmlConstants.HtmlConventionsSettings, new HtmlConventionSettings());
        }
    }
}