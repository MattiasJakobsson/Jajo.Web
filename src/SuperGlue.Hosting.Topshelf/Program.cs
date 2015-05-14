using Topshelf;

namespace SuperGlue.Hosting.Topshelf
{
    public class Program
    {
        public static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                //x.SetServiceName(settings.Name);
                //x.SetDisplayName(settings.DisplayName);
                //x.SetDescription(settings.Description);

                x.RunAsLocalSystem();

                x.Service<SuperGlueServiceRuntime>(s =>
                {
                    s.ConstructUsing(name => new SuperGlueServiceRuntime());
                    s.WhenStarted(r => r.Start());
                    s.WhenStopped(r => r.Stop());
                    s.WhenPaused(r => r.Stop());
                    s.WhenContinued(r => r.Start());
                    s.WhenShutdown(r => r.Stop());
                });

                x.StartAutomatically();
            });
        }
    }
}