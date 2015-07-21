using Fclp;
using Topshelf;

namespace SuperGlue.Hosting.Topshelf
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var settings = new ApplicationSettings();

            var parser = new FluentCommandLineParser();

            parser
                .Setup<string>('e', "environment")
                .Callback(x => settings.Environment = x)
                .SetDefault("test");

            parser
                .Setup<string>('s', "servicename")
                .Callback(x => settings.ServiceName = x)
                .SetDefault("SuperGlue.Topshelf");

            parser
                .Setup<string>('n', "displayname")
                .Callback(x => settings.DisplayName = x)
                .SetDefault("SuperGlue.Topshelf");

            parser
                .Setup<string>('d', "description")
                .Callback(x => settings.Description = x)
                .SetDefault("");

            HostFactory.Run(x =>
            {
                x.SetServiceName(settings.ServiceName);
                x.SetDisplayName(settings.DisplayName);
                x.SetDescription(settings.Description);

                x.RunAsLocalSystem();

                x.Service<SuperGlueServiceRuntime>(s =>
                {
                    s.ConstructUsing(name => new SuperGlueServiceRuntime());
                    s.WhenStarted(r => r.Start(settings.Environment));
                    s.WhenStopped(r => r.Stop());
                    s.WhenPaused(r => r.Stop());
                    s.WhenContinued(r => r.Start(settings.Environment));
                    s.WhenShutdown(r => r.Stop());
                });

                x.StartAutomatically();
            });
        }

        public class ApplicationSettings
        {
            public string Environment { get; set; }
            public string ServiceName { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
        }
    }
}