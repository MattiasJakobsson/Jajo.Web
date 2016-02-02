using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.ContentParsing
{
    public interface ITextParser
    {
        Task<string> Parse(IDictionary<string, object> environment, string text, Func<string, Task<string>> recurse);
    }
}