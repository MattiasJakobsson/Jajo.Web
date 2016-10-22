namespace SuperGlue.Web.StaticFiles
{
    public class StaticFileOutput
    {
        public StaticFileOutput(string filePath, string cacheControl)
        {
            FilePath = filePath;
            CacheControl = cacheControl;
        }

        public string FilePath { get; private set; }
        public string CacheControl { get; private set; }
    }
}