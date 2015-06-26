using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SuperGlue.MetaData
{
    public static class MetaDataEnvironmentExtensions
    {
        public static class MetaDataConstants
        {
            public const string RequestMetaData = "superglue.RequestMetaData";
        }

        public static RequestMetaData GetMetaData(this IDictionary<string, object> environment)
        {
            return environment.Get(MetaDataConstants.RequestMetaData, new RequestMetaData(new ReadOnlyDictionary<string, object>(new Dictionary<string, object>())));
        }
    }
}