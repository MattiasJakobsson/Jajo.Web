using System;
using System.Collections.Generic;

namespace SuperGlue.ApiDiscovery
{
    public class ApiDefinition
    {
        public ApiDefinition()
        {
            Accepts = new List<string>();
        }

        public ApiDefinition(string name, Uri location, IEnumerable<string> accepts)
        {
            Name = name;
            Accepts = accepts;
            Location = location;
        }

        public string Name { get; set; }
        public Uri Location { get; set; }
        public IEnumerable<string> Accepts { get; set; }
    }
}