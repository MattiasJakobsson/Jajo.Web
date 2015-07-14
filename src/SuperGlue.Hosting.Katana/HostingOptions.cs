using System.Collections.Generic;

namespace SuperGlue.Hosting.Katana
{
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