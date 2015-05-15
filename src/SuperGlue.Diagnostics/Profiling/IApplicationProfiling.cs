using System;

namespace SuperGlue.Diagnostics.Profiling
{
    public interface IApplicationProfiling
    {
        Guid Id { get; }
        void AddContext(IProfilingContext context);
    }
}