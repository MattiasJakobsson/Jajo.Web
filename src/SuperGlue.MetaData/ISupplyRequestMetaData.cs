using System.Collections.Generic;

namespace SuperGlue.MetaData
{
    public interface ISupplyRequestMetaData
    {
        bool CanHandleChain(string chain);
        IDictionary<string, object> GetMetaData(IDictionary<string, object> environment);
    }
}