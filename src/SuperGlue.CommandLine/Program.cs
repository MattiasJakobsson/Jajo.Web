using System;
using System.Collections.Generic;
using System.Linq;
using Fclp;

namespace SuperGlue
{
    class Program
    {
        private static readonly IDictionary<string, Func<FluentCommandLineParser, string[], ICommand>> CommandBuilders = new Dictionary<string, Func<FluentCommandLineParser, string[], ICommand>>
        {
            {"run", BuildRunCommand}
        };

        static void Main(string[] args)
        {
            var parser = new FluentCommandLineParser();

            var commandName = args.FirstOrDefault() ?? "";

            if (!CommandBuilders.ContainsKey(commandName))
            {
                Console.WriteLine("{0} isn't a valid command. Available commands: {1}.", commandName, string.Join(", ", CommandBuilders.Select(x => x.Key)));
                return;
            }

            var commandArgs = args.Skip(1).ToArray();

            var command = CommandBuilders[commandName](parser, commandArgs);

            command.Execute();
        }

        private static RunCommand BuildRunCommand(FluentCommandLineParser parser, string[] args)
        {
            return new RunCommand();
        }
    }
}
