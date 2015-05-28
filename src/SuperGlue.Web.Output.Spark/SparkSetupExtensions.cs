namespace SuperGlue.Web.Output.Spark
{
    public static class SparkSetupExtensions
    {
        public static OutputSetupExtensions.OutputConfiguration UseSpark(this OutputSetupExtensions.RendererConfigurer configurer)
        {
            var renderer = new RenderOutputUsingSpark(configurer.Environment.GetViewEngine(), configurer.Environment.GetTemplateSource().FindTemplates(), configurer.Environment);

            return configurer.UseRenderer(renderer);
        }
    }
}