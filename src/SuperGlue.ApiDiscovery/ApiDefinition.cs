using System;
using System.Collections.Generic;

namespace SuperGlue.ApiDiscovery
{
    public class ApiDefinition
    {
        public ApiDefinition(string name, Uri location, IEnumerable<string> accepts)
        {
            Name = name;
            Accepts = accepts;
            Location = location;
        }

        public string Name { get; private set; }
        public Uri Location { get; private set; }
        public IEnumerable<string> Accepts { get; private set; }
    }
}