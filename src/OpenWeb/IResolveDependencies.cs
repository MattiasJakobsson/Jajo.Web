using System;

namespace OpenWeb
{
    public interface IResolveDependencies
    {
        TService Resolve<TService>();
        object Resolve(Type serviceType);
    }
}