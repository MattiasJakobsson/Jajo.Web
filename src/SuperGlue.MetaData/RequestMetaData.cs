using System.Collections.Generic;

namespace SuperGlue.MetaData
{
    public class RequestMetaData
    {
        public RequestMetaData(IReadOnlyDictionary<string, object> metaData)
        {
            MetaData = metaData;
        }

        public IReadOnlyDictionary<string, object> MetaData { get; }

        public object Get(string key)
        {
            return !MetaData.ContainsKey(key) ? null : MetaData[key];
        }
    }
}