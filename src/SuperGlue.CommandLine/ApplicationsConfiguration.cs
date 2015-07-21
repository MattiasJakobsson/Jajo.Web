using System.Collections.Generic;
using System.Linq;

namespace SuperGlue
{
    public class ApplicationsConfiguration
    {
        public ApplicationsConfiguration()
        {
            Groups = new List<GroupConfiguration>();
        }

        public ICollection<GroupConfiguration> Groups { get; private set; }

        public void SetProxyForGroup(string group, int port)
        {
            var currentGroup = GetGroup(group);

            currentGroup.UseProxy = true;
            currentGroup.ProxyPort = port;
        }

        public void AddApplicationTo(string group, string application)
        {
            var currentGroup = GetGroup(group);

            if (!currentGroup.Applications.Contains(application))
                currentGroup.Applications.Add(application);
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
                Applications = new List<string>();
                ProxyPort = 8800;
                Name = "Default";
            }

            public string Name { get; set; }
            public bool UseProxy { get; set; }
            public int ProxyPort { get; set; }
            public ICollection<string> Applications { get; private set; }
        } 
    }
}