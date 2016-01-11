using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace SuperGlue
{
    public class RunCommand : ICommand
    {
        public string Application { get; set; }
        public string Environment { get; set; }

        public async Task Execute()
        {
            var application = new RunnableApplication(Environment, Application, Path.Combine(Assembly.GetExecutingAssembly().Location, $"Applications\\{Path.GetFileName(Application)}"));

            await application.Start();

            var key = Console.ReadKey();

            while (key.Key != ConsoleKey.Q)
            {
                if (key.Key != ConsoleKey.R)
                    continue;

                Console.WriteLine();

                await application.Recycle();

                key = Console.ReadKey();
            }

            await application.Stop();
        }
    }
}