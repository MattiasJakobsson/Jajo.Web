using System.IO;

namespace SuperGlue.FileUpload
{
    public class UploadFile
    {
        public UploadFile(Stream data, string name, string contentType)
        {
            ContentType = contentType;
            Name = name;
            Data = data;
        }

        public Stream Data { get; private set; }
        public string Name { get; private set; }
        public string ContentType { get; private set; }
    }
}