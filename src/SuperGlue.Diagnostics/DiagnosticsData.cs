using System.Collections.Generic;

namespace SuperGlue.Diagnostics
{
    public class DiagnosticsData
    {
        public DiagnosticsData(string key, IReadOnlyDictionary<string, IDiagnosticsValue> data)
        {
            Key = key;
            Data = data;
        }

        public string Key { get; private set; }
        public IReadOnlyDictionary<string, IDiagnosticsValue> Data { get; private set; }
    }
}