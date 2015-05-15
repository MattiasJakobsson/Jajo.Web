namespace SuperGlue.Diagnostics.Profiling
{
    public interface IProfilingContext
    {
        string Name { get; }

        ProfilingInformation GetProfilingInformation();
    }
}