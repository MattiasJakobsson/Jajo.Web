using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JaJo.Projects.Templating;

namespace SuperGlue
{
    public class AddCommand : ICommand
    {
        public string Name { get; set; }
        public string Solution { get; set; }
        public string Template { get; set; }
        public ICollection<string> TemplatePaths { get; set; }
        public bool CreateTestProject { get; set; }
        public string Location { get; set; }

        public async Task Execute()
        {
            var engine = TemplatingEngine.Init();

            var projectDirectory = TemplatePaths
                .Select(x => Path.Combine(x, $"projects\\{Template}"))
                .FirstOrDefault(Directory.Exists);

            var substitutions = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
                {"PROJECT_NAME", Name},
                {"SOLUTION_NAME", Name}
            });

            if (!string.IsNullOrEmpty(projectDirectory))
            {
                await engine.RunTemplate(new ProjectTemplateType(Name, Location, Path.Combine(Location, $"src\\{Name}"), substitutions), projectDirectory);

                var alterationDirectories = TemplatePaths
                    .Select(x => Path.Combine(x, $"alterations\\{Template}"))
                    .Where(Directory.Exists)
                    .ToList();

                foreach (var alterationDirectory in alterationDirectories)
                    await engine.RunTemplate(new AlterationTemplateType(Name, Location, Path.Combine($"src\\{Name}"), substitutions), alterationDirectory);
            }

            if (!CreateTestProject)
                return;

            var testSubstitutions = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
                {"PROJECT_NAME", $"{Name}.Tests"},
                {"SOLUTION_NAME", Name},
                {"FOR_PROJECT", Name}
            });

            var testDirectory = TemplatePaths
                .Select(x => Path.Combine(x, $"projects\\{Template}Test"))
                .FirstOrDefault(Directory.Exists);

            if (!string.IsNullOrEmpty(testDirectory))
            {
                await engine.RunTemplate(new ProjectTemplateType($"{Name}.Tests", Location, Path.Combine(Location, $"src\\{Name}.Tests"), testSubstitutions), testDirectory);

                var alterationDirectories = TemplatePaths
                    .Select(x => Path.Combine(x, $"alterations\\{Template}Test"))
                    .Where(Directory.Exists)
                    .ToList();

                foreach (var alterationDirectory in alterationDirectories)
                    await engine.RunTemplate(new AlterationTemplateType($"{Name}.Tests", Location, Path.Combine($"src\\{Name}.Tests"), testSubstitutions), alterationDirectory);
            }
        }
    }
}