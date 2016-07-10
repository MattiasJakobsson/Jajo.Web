using System;
using System.Collections.Generic;

namespace SuperGlue.Configuration.Ioc
{
    public interface IResolveServices
    {
        object Resolve(Type service);
        IEnumerable<object> ResolveAll(Type service);
    }
}