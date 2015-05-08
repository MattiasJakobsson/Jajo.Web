namespace OpenWeb.Output.Spark
{
    public class FileFound
    {
        public FileFound(string path, string root, string directory)
        {
            Path = path;
            Root = root;
            Directory = directory;
        }

        public string Path { get; private set; }
        public string Root { get; private set; }
        public string Directory { get; private set; }

        public string GetFileName()
        {
            return System.IO.Path.GetFileName(Path);
        }
    }
}