using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SuperGlue
{
    public class RunCommand : ICommand
    {
        public string Application { get; set; }
        public string Environment { get; set; }

        public async Task Execute()
        {
            var application = new RunnableApplication(Environment, Application, Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", $"Applications\\{HashUsingSha1(Application)}"));

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

        private static string HashUsingSha1(string input)
        {
            var hasher = new SHA1Managed();
            var passwordBytes = Encoding.ASCII.GetBytes(input);
            var passwordHash = hasher.ComputeHash(passwordBytes);
            return Convert.ToBase64String(passwordHash);
        }
    }
}