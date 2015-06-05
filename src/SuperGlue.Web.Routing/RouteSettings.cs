using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SuperGlue.Web.Routing
{
    public class RouteSettings
    {
        private readonly ICollection<IRoutePolicy> _policies = new List<IRoutePolicy>();

        public RouteSettings UsePolicy(IRoutePolicy policy)
        {
            _policies.Add(policy);

            return this;
        }

        internal IReadOnlyCollection<IRoutePolicy> GetPolicies()
        {
            return new ReadOnlyCollection<IRoutePolicy>(_policies.ToList());
        }
    }
}