using System;
using System.Collections.Generic;

namespace SuperGlue.Diagnostics.Profiling
{
    public class ProfilingRow
    {
        public ProfilingRow(DateTime at, double durationMilliseconds, string status, IDictionary<string, string> data)
        {
            Data = data;
            Status = status;
            DurationMilliseconds = durationMilliseconds;
            At = at;
        }

        public DateTime At { get; private set; }
        public double DurationMilliseconds { get; private set; }
        public string Status { get; private set; }
        public IDictionary<string, string> Data { get; private set; }
    }
}