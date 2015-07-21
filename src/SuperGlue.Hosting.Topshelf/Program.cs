using Topshelf;

namespace SuperGlue.Hosting.Topshelf
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var settings = new ApplicationSettings();

            HostFactory.Run(x =>
            {
                x.AddCommandLineDefinition("environment", y => settings.Environment = y);

                x.Service<SuperGlueServiceRuntime>(s =>
                {
                    s.ConstructUsing(name => new SuperGlueServiceRuntime());
                    s.WhenStarted(r => r.Start(settings.Environment));
                    s.WhenStopped(r => r.Stop());
                    s.WhenPaused(r => r.Stop());
                    s.WhenContinued(r => r.Start(settings.Environment));
                    s.WhenShutdown(r => r.Stop());
                });
            });
        }

        public class ApplicationSettings
        {
            public ApplicationSettings()
            {
                Environment = "local";
            }

            public string Environment { get; set; }
        }
    }
}