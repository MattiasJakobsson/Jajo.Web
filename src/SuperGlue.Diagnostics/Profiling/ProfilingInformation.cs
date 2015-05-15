using System.Collections.Generic;

namespace SuperGlue.Diagnostics.Profiling
{
    public class ProfilingInformation
    {
        public ProfilingInformation(string context, IEnumerable<ProfilingRow> rows)
        {
            Rows = rows;
            Context = context;
        }

        public string Context { get; private set; }
        public IEnumerable<ProfilingRow> Rows { get; private set; }
    }
}