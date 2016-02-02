namespace SuperGlue.Web.StaticFiles
{
    public class StaticFileOutput
    {
        public StaticFileOutput(string filePath)
        {
            FilePath = filePath;
        }

        public string FilePath { get; private set; } 
    }
}