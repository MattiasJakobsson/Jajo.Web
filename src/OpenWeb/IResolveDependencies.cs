using System;
using System.Collections.Generic;

namespace OpenWeb
{
    public interface IResolveDependencies
    {
        TService Resolve<TService>();
        object Resolve(Type serviceType);
        IEnumerable<TService> ResolveAll<TService>();
    }
}