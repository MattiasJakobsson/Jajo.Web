using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.FileUpload
{
    public interface IUploadFiles
    {
        Task<string> Upload(UploadFile file, IDictionary<string, object> environment);
    }
}