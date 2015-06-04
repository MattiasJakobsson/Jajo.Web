using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SuperGlue.Web.Routing.Superscribe.Policies;

namespace SuperGlue.Web.Routing.Superscribe
{
    public class SuperscribeRouteSettings
    {
        private readonly ICollection<IRoutePolicy> _policies = new List<IRoutePolicy>();

        public SuperscribeRouteSettings UsePolicy(IRoutePolicy policy)
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