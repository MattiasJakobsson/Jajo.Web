using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Fclp;

namespace SuperGlue
{
    class Program
    {
        private static readonly IDictionary<string, Func<FluentCommandLineParser, string[], ICommand>> CommandBuilders = new Dictionary<string, Func<FluentCommandLineParser, string[], ICommand>>
        {
            {"buildassets", BuildBuildAssetsCommand},
            {"group", BuildGroupCommand},
            {"run", BuildRunCommand},
            {"new", BuildNewCommand},
            {"add", BuildAddCommand}
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

            command.Execute().Wait();
        }

        private static BuildAssetsCommand BuildBuildAssetsCommand(FluentCommandLineParser parser, string[] args)
        {
            var command = new BuildAssetsCommand();

            parser
                .Setup<string>('p', "path")
                .Callback(x => command.AppPath = Path.IsPathRooted(x) ? x : Path.Combine(Environment.CurrentDirectory, x))
                .SetDefault(Environment.CurrentDirectory);

            parser
                .Setup<string>('d', "destination")
                .Callback(x => command.Destination = Path.IsPathRooted(x) ? x : Path.Combine(Environment.CurrentDirectory, x))
                .SetDefault("/_assets");

            parser.Parse(args);

            return command;
        }

        private static ICommand BuildGroupCommand(FluentCommandLineParser parser, string[] args)
        {
            var groupCommands = new Dictionary<string, Func<FluentCommandLineParser, string[], ICommand>>
            {
                {"add", BuildAddApplicationToGroupCommand},
                {"setproxy", BuildSetGroupProxyCommand}
            };

            var commandName = args.FirstOrDefault() ?? "";

            if (!groupCommands.ContainsKey(commandName))
            {
                Console.WriteLine("{0} isn't a valid command for group. Available commands: {1}.", commandName, string.Join(", ", groupCommands.Select(x => x.Key)));
                return null;
            }

            var commandArgs = args.Skip(1).ToArray();

            return groupCommands[commandName](parser, commandArgs);
        }

        private static AddApplicationToGroupCommand BuildAddApplicationToGroupCommand(FluentCommandLineParser parser, string[] args)
        {
            var command = new AddApplicationToGroupCommand();

            parser
                .Setup<string>('g', "group")
                .Callback(x => command.Group = x)
                .SetDefault("Default");

            parser
                .Setup<string>('a', "applicationpath")
                .Callback(x => command.AppPath = Path.IsPathRooted(x) ? x : Path.Combine(Environment.CurrentDirectory, x))
                .SetDefault(Environment.CurrentDirectory);

            parser.Parse(args);

            return command;
        }

        private static SetGroupProxyCommand BuildSetGroupProxyCommand(FluentCommandLineParser parser, string[] args)
        {
            var command = new SetGroupProxyCommand();

            parser
                .Setup<string>('g', "group")
                .Callback(x => command.Group = x)
                .SetDefault("Default");

            parser
                .Setup<int>('p', "port")
                .Callback(x => command.Port = x)
                .SetDefault(8800);

            parser.Parse(args);

            return command;
        }

        private static NewCommand BuildNewCommand(FluentCommandLineParser parser, string[] args)
        {
            var command = new NewCommand();

            parser
                .Setup<string>('n', "name")
                .Callback(x => command.Name = x)
                .Required();

            parser
                .Setup<string>('t', "template")
                .Callback(x => command.Template = x)
                .Required();

            parser
                .Setup<string>('l', "location")
                .Callback(x => command.Location = x)
                .SetDefault(Environment.CurrentDirectory);

            parser
                .Setup<string>('p', "templatepath")
                .Callback(x => command.TemplatePaths.Add(x));

            parser
                .Setup<string>('g', "guid")
                .Callback(x => command.ProjectGuid = x)
                .SetDefault(Guid.NewGuid().ToString());

            parser.Parse(args);

            command.TemplatePaths.Add(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "Templates"));

            return command;
        }

        private static AddCommand BuildAddCommand(FluentCommandLineParser parser, string[] args)
        {
            var command = new AddCommand();

            parser
                .Setup<string>('n', "name")
                .Callback(x => command.Name = x)
                .Required();

            parser
                .Setup<string>('s', "solution")
                .Callback(x => command.Solution = x);

            parser
                .Setup<string>('t', "template")
                .Callback(x => command.Template = x)
                .Required();

            parser
                .Setup<string>('l', "location")
                .Callback(x => command.Location = x)
                .SetDefault(Environment.CurrentDirectory);

            parser
                .Setup<string>('p', "templatepath")
                .Callback(x => command.TemplatePaths.Add(x));

            parser
                .Setup<string>('g', "guid")
                .Callback(x => command.ProjectGuid = x)
                .SetDefault(Guid.NewGuid().ToString());

            parser.Parse(args);

            command.TemplatePaths.Add(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "Templates"));

            return command;
        }

        private static RunCommand BuildRunCommand(FluentCommandLineParser parser, string[] args)
        {
            var command = new RunCommand();

            parser
                .Setup<string>('a', "application")
                .Callback(x => command.Application = x)
                .SetDefault(Environment.CurrentDirectory);

            parser
                .Setup<string>('e', "environment")
                .Callback(x => command.Environment = x)
                .SetDefault("local");

            parser.Parse(args);

            return command;
        }
    }
}
