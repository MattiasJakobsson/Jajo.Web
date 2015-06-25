using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.ParameterParsing
{
    public interface IFindParameterValue
    {
        Task<object> Find(string parameter, IDictionary<string, object> environment);
    }
}