using System;

namespace SuperGlue
{
    public class RunCommand : ICommand
    {
        public string AppPath { get; set; }
        public string Environment { get; set; }

        public void Execute()
        {
            var application = new RemoteApplication(AppPath, Environment);

            application.Start();

            var key = Console.ReadKey();
            while (key.Key != ConsoleKey.Q)
            {
                if (key.Key != ConsoleKey.R) 
                    continue;

                Console.WriteLine();
                Console.WriteLine();

                application.Recycle();

                key = Console.ReadKey();
            }

            application.Stop();
        }
    }
}