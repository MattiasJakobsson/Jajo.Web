using System;
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
                x.AddCommandLineDefinition("exclude", y => settings.ExcludedChains = y);
                x.AddCommandLineDefinition("maxretries", y => settings.MaxStartRetries = y);
                x.AddCommandLineDefinition("retryinterval", y => settings.RetryInterval = y);

                x.Service<SuperGlueServiceRuntime>(s =>
                {
                    s.ConstructUsing(name => new SuperGlueServiceRuntime());
                    s.WhenStarted(r => r.Start(settings.Environment, settings.GetMaxStartRetrys(), settings.GetRetryInterval(), settings.GetExcludedChains()));
                    s.WhenStopped(r => r.Stop());
                    s.WhenPaused(r => r.Stop());
                    s.WhenContinued(r => r.Start(settings.Environment, settings.GetMaxStartRetrys(), settings.GetRetryInterval(), settings.GetExcludedChains()));
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
            public string ExcludedChains { get; set; }
            public string MaxStartRetries { get; set; }
            public string RetryInterval { get; set; }

            public string[] GetExcludedChains()
            {
                return (ExcludedChains ?? "").Split(';');
            }

            public int GetMaxStartRetrys()
            {
                int retries;
                return int.TryParse(MaxStartRetries ?? "", out retries) ? retries : 10;
            }

            public TimeSpan GetRetryInterval()
            {
                int seconds;
                return int.TryParse(RetryInterval ?? "", out seconds)
                    ? TimeSpan.FromSeconds(seconds)
                    : TimeSpan.FromSeconds(5);
            }
        }
    }
}