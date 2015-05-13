using System;
using System.IO;

namespace SuperGlue.Web.Output.Spark
{
    public class Template
    {
        public Template(string name, string path, string pathSeperator, Type modelType, Func<TextReader> contents)
        {
            Name = name;
            Path = path;
            PathSeperator = pathSeperator;
            ModelType = modelType;
            Contents = contents;
        }

        public string Path { get; private set; }
        public string PathSeperator { get; private set; }
        public Func<TextReader> Contents { get; private set; }
        public Type ModelType { get; private set; }
        public string Name { get; private set; }
    }
}