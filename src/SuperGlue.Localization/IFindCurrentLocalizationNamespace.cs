using System.Collections.Generic;

namespace SuperGlue.Localization
{
    public interface IFindCurrentLocalizationNamespace
    {
        string Find(IDictionary<string, object> environment);
    }
}