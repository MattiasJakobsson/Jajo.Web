using System;

namespace SuperGlue.Web.Diagnostics.Profiling
{
    public interface IApplicationProfiling
    {
        Guid Id { get; }
        void AddContext(IProfilingContext context);
    }
}