using System;
using System.IO;
using System.Threading.Tasks;

namespace SuperGlue.Web.StaticFiles
{
    public class ReadResult
    {
        public ReadResult(bool exists, Func<Task<Stream>> read, string name)
        {
            Exists = exists;
            Read = read;
            Name = name;
        }

        public bool Exists { get; }
        public string Name { get; }
        public Func<Task<Stream>> Read { get; }
    }
}