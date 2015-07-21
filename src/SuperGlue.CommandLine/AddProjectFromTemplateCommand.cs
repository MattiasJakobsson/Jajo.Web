using System;
using System.IO;
using System.Threading.Tasks;
using FubuCsProjFile.Templating.Graph;
using FubuCsProjFile.Templating.Planning;

namespace SuperGlue
{
    public class AddProjectFromTemplateCommand : ICommand
    {
        public string Name { get; set; }
        public string Template { get; set; }
        public string Solution { get; set; }

        public Task Execute()
        {
            var location = Path.GetDirectoryName(Environment.CurrentDirectory);

            var request = new TemplateRequest
            {
                RootDirectory = location,
                SolutionName = Solution,
                Version = "v4.5"
            };

            request.AddProjectRequest(Name, "baseline", project =>
            {
                project.Alterations.Add(Template);

                project.Version = "v4.5";

                project.Substitutions.Set("%PROJECT_NAME%", Name);
                project.Substitutions.Set("%SERVICE_NAME%", Name.Replace(".", ""));
            });

            var library = new TemplateLibrary(string.Format("{0}\\Templates", location));
            var builder = new TemplatePlanBuilder(library);

            var plan = builder.BuildPlan(request);
            plan.Execute();

            return Task.CompletedTask;
        }
    }
}