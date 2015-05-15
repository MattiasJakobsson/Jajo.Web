using System;

namespace SuperGlue.Diagnostics.Profiling
{
    public class DefaultApplicationProfiling : IApplicationProfiling
    {
        private readonly IProfilingData _profilingData;

        public DefaultApplicationProfiling(IProfilingData profilingData)
        {
            _profilingData = profilingData;
            Id = Guid.NewGuid();
        }

        public Guid Id { get; private set; }

        public void AddContext(IProfilingContext context)
        {
            _profilingData.AddContextFor(Id, context);
        }
    }
}