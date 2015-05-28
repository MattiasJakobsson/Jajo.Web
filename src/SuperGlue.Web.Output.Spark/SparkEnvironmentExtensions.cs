using System.Collections.Generic;
using Spark;

namespace SuperGlue.Web.Output.Spark
{
    public static class SparkEnvironmentExtensions
    {
        public static class SparkConstants
        {
            public const string TemplateSource = "superglue.Spark.TemplateSource";
            public const string ViewEngine = "superglue.Spark.ViewEngine";
        }

        public static ITemplateSource GetTemplateSource(this IDictionary<string, object> environment)
        {
            return environment.Get<ITemplateSource>(SparkConstants.TemplateSource);
        }

        public static ISparkViewEngine GetViewEngine(this IDictionary<string, object> environment)
        {
            return environment.Get<ISparkViewEngine>(SparkConstants.ViewEngine);
        }
    }
}