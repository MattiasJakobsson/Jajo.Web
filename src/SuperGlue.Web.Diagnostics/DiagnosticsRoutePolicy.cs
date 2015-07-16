using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SuperGlue.Web.Routing;

namespace SuperGlue.Web.Diagnostics
{
    public class DiagnosticsRoutePolicy : QueryCommandMethodRoutePolicy
    {
        public DiagnosticsRoutePolicy()
            : base(new List<Assembly> { typeof(DiagnosticsRoutePolicy).Assembly })
        {
        }

        protected override IEnumerable<string> GetNamespaceParts(string ns)
        {
            return Enumerable.Empty<string>();
        }

        protected override IEnumerable<IUrlPart> GetInitialUrlParts()
        {
            return new List<IUrlPart>
            {
                new StaticUrlPart("_diagnostics")
            };
        }
    }
}