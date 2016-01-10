using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JaJo.Projects.Templating;

namespace SuperGlue
{
    public class NewCommand : ICommand
    {
        public NewCommand()
        {
            TemplatePaths = new List<string>();
        }

        public string Name { get; set; }
        public string Template { get; set; }
        public ICollection<string> TemplatePaths { get; set; }
        public string Location { get; set; }
        public string ProjectGuid { get; set; }

        public async Task Execute()
        {
            var engine = TemplatingEngine.Init();

            var baseDirectory = TemplatePaths
                .Select(x => Path.Combine(x, "solutions\\base"))
                .FirstOrDefault(Directory.Exists);

            if (string.IsNullOrEmpty(baseDirectory))
                return;

            var substitutions = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
                {"PROJECT_NAME", Name},
                {"SOLUTION_NAME", Name},
                {"PROJECT_GUID", ProjectGuid}
            });

            await engine.RunTemplate(new SolutionTemplateType(Name, Location, substitutions), baseDirectory);

            await new AddCommand
            {
                Name = Name,
                Location = Location,
                TemplatePaths = TemplatePaths,
                Template = Template,
                Solution = Name
            }.Execute();
        }
    }
}