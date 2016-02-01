namespace SuperGlue.FileUpload
{
    public class FileUploadSettings
    {
        public string UploadPath { get; private set; }

        public FileUploadSettings UploadTo(string path)
        {
            UploadPath = path;

            return this;
        }
    }
}