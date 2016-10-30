using System;
using System.IO;
using System.Threading.Tasks;

namespace SuperGlue.Web.StaticFiles
{
    public class StaticFileOutput
    {
        public StaticFileOutput(Func<Task<Stream>> readContent, string cacheControl)
        {
            CacheControl = cacheControl;
            ReadContent = readContent;
        }

        public Func<Task<Stream>> ReadContent { get; private set; }
        public string CacheControl { get; private set; }
    }
}