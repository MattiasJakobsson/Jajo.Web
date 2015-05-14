using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SuperGlue.Web.ModelBinding
{
    public class JsonRequestBodyBindingSource : IBindingSource
    {
        private IDictionary<string, string> _data;

        public Task<IDictionary<string, object>> GetValues(IDictionary<string, object> envinronment)
        {
            SetValues(envinronment);

            return Task.Factory.StartNew(() => (IDictionary<string, object>)_data.ToDictionary(x => x.Key.ToLower(), x => (object)x.Value));
        }

        private void SetValues(IDictionary<string, object> envinronment)
        {
            if (_data != null) 
                return;

            using (var streamReader = new StreamReader(envinronment.GetRequest().Body, Encoding.UTF8, true, 4 * 1024, true))
                _data = JsonConvert.DeserializeObject<Dictionary<string, string>>(streamReader.ReadToEnd()) ?? new Dictionary<string, string>();
        }
    }
}