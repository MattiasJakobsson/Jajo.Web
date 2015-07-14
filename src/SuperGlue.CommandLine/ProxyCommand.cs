using System.Collections.Generic;
using System.IO;
using SuperGlue.Configuration;

namespace SuperGlue
{
    public class ProxyCommand : ICommand
    {
        public string AppPath { get; set; }
        public IEnumerable<string> Bindings { get; set; }

        public void Execute()
        {
            
        }

        private IEnumerable<HostingOptions> FindApplicationsToRun(string fromLocation)
        {
            var files = Directory.GetFiles(fromLocation, ".hostingoptions");

            foreach (var file in files)
            {
                var options = Files.ReadAsJson<HostingOptions>(file).Result;

                if (options != null)
                    yield return options;
            }

            foreach (var child in Directory.GetDirectories(fromLocation))
            {
                foreach (var option in FindApplicationsToRun(child))
                    yield return option;
            }
        }

        public class HostingOptions
        {
            public HostingOptions()
            {
                Bindings = new List<string>();
            }

            public ICollection<string> Bindings { get; set; }
            public string ApplicationName { get; set; }
        }
    }
}