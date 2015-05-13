using System;
using System.Collections.Generic;
using System.Reflection;

namespace SuperGlue.Web.Routing.Superscribe.Conventional
{
    public interface IRoutePolicy
    {
        bool Matches(MethodInfo endpoint);
        void Build(MethodInfo endpoint, IRouteBuilder builder);
        IEnumerable<RouteParameter> GetAvailableRouteParameters(Type input);
    }
}