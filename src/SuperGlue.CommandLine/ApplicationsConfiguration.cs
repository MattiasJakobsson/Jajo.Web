using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SuperGlue
{
    public class ApplicationsConfiguration
    {
        public ApplicationsConfiguration()
        {
            Groups = new List<GroupConfiguration>();
            Applications = new List<ApplicationConfiguration>();
        }

        public ICollection<GroupConfiguration> Groups { get; private set; }
        public ICollection<ApplicationConfiguration> Applications { get; private set; }

        public void SetProxyForGroup(string group, int port)
        {
            var currentGroup = GetGroup(group);

            currentGroup.UseProxy = true;
            currentGroup.ProxyPort = port;
        }

        public void AddApplication(string path)
        {
            var configuration = Applications.FirstOrDefault(x => x.Path == path);

            if (configuration != null)
                return;

            configuration = new ApplicationConfiguration
            {
                Name = Guid.NewGuid().ToString(),
                Path = path
            };

            Applications.Add(configuration);
        }

        public void AddApplicationTo(string group, string application)
        {
            var currentGroup = GetGroup(group);

            if (currentGroup.Applications.All(x => x.Path != application))
                currentGroup.Applications.Add(new ApplicationConfiguration
                {
                    Path = application,
                    Name = Guid.NewGuid().ToString()
                });
        }

        public ApplicationConfigurationDetails GetApplicationDetails(string application, string basePath)
        {
            var configuration = Applications.FirstOrDefault(x => x.Path == application);

            return configuration != null ? new ApplicationConfigurationDetails(configuration.Name, configuration.Path, Path.Combine(basePath, "applications")) : null;
        }

        private GroupConfiguration GetGroup(string group)
        {
            var currentGroup = Groups.FirstOrDefault(x => x.Name == group);

            if (currentGroup != null)
                return currentGroup;

            currentGroup = new GroupConfiguration
            {
                Name = @group
            };

            Groups.Add(currentGroup);

            return currentGroup;
        }

        public class GroupConfiguration
        {
            public GroupConfiguration()
            {
                Applications = new List<ApplicationConfiguration>();
                ProxyPort = 8800;
                Name = "Default";
            }

            public string Name { get; set; }
            public bool UseProxy { get; set; }
            public int ProxyPort { get; set; }
            public ICollection<ApplicationConfiguration> Applications { get; private set; }

            public ApplicationConfigurationDetails GetApplicationDetails(string application, string basePath)
            {
                var configuration = Applications.FirstOrDefault(x => x.Path == application);

                return configuration != null ? new ApplicationConfigurationDetails(configuration.Name, configuration.Path, Path.Combine(basePath, "groups", Name)) : null;
            }
        }

        public class ApplicationConfiguration
        {
            public string Name { get; set; }
            public string Path { get; set; }
        }

        public class ApplicationConfigurationDetails
        {
            public ApplicationConfigurationDetails(string name, string path, string destination)
            {
                Name = name;
                Path = path;
                Destination = destination;
            }

            public string Name { get; private set; }
            public string Path { get; private set; }
            public string Destination { get; private set; }
        }
    }
}