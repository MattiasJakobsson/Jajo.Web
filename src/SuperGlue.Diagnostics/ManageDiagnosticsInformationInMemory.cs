using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SuperGlue.Diagnostics
{
    public class ManageDiagnosticsInformationInMemory : IManageDiagnosticsInformation
    {
        private readonly IDictionary<string, IDictionary<string, ConcurrentLruLSet<TimeSpan>>> _messurements = new ConcurrentDictionary<string, IDictionary<string, ConcurrentLruLSet<TimeSpan>>>(); 

        public void AddMessurement(string messurementKey, string key, TimeSpan executionTime)
        {
            if(!_messurements.ContainsKey(messurementKey))
                _messurements[messurementKey] = new ConcurrentDictionary<string, ConcurrentLruLSet<TimeSpan>>();

            if(!_messurements[messurementKey].ContainsKey(key))
                _messurements[messurementKey][key] = new ConcurrentLruLSet<TimeSpan>(100);

            _messurements[messurementKey][key].Push(executionTime);
        }

        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, TimeSpan>> GetAllMessurements()
        {
            return new ReadOnlyDictionary<string, IReadOnlyDictionary<string, TimeSpan>>(_messurements.ToDictionary(x => x.Key, 
                x => (IReadOnlyDictionary<string, TimeSpan>)new ReadOnlyDictionary<string, TimeSpan>(x.Value.ToDictionary(y => y.Key, y => TimeSpan.FromMilliseconds(y.Value.Average(z => z.TotalMilliseconds))))));
        }
    }
}